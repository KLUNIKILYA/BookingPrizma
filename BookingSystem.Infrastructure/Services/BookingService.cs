using BookingSystem.Domain.Entities;
using BookingSystem.Shared;
using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Брони: чтение диапазона для планировщика и CRUD.
/// Время «по» и снапшоты цены/длительности/перерыва считает сервер из каталога.
/// </summary>
public class BookingService : IBookingService
{
    private const int DefaultDurationMinutes = 30;

    private readonly BookingDbContext _db;

    public BookingService(BookingDbContext db) => _db = db;

    public async Task<List<BookingEventDto>> GetBookingsAsync(
        DateTime from, DateTime to, int? resourceId, int? groupId, CancellationToken ct = default)
    {
        // Пересечение интервалов: начинается раньше конца окна и кончается позже начала.
        var q = _db.Bookings.AsNoTracking()
            .Include(b => b.Services)
            .Where(b => b.IsActive && b.TimeFrom < to && b.TimeTo > from);

        if (resourceId.HasValue)
            q = q.Where(b => b.ResourceId == resourceId.Value);

        if (groupId.HasValue)
            q = q.Where(b => b.Services.Any(s =>
                _db.SingleServices.Any(ss => ss.Id == s.ServiceId && ss.SingleServiceGroupId == groupId.Value)));

        var list = await q.ToListAsync(ct);

        var ids = list.Where(b => b.ClientVisitorId.HasValue).Select(b => b.ClientVisitorId!.Value)
            .Concat(list.Where(b => b.WaiterVisitorId.HasValue).Select(b => b.WaiterVisitorId!.Value));
        var names = await ResolveClientNamesAsync(ids, ct);

        return list.Select(b => MapToEvent(b, names)).ToList();
    }

    public async Task<BookingEventDto> CreateAsync(BookingUpsertRequest request, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var booking = new Booking { CreatedAt = now, UpdatedAt = now, IsActive = true };

        await ApplyRequestAsync(booking, request, ct);

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(booking.Id, ct))!;
    }

    public async Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Services)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive, ct);
        if (booking is null) return null;

        // Пересобираем строки услуг заново.
        _db.BookingServiceItems.RemoveRange(booking.Services);
        booking.Services.Clear();

        await ApplyRequestAsync(booking, request, ct);
        booking.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(booking.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.IsActive, ct);
        if (booking is null) return false;

        booking.IsActive = false;
        booking.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ===== helpers =====

    /// <summary>Заполняет поля брони из запроса: услуги-снапшоты, время «по», прочие поля.</summary>
    private async Task ApplyRequestAsync(Booking booking, BookingUpsertRequest request, CancellationToken ct)
    {
        booking.ResourceId = request.ResourceId;
        booking.Title = (request.Title ?? string.Empty).Trim();
        booking.ClientVisitorId = request.ClientVisitorId;
        booking.WaiterVisitorId = request.WaiterVisitorId;
        booking.Label = request.Label;
        booking.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        booking.TimeFrom = request.TimeFrom;
        booking.Date = request.TimeFrom.Date;

        // Снапшоты услуг из каталога (SingleService + Booking_ServiceSetting).
        var ids = request.Services.Select(s => s.ServiceId).Distinct().ToList();
        var catalog = ids.Count == 0
            ? new Dictionary<int, (string Name, decimal Price, int Dur, int Brk)>()
            : await (from s in _db.SingleServices.AsNoTracking().Where(s => ids.Contains(s.Id))
                     join st in _db.ServiceSettings.AsNoTracking() on s.Id equals st.ServiceId into stg
                     from st in stg.DefaultIfEmpty()
                     select new
                     {
                         s.Id,
                         s.Name,
                         s.Price,
                         Dur = st != null ? st.DurationMinutes : 0,
                         Brk = st != null ? st.BreakMinutes : 0
                     })
                .ToDictionaryAsync(x => x.Id, x => (x.Name, x.Price, x.Dur, x.Brk), ct);

        var totalMinutes = 0;
        var order = 0;
        foreach (var sel in request.Services)
        {
            if (!catalog.TryGetValue(sel.ServiceId, out var info)) continue;
            booking.Services.Add(new BookingServiceItem
            {
                ServiceId = sel.ServiceId,
                ServiceName = info.Name,
                PriceSnapshot = info.Price,
                DurationSnapshot = info.Dur,
                BreakSnapshot = info.Brk,
                SortOrder = order++,
                IsDone = sel.IsDone
            });
            totalMinutes += info.Dur + info.Brk;
        }

        if (request.TimeToOverride.HasValue && request.TimeToOverride.Value > booking.TimeFrom)
            booking.TimeTo = request.TimeToOverride.Value;
        else
            booking.TimeTo = booking.TimeFrom.AddMinutes(totalMinutes > 0 ? totalMinutes : DefaultDurationMinutes);
    }

    private async Task<BookingEventDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var b = await _db.Bookings.AsNoTracking()
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return null;

        var ids = new[] { b.ClientVisitorId, b.WaiterVisitorId }.Where(x => x.HasValue).Select(x => x!.Value);
        var names = await ResolveClientNamesAsync(ids, ct);
        return MapToEvent(b, names);
    }

    private async Task<Dictionary<int, string>> ResolveClientNamesAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<int, string>();

        return await _db.CashboxVisitors.AsNoTracking()
            .Where(v => idList.Contains(v.IdVisitor))
            .ToDictionaryAsync(v => v.IdVisitor, v => (v.Surname + " " + v.Name).Trim(), ct);
    }

    private static BookingEventDto MapToEvent(Booking b, IReadOnlyDictionary<int, string> clientNames)
    {
        var services = b.Services
            .OrderBy(s => s.SortOrder)
            .Select(s => new BookingServiceLineDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Price = s.PriceSnapshot,
                DurationMinutes = s.DurationSnapshot,
                BreakMinutes = s.BreakSnapshot,
                IsDone = s.IsDone
            })
            .ToList();

        string? clientName = null;
        if (b.ClientVisitorId.HasValue)
            clientNames.TryGetValue(b.ClientVisitorId.Value, out clientName);

        string? waiterName = null;
        if (b.WaiterVisitorId.HasValue)
            clientNames.TryGetValue(b.WaiterVisitorId.Value, out waiterName);

        return new BookingEventDto
        {
            Id = b.Id,
            ResourceId = b.ResourceId,
            Title = b.Title,
            TimeFrom = b.TimeFrom,
            TimeTo = b.TimeTo,
            Label = b.Label,
            LabelName = BookingLabelInfo.Name(b.Label),
            Color = BookingLabelInfo.Color(b.Label),
            Note = b.Note,
            ClientVisitorId = b.ClientVisitorId,
            ClientName = clientName,
            WaiterVisitorId = b.WaiterVisitorId,
            WaiterName = waiterName,
            Services = services,
            TotalPrice = services.Sum(s => s.Price),
            TotalMinutes = services.Sum(s => s.DurationMinutes + s.BreakMinutes)
        };
    }
}
