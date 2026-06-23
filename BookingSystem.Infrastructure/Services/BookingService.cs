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

        _db.ResExtras.Add(new BookingResExtra
        {
            ReservationId = reservation.Id,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            ClientVisitorId = request.ClientVisitorId,
            WaiterLoginId = request.WaiterVisitorId,
            Label = request.Label,
            CreatedAt = now,
            UpdatedAt = now
        });
        await AddServicesAsync(reservation.Id, request.Services, ct);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    public async Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default)
    {
        var reservation = await _db.ZoneReservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null) return null;
        // OrderID не трогаем — связь брони с кассовым заказом сохраняется.

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

    // ===== helpers =====

    private static DateTime ResolveEnd(BookingUpsertRequest req) =>
        req.TimeToOverride.HasValue && req.TimeToOverride.Value > req.TimeFrom
            ? req.TimeToOverride.Value
            : req.TimeFrom.AddHours(1);

    private static string? NormalizeNote(string? note) =>
        string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private async Task AddServicesAsync(int reservationId, List<BookingServiceSelection> selections, CancellationToken ct)
    {
        var ids = selections.Select(s => s.ServiceId).Distinct().ToList();
        if (ids.Count == 0) return;

        var catalog = await _db.SingleServices.AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.Name, s.Price }, ct);

        var order = 0;
        foreach (var sel in selections)
        {
            if (!catalog.TryGetValue(sel.ServiceId, out var info)) continue;
            _db.ResServices.Add(new BookingResServiceItem
            {
                ReservationId = reservationId,
                ServiceId = sel.ServiceId,
                ServiceName = info.Name,
                PriceSnapshot = info.Price,
                SortOrder = order++,
                IsDone = sel.IsDone
            });
        }
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
                IsDone = s.IsDone
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
            TotalPrice = services.Sum(s => s.Price),
            CanEdit = true   // тестовая БД — редактируем все брони, включая кассовые
        };
    }
}
