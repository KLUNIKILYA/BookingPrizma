using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Legacy;
using BookingSystem.Shared;
using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Брони поверх боевой dbo.ZoneReservation + собственные Booking_ResExtra/Booking_ResService.
/// Создаём/редактируем/удаляем только свои брони (OrderID IS NULL); кассовые (OrderID != null) — только чтение.
/// </summary>
public class BookingService : IBookingService
{
    private readonly BookingDbContext _db;

    public BookingService(BookingDbContext db) => _db = db;

    public async Task<List<BookingEventDto>> GetBookingsAsync(
        DateTime from, DateTime to, int? resourceId, int? groupId, CancellationToken ct = default)
    {
        var q = _db.ZoneReservations.AsNoTracking()
            .Where(r => r.DateFrom < to && r.DateTo > from);
        if (resourceId.HasValue)
            q = q.Where(r => r.ZoneId == resourceId.Value);

        var reservations = await q.ToListAsync(ct);
        if (reservations.Count == 0) return new List<BookingEventDto>();

        var ids = reservations.Select(r => r.Id).ToList();

        var extras = await _db.ResExtras.AsNoTracking()
            .Where(e => ids.Contains(e.ReservationId))
            .ToDictionaryAsync(e => e.ReservationId, ct);

        var svcRows = await _db.ResServices.AsNoTracking()
            .Where(s => ids.Contains(s.ReservationId))
            .ToListAsync(ct);
        var svcByRes = svcRows.GroupBy(s => s.ReservationId)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.SortOrder).ToList());

        var clientIds = extras.Values.Where(e => e.ClientVisitorId.HasValue)
            .Select(e => e.ClientVisitorId!.Value).Distinct().ToList();
        var clientNames = clientIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.CashboxVisitors.AsNoTracking().Where(v => clientIds.Contains(v.IdVisitor))
                .ToDictionaryAsync(v => v.IdVisitor, v => (v.Surname + " " + v.Name).Trim(), ct);

        var waiterIds = extras.Values.Where(e => e.WaiterLoginId.HasValue)
            .Select(e => e.WaiterLoginId!.Value).Distinct().ToList();
        var waiterNames = waiterIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.TLogins.AsNoTracking().Where(t => waiterIds.Contains(t.Fid))
                .ToDictionaryAsync(t => t.Fid, t => t.Fuser ?? "", ct);

        return reservations
            .Select(r => Map(r,
                extras.GetValueOrDefault(r.Id),
                svcByRes.GetValueOrDefault(r.Id),
                clientNames, waiterNames))
            .ToList();
    }

    public async Task<BookingEventDto> CreateAsync(BookingUpsertRequest request, CancellationToken ct = default)
    {
        var end = ResolveEnd(request);
        var conflicts = await CheckConflictsAsync(null, new[] { request.ResourceId }, request.TimeFrom, end, ct);
        if (conflicts.Count > 0) throw new BookingConflictException(conflicts);

        var now = DateTime.Now;
        var reservation = new ZoneReservation
        {
            ZoneId = request.ResourceId,
            DateFrom = request.TimeFrom,
            DateTo = ResolveEnd(request),
            Info = NormalizeNote(request.Note),
            OrderId = null // своя бронь, не привязана к кассовому заказу
        };
        _db.ZoneReservations.Add(reservation);
        await _db.SaveChangesAsync(ct); // получить identity ID

        var extra = new BookingResExtra
        {
            ReservationId = reservation.Id,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            ClientVisitorId = request.ClientVisitorId,
            WaiterLoginId = request.WaiterVisitorId,
            Label = request.Label,
            CelebrantName = string.IsNullOrWhiteSpace(request.CelebrantName) ? null : request.CelebrantName.Trim(),
            CelebrantBirthDate = request.CelebrantBirthDate,
            IsPrepaid = request.IsPrepaid,
            PrepaidAmount = request.IsPrepaid ? request.PrepaidAmount : null,
            CreatedAt = now,
            UpdatedAt = now
        };
        await ApplyTariffAsync(extra, reservation.ZoneId, reservation.DateFrom, reservation.DateTo, ct);
        _db.ResExtras.Add(extra);
        await AddServicesAsync(reservation.Id, request.Services, ct);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    public async Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default)
    {
        var reservation = await _db.ZoneReservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return null;
        // OrderID не трогаем — связь брони с кассовым заказом сохраняется.

        var conflicts = await CheckConflictsAsync(id, new[] { request.ResourceId }, request.TimeFrom, ResolveEnd(request), ct);
        if (conflicts.Count > 0) throw new BookingConflictException(conflicts);

        reservation.ZoneId = request.ResourceId;
        reservation.DateFrom = request.TimeFrom;
        reservation.DateTo = ResolveEnd(request);
        reservation.Info = NormalizeNote(request.Note);

        var extra = await _db.ResExtras.FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        if (extra is null)
        {
            extra = new BookingResExtra { ReservationId = id, CreatedAt = DateTime.Now };
            _db.ResExtras.Add(extra);
        }
        extra.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        extra.ClientVisitorId = request.ClientVisitorId;
        extra.WaiterLoginId = request.WaiterVisitorId;
        extra.Label = request.Label;
        extra.CelebrantName = string.IsNullOrWhiteSpace(request.CelebrantName) ? null : request.CelebrantName.Trim();
        extra.CelebrantBirthDate = request.CelebrantBirthDate;
        extra.IsPrepaid = request.IsPrepaid;
        extra.PrepaidAmount = request.IsPrepaid ? request.PrepaidAmount : null;
        await ApplyTariffAsync(extra, reservation.ZoneId, reservation.DateFrom, reservation.DateTo, ct);
        extra.UpdatedAt = DateTime.Now;

        var oldServices = await _db.ResServices.Where(s => s.ReservationId == id).ToListAsync(ct);
        _db.ResServices.RemoveRange(oldServices);
        await AddServicesAsync(id, request.Services, ct);

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _db.ZoneReservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return false;
        // Удаляем только бронь (+ companion). Кассовый заказ (OrderID) не трогаем.

        var services = await _db.ResServices.Where(s => s.ReservationId == id).ToListAsync(ct);
        _db.ResServices.RemoveRange(services);
        var extra = await _db.ResExtras.FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        if (extra is not null) _db.ResExtras.Remove(extra);
        _db.ZoneReservations.Remove(reservation);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<BookingConflictDto>> CheckConflictsAsync(
        int? excludeId, IReadOnlyCollection<int> resourceIds, DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (resourceIds is null || resourceIds.Count == 0 || to <= from)
            return new List<BookingConflictDto>();

        var ids = resourceIds.Where(i => i > 0).Distinct().ToList();
        if (ids.Count == 0) return new List<BookingConflictDto>();

        // Пересечение: существующая.DateFrom < новая.DateTo И существующая.DateTo > новая.DateFrom.
        // Касание границами (10:00–11:00 и 11:00–12:00) пересечением не считается.
        var q = _db.ZoneReservations.AsNoTracking()
            .Where(r => ids.Contains(r.ZoneId) && r.DateFrom < to && r.DateTo > from);
        if (excludeId.HasValue)
            q = q.Where(r => r.Id != excludeId.Value);

        var rows = await q.ToListAsync(ct);
        if (rows.Count == 0) return new List<BookingConflictDto>();

        var roomNames = await _db.Zones.AsNoTracking()
            .Where(z => ids.Contains(z.IdZone))
            .ToDictionaryAsync(z => z.IdZone, z => z.NameZone, ct);

        var resIds = rows.Select(r => r.Id).ToList();
        var titles = await _db.ResExtras.AsNoTracking()
            .Where(e => resIds.Contains(e.ReservationId))
            .ToDictionaryAsync(e => e.ReservationId, e => e.Title, ct);

        return rows.Select(r => new BookingConflictDto
        {
            ResourceId = r.ZoneId,
            ResourceName = roomNames.GetValueOrDefault(r.ZoneId, ""),
            ExistingId = r.Id,
            TimeFrom = r.DateFrom,
            TimeTo = r.DateTo,
            Title = titles.GetValueOrDefault(r.Id) ?? (r.OrderId != null ? $"Касса #{r.OrderId}" : null)
        }).ToList();
    }

    public async Task<AvailabilityDto> GetAvailabilityAsync(
        DateTime from, DateTime to, int? excludeId, CancellationToken ct = default)
    {
        var result = new AvailabilityDto();
        if (to <= from) return result;

        var q = _db.ZoneReservations.AsNoTracking()
            .Where(r => r.DateFrom < to && r.DateTo > from);
        if (excludeId.HasValue) q = q.Where(r => r.Id != excludeId.Value);

        var rows = await q.ToListAsync(ct);
        if (rows.Count == 0) return result;

        var roomIds = rows.Select(r => r.ZoneId).Distinct().ToList();
        var roomNames = await _db.Zones.AsNoTracking()
            .Where(z => roomIds.Contains(z.IdZone))
            .ToDictionaryAsync(z => z.IdZone, z => z.NameZone, ct);

        var resIds = rows.Select(r => r.Id).ToList();
        var extras = await _db.ResExtras.AsNoTracking()
            .Where(e => resIds.Contains(e.ReservationId))
            .ToDictionaryAsync(e => e.ReservationId, ct);

        var clientIds = extras.Values.Where(e => e.ClientVisitorId.HasValue)
            .Select(e => e.ClientVisitorId!.Value).Distinct().ToList();
        var clientNames = clientIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.CashboxVisitors.AsNoTracking().Where(v => clientIds.Contains(v.IdVisitor))
                .ToDictionaryAsync(v => v.IdVisitor, v => (v.Surname + " " + v.Name).Trim(), ct);

        var waiterIds = extras.Values.Where(e => e.WaiterLoginId.HasValue)
            .Select(e => e.WaiterLoginId!.Value).Distinct().ToList();
        var waiterNames = waiterIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.TLogins.AsNoTracking().Where(t => waiterIds.Contains(t.Fid))
                .ToDictionaryAsync(t => t.Fid, t => t.Fuser ?? "", ct);

        string? TitleOf(ZoneReservation r)
        {
            var ex = extras.GetValueOrDefault(r.Id);
            if (ex?.ClientVisitorId is int cid && clientNames.TryGetValue(cid, out var cn) && !string.IsNullOrWhiteSpace(cn))
                return cn;
            if (!string.IsNullOrWhiteSpace(ex?.Title)) return ex!.Title;
            return r.OrderId != null ? $"Касса #{r.OrderId}" : null;
        }

        foreach (var r in rows)
        {
            result.Rooms.Add(new BusySlotDto
            {
                Id = r.ZoneId,
                Name = roomNames.GetValueOrDefault(r.ZoneId, ""),
                TimeFrom = r.DateFrom,
                TimeTo = r.DateTo,
                Title = TitleOf(r)
            });

            if (extras.GetValueOrDefault(r.Id)?.WaiterLoginId is int wid)
                result.Waiters.Add(new BusySlotDto
                {
                    Id = wid,
                    Name = waiterNames.GetValueOrDefault(wid, ""),
                    TimeFrom = r.DateFrom,
                    TimeTo = r.DateTo,
                    Title = TitleOf(r)
                });
        }

        return result;
    }

    // ===== helpers =====

    private static DateTime ResolveEnd(BookingUpsertRequest req) =>
        req.TimeToOverride.HasValue && req.TimeToOverride.Value > req.TimeFrom
            ? req.TimeToOverride.Value
            : req.TimeFrom.AddHours(1);

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private async Task AddServicesAsync(int reservationId, List<BookingServiceSelection> selections, CancellationToken ct)
    {
        if (selections.Count == 0) return;

        var serviceIds = selections.Where(s => !s.IsTicket).Select(s => s.ServiceId).Distinct().ToList();
        var ticketIds = selections.Where(s => s.IsTicket).Select(s => s.ServiceId).Distinct().ToList();

        var services = serviceIds.Count == 0
            ? new List<SingleService>()
            : await _db.SingleServices.AsNoTracking().Where(s => serviceIds.Contains(s.Id)).ToListAsync(ct);
        var svcMap = services.ToDictionary(s => s.Id);

        var tickets = ticketIds.Count == 0
            ? new List<Ticket>()
            : await _db.Tickets.AsNoTracking().Where(t => ticketIds.Contains(t.IdTicket)).ToListAsync(ct);
        var tikMap = tickets.ToDictionary(t => t.IdTicket);

        var order = 0;
        foreach (var sel in selections)
        {
            string name;
            decimal price;
            if (sel.IsTicket)
            {
                if (!tikMap.TryGetValue(sel.ServiceId, out var tk)) continue;
                name = tk.NameTicket;
                price = tk.TotalPrice;
            }
            else
            {
                if (!svcMap.TryGetValue(sel.ServiceId, out var sv)) continue;
                name = sv.Name;
                price = sv.Price;
            }

            _db.ResServices.Add(new BookingResServiceItem
            {
                ReservationId = reservationId,
                ServiceId = sel.ServiceId,
                ServiceName = name,
                PriceSnapshot = price,
                Quantity = sel.Quantity < 1 ? 1 : sel.Quantity,
                IsTicket = sel.IsTicket,
                SortOrder = order++,
                IsDone = sel.IsDone
            });
        }
    }

    /// <summary>
    /// Автоматически подбирает тариф брони по комнате и длительности (менеджер тариф не выбирает).
    /// Берётся самый дешёвый тариф, чья длительность покрывает бронь (округление вверх до тарифа);
    /// если бронь длиннее всех тарифов — самый длинный.
    /// </summary>
    private async Task ApplyTariffAsync(BookingResExtra extra, int zoneId, DateTime from, DateTime to, CancellationToken ct)
    {
        extra.TariffTicketId = null;
        extra.TariffName = null;
        extra.TariffPrice = null;

        var minutes = (int)Math.Round((to - from).TotalMinutes);
        if (minutes <= 0) return;

        var tariffs = await (from tz in _db.TicketZones.AsNoTracking()
                             join t in _db.Tickets.AsNoTracking() on tz.IdTicket equals t.IdTicket
                             where tz.IdZone == zoneId && tz.Reservation && t.Active
                             select new { t.IdTicket, t.NameTicket, t.TotalPrice, tz.ReservationTime })
                            .ToListAsync(ct);
        if (tariffs.Count == 0) return;

        var chosen = tariffs
            .Where(x => x.ReservationTime >= minutes)
            .OrderBy(x => x.ReservationTime).ThenBy(x => x.TotalPrice)
            .FirstOrDefault()
            ?? tariffs.OrderByDescending(x => x.ReservationTime).ThenBy(x => x.TotalPrice).First();

        extra.TariffTicketId = chosen.IdTicket;
        extra.TariffName = chosen.NameTicket;
        extra.TariffPrice = chosen.TotalPrice;
    }

    private async Task<BookingEventDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var r = await _db.ZoneReservations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return null;

        var extra = await _db.ResExtras.AsNoTracking().FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        var svc = await _db.ResServices.AsNoTracking()
            .Where(s => s.ReservationId == id).OrderBy(s => s.SortOrder).ToListAsync(ct);

        var clientNames = new Dictionary<int, string>();
        if (extra?.ClientVisitorId is int cid)
        {
            var name = await _db.CashboxVisitors.AsNoTracking()
                .Where(v => v.IdVisitor == cid).Select(v => (v.Surname + " " + v.Name).Trim())
                .FirstOrDefaultAsync(ct);
            if (name is not null) clientNames[cid] = name;
        }
        var waiterNames = new Dictionary<int, string>();
        if (extra?.WaiterLoginId is int wid)
        {
            var name = await _db.TLogins.AsNoTracking()
                .Where(t => t.Fid == wid).Select(t => t.Fuser ?? "").FirstOrDefaultAsync(ct);
            if (name is not null) waiterNames[wid] = name;
        }

        return Map(r, extra, svc, clientNames, waiterNames);
    }

    private static BookingEventDto Map(
        ZoneReservation r,
        BookingResExtra? ex,
        List<BookingResServiceItem>? svc,
        IReadOnlyDictionary<int, string> clientNames,
        IReadOnlyDictionary<int, string> waiterNames)
    {
        var label = ex?.Label ?? BookingLabel.None;

        var title = ex?.Title;
        if (string.IsNullOrWhiteSpace(title))
            title = r.OrderId != null ? $"Касса #{r.OrderId}" : "(бронь)";

        string? clientName = null;
        if (ex?.ClientVisitorId is int cid) clientNames.TryGetValue(cid, out clientName);
        string? waiterName = null;
        if (ex?.WaiterLoginId is int wid) waiterNames.TryGetValue(wid, out waiterName);

        var services = (svc ?? new List<BookingResServiceItem>())
            .Select(s => new BookingServiceLineDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Price = s.PriceSnapshot,
                Quantity = s.Quantity < 1 ? 1 : s.Quantity,
                IsDone = s.IsDone,
                IsTicket = s.IsTicket
            }).ToList();

        return new BookingEventDto
        {
            Id = r.Id,
            ResourceId = r.ZoneId,
            Title = title,
            TimeFrom = r.DateFrom,
            TimeTo = r.DateTo,
            Label = label,
            LabelName = BookingLabelInfo.Name(label),
            Color = BookingLabelInfo.Color(label),
            Note = r.Info,
            ClientVisitorId = ex?.ClientVisitorId,
            ClientName = clientName,
            WaiterVisitorId = ex?.WaiterLoginId,
            WaiterName = waiterName,
            Services = services,
            TariffTicketId = ex?.TariffTicketId,
            TariffName = ex?.TariffName,
            TariffPrice = ex?.TariffPrice,
            CelebrantName = ex?.CelebrantName,
            CelebrantBirthDate = ex?.CelebrantBirthDate,
            IsPrepaid = ex?.IsPrepaid ?? false,
            PrepaidAmount = ex?.PrepaidAmount,
            TotalPrice = services.Sum(s => s.Price * s.Quantity) + (ex?.TariffPrice ?? 0m),
            CanEdit = true   // тестовая БД — редактируем все брони, включая кассовые
        };
    }
}
