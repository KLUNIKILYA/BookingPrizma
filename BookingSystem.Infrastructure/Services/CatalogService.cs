using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Каталог услуг из dbo.SingleService / dbo.SingleServiceGroup (PPS_Prizma).
/// Услуги — позиции с ценой, без времени.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly BookingDbContext _db;

    public CatalogService(BookingDbContext db) => _db = db;

    public async Task<List<ServiceGroupDto>> GetGroupsAsync(CancellationToken ct = default)
    {
        return await _db.SingleServiceGroups.AsNoTracking()
            .Where(g => g.Active)
            .OrderBy(g => g.Name)
            .Select(g => new ServiceGroupDto { Id = g.Id, Name = g.Name, ParentId = g.ParentId })
            .ToListAsync(ct);
    }

    public async Task<List<ServiceDto>> GetServicesAsync(int? groupId, CancellationToken ct = default)
    {
        var q = _db.SingleServices.AsNoTracking().Where(s => s.Active);
        if (groupId.HasValue)
            q = q.Where(s => s.SingleServiceGroupId == groupId.Value);

        return await q
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                GroupId = s.SingleServiceGroupId
            })
            .ToListAsync(ct);
    }

    public async Task<List<TariffDto>> GetTariffsAsync(int zoneId, CancellationToken ct = default)
    {
        // Тарифы на бронь комнаты: связь Ticket↔Zone в TicketZone с Reservation=1,
        // длительность = ReservationTime (мин), цена = Ticket.TotalPrice.
        return await (from tz in _db.TicketZones.AsNoTracking()
                      join t in _db.Tickets.AsNoTracking() on tz.IdTicket equals t.IdTicket
                      where tz.IdZone == zoneId && tz.Reservation && t.Active
                      orderby tz.ReservationTime, t.NameTicket
                      select new TariffDto
                      {
                          TicketId = t.IdTicket,
                          Name = t.NameTicket,
                          Minutes = tz.ReservationTime,
                          Price = t.TotalPrice
                      }).ToListAsync(ct);
    }

    public async Task<List<TicketDto>> SearchTicketsAsync(string? search, int take = 20, CancellationToken ct = default)
    {
        if (take <= 0) take = 20;
        var q = _db.Tickets.AsNoTracking().Where(t => t.Active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(t => EF.Functions.Like(t.NameTicket, "%" + s + "%"));
        }
        return await q
            .OrderBy(t => t.NameTicket)
            .Take(take)
            .Select(t => new TicketDto { TicketId = t.IdTicket, Name = t.NameTicket, Price = t.TotalPrice })
            .ToListAsync(ct);
    }
}
