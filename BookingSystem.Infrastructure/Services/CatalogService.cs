using System.Globalization;
using BookingSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

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

    public async Task<List<TicketFolderDto>> GetTicketFoldersAsync(CancellationToken ct = default)
    {
        var folders = await _db.TicketFolders.AsNoTracking()
            .Where(f => f.Active)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync(ct);
        var folderIds = folders.Select(f => f.Id).ToHashSet();

        var tickets = await _db.Tickets.AsNoTracking()
            .Where(t => t.Active)
            .OrderBy(t => t.NameTicket)
            .Select(t => new { t.IdTicket, t.NameTicket, t.TotalPrice, t.TicketFolderId })
            .ToListAsync(ct);

        var byFolder = tickets
            .GroupBy(t => t.TicketFolderId is int fid && folderIds.Contains(fid) ? fid : 0)
            .ToDictionary(
                g => g.Key,
                g => g.Select(t => new TicketDto { TicketId = t.IdTicket, Name = t.NameTicket, Price = t.TotalPrice }).ToList());

        var ruOrder = StringComparer.Create(CultureInfo.GetCultureInfo("ru-RU"), ignoreCase: true);
        var result = new List<TicketFolderDto>();
        foreach (var f in folders.OrderBy(f => f.Name, ruOrder))
            if (byFolder.TryGetValue(f.Id, out var tk) && tk.Count > 0)
                result.Add(new TicketFolderDto { Id = f.Id, Name = f.Name, Tickets = tk });
        if (byFolder.TryGetValue(0, out var orphans) && orphans.Count > 0)
            result.Add(new TicketFolderDto { Id = 0, Name = "Без раздела", Tickets = orphans });
        return result;
    }
}
