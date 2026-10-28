using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Legacy;
using BookingSystem.Shared;
using BookingSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

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

        var clients = clientIds.Count == 0
            ? new Dictionary<int, (string Name, string? Info)>()
            : await _db.CashboxVisitors.AsNoTracking()
                .Where(v => clientIds.Contains(v.IdVisitor))
                .Select(v => new { v.IdVisitor, Name = (v.Surname + " " + v.Name).Trim(), v.Info })
                .ToDictionaryAsync(x => x.IdVisitor, x => (x.Name, x.Info), ct);

        var clientNames = clients.ToDictionary(c => c.Key, c => c.Value.Name);
        var clientInfos = clients.ToDictionary(c => c.Key, c => c.Value.Info);

        var waiterIds = extras.Values.Where(e => e.WaiterLoginId.HasValue)
            .Select(e => e.WaiterLoginId!.Value).Distinct().ToList();
        var waiterNames = waiterIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.TLogins.AsNoTracking().Where(t => waiterIds.Contains(t.Fid))
                .ToDictionaryAsync(t => t.Fid, t => t.Fuser ?? "", ct);

        var labels = await LoadLabelsAsync(ct);

        return reservations
            .Select(r => Map(r,
                extras.GetValueOrDefault(r.Id),
                svcByRes.GetValueOrDefault(r.Id),
                clientNames, waiterNames, labels, clientInfos))
            .ToList();
    }

    private async Task<Dictionary<int, LabelDto>> LoadLabelsAsync(CancellationToken ct) =>
        await _db.Labels.AsNoTracking().ToDictionaryAsync(
            l => l.Id,
            l => new LabelDto { Id = l.Id, Name = l.Name, Color = l.Color },
            ct);

    public async Task<List<PrepaymentTypeDto>> GetPrepaymentTypesAsync() =>
    await _db.PrepaymentTypes.AsNoTracking()
        .OrderBy(p => p.Id)
        .Select(p => new PrepaymentTypeDto { Id = p.Id, Name = p.Name })
        .ToListAsync();

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
            OrderId = null
        };
        _db.ZoneReservations.Add(reservation);
        await _db.SaveChangesAsync(ct);

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
            PrepaymentTypeId = request.IsPrepaid ? request.PrepaymentTypeId : null,
            GuestsCount = NormalizeGuests(request.GuestsCount),
            CreatedAt = now,
            UpdatedAt = now
        };
        await ApplyTariffAsync(extra, reservation.ZoneId, reservation.DateFrom, reservation.DateTo, ct);
        _db.ResExtras.Add(extra);
        await AddServicesAsync(reservation.Id, request.Services, ct);
        await UpdateClientInfoAsync(request.ClientVisitorId, request.ClientInfo, ct);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    public async Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default)
    {
        var reservation = await _db.ZoneReservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return null;

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
        extra.PrepaymentTypeId = request.IsPrepaid ? request.PrepaymentTypeId : null;
        await ApplyTariffAsync(extra, reservation.ZoneId, reservation.DateFrom, reservation.DateTo, ct);
        extra.GuestsCount = NormalizeGuests(request.GuestsCount);
        extra.UpdatedAt = DateTime.Now;

        await UpdateClientInfoAsync(request.ClientVisitorId, request.ClientInfo, ct);

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

        var services = await _db.ResServices.Where(s => s.ReservationId == id).ToListAsync(ct);
        _db.ResServices.RemoveRange(services);
        var extra = await _db.ResExtras.FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        if (extra is not null) _db.ResExtras.Remove(extra);
        _db.ZoneReservations.Remove(reservation);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BookingEventDto?> CancelAsync(int id, CancellationToken ct = default)
    {
        var extra = await _db.ResExtras.FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        if (extra is null)
        {
            var exists = await _db.ZoneReservations.AsNoTracking().AnyAsync(r => r.Id == id, ct);
            if (!exists) return null;
            extra = new BookingResExtra { ReservationId = id, CreatedAt = DateTime.Now };
            _db.ResExtras.Add(extra);
        }
        extra.IsCancelled = true;
        extra.CancelledAt = DateTime.Now;
        extra.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<BookingEventDto?> RestoreAsync(int id, CancellationToken ct = default)
    {
        var extra = await _db.ResExtras.FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        if (extra is null || !extra.IsCancelled) return await GetByIdAsync(id, ct);

        var reservation = await _db.ZoneReservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return null;

        var conflicts = await CheckConflictsAsync(id, new[] { reservation.ZoneId }, reservation.DateFrom, reservation.DateTo, ct);
        if (conflicts.Count > 0) throw new BookingConflictException(conflicts);

        extra.IsCancelled = false;
        extra.CancelledAt = null;
        extra.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<List<BookingConflictDto>> CheckConflictsAsync(
        int? excludeId, IReadOnlyCollection<int> resourceIds, DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (resourceIds is null || resourceIds.Count == 0 || to <= from)
            return new List<BookingConflictDto>();

        var ids = resourceIds.Where(i => i > 0).Distinct().ToList();
        if (ids.Count == 0) return new List<BookingConflictDto>();

        var q = _db.ZoneReservations.AsNoTracking()
            .Where(r => ids.Contains(r.ZoneId) && r.DateFrom < to && r.DateTo > from)
            .Where(r => !_db.ResExtras.Any(e => e.ReservationId == r.Id && e.IsCancelled));
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
            .Where(r => r.DateFrom < to && r.DateTo > from)
            .Where(r => !_db.ResExtras.Any(e => e.ReservationId == r.Id && e.IsCancelled));
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


    private static DateTime ResolveEnd(BookingUpsertRequest req) =>
        req.TimeToOverride.HasValue && req.TimeToOverride.Value > req.TimeFrom
            ? req.TimeToOverride.Value
            : req.TimeFrom.AddHours(1);

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private static string? NormalizeGuests(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        v = v.Trim();
        return v.Length > 50 ? v[..50] : v;
    }

    private async Task UpdateClientInfoAsync(int? clientId, string? info, CancellationToken ct)
    {
        if (clientId is not int id || info is null) return;

        var visitor = await _db.CashboxVisitors.FirstOrDefaultAsync(v => v.IdVisitor == id, ct);
        if (visitor is null) return;

        var value = string.IsNullOrWhiteSpace(info) ? null : info.Trim();
        if (value is { Length: > 500 }) value = value[..500];
        visitor.Info = value;
    }

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

    private async Task ApplyTariffAsync(BookingResExtra extra, int zoneId, DateTime from, DateTime to, CancellationToken ct)
    {
        extra.TariffTicketId = null;
        extra.TariffName = null;
        extra.TariffPrice = null;

        //var minutes = (int)Math.Round((to - from).TotalMinutes);
        //if (minutes <= 0) return;

        //var tariffs = await (from tz in _db.TicketZones.AsNoTracking()
        //                     join t in _db.Tickets.AsNoTracking() on tz.IdTicket equals t.IdTicket
        //                     where tz.IdZone == zoneId && tz.Reservation && t.Active
        //                     select new { t.IdTicket, t.NameTicket, t.TotalPrice, tz.ReservationTime })
        //                    .ToListAsync(ct);
        //if (tariffs.Count == 0) return;

        //var chosen = tariffs
        //    .Where(x => x.ReservationTime >= minutes)
        //    .OrderBy(x => x.ReservationTime).ThenBy(x => x.TotalPrice)
        //    .FirstOrDefault()
        //    ?? tariffs.OrderByDescending(x => x.ReservationTime).ThenBy(x => x.TotalPrice).First();

        //extra.TariffTicketId = chosen.IdTicket;
        //extra.TariffName = chosen.NameTicket;
        //extra.TariffPrice = chosen.TotalPrice;
    }

    public async Task<BookingEventDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var r = await _db.ZoneReservations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return null;

        var extra = await _db.ResExtras.AsNoTracking().FirstOrDefaultAsync(e => e.ReservationId == id, ct);
        var svc = await _db.ResServices.AsNoTracking()
            .Where(s => s.ReservationId == id).OrderBy(s => s.SortOrder).ToListAsync(ct);

        var clientNames = new Dictionary<int, string>();
        var clientInfos = new Dictionary<int, string?>();
        if (extra?.ClientVisitorId is int cid)
        {
            var client = await _db.CashboxVisitors.AsNoTracking()
                .Where(v => v.IdVisitor == cid)
                .Select(v => new { Name = (v.Surname + " " + v.Name).Trim(), v.Info })
                .FirstOrDefaultAsync(ct);
            if (client is not null)
            {
                clientNames[cid] = client.Name;
                clientInfos[cid] = client.Info;
            }
        }
        var waiterNames = new Dictionary<int, string>();
        if (extra?.WaiterLoginId is int wid)
        {
            var name = await _db.TLogins.AsNoTracking()
                .Where(t => t.Fid == wid).Select(t => t.Fuser ?? "").FirstOrDefaultAsync(ct);
            if (name is not null) waiterNames[wid] = name;
        }

        return Map(r, extra, svc, clientNames, waiterNames, await LoadLabelsAsync(ct), clientInfos);
    }

    private static BookingEventDto Map(
        ZoneReservation r,
        BookingResExtra? ex,
        List<BookingResServiceItem>? svc,
        IReadOnlyDictionary<int, string> clientNames,
        IReadOnlyDictionary<int, string> waiterNames,
        IReadOnlyDictionary<int, LabelDto> labels,
        IReadOnlyDictionary<int, string?>? clientInfos = null)
    {
        var label = ex?.Label ?? BookingLabel.None;
        var labelName = labels.TryGetValue((int)label, out var ld) ? ld.Name : BookingLabelInfo.Name(label);
        var labelColor = labels.TryGetValue((int)label, out var lc) ? lc.Color : BookingLabelInfo.Color(label);

        var title = ex?.Title;
        if (string.IsNullOrWhiteSpace(title))
            title = r.OrderId != null ? $"Касса #{r.OrderId}" : "(бронь)";

        string? clientName = null;
        if (ex?.ClientVisitorId is int cid) clientNames.TryGetValue(cid, out clientName);

        string? clientInfo = null;                                            
        if (ex?.ClientVisitorId is int cid2 && clientInfos is not null)        
            clientInfos.TryGetValue(cid2, out clientInfo);

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
            LabelName = labelName,
            Color = labelColor,
            Note = r.Info,
            ClientVisitorId = ex?.ClientVisitorId,
            ClientName = clientName,
            ClientInfo = clientInfo,
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
            PrepaymentTypeId = ex?.PrepaymentTypeId,
            IsCancelled = ex?.IsCancelled ?? false,
            CancelledAt = ex?.CancelledAt,
            GuestsCount = ex?.GuestsCount,
            TotalPrice = services.Sum(s => s.Price * s.Quantity) + (ex?.TariffPrice ?? 0m),
            CanEdit = true
        };
    }
}
