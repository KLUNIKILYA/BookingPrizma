using BookingSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class ResourceService : IResourceService
{
    private static readonly string[] NonRoomZones = { "Касса", "Парк" };

    private readonly BookingDbContext _db;

    public ResourceService(BookingDbContext db) => _db = db;

    public async Task<List<ResourceDto>> GetResourcesAsync(CancellationToken ct = default)
    {
        var zones = await _db.Zones.AsNoTracking()
            .Where(z => z.Active && !NonRoomZones.Contains(z.NameZone))
            .OrderBy(z => z.IdZone)
            .Select(z => new { z.IdZone, z.NameZone })
            .ToListAsync(ct);

        var assignments = await (from a in _db.ZoneAssignments.AsNoTracking()
                                 join t in _db.ZoneTypes.AsNoTracking() on a.ZoneTypeId equals t.Id
                                 select new { a.ZoneId, a.ZoneTypeId, TypeName = t.Name, a.SortOrder })
                                .ToListAsync(ct);
        var byZone = assignments.ToDictionary(a => a.ZoneId);

        return zones.Select(z =>
        {
            byZone.TryGetValue(z.IdZone, out var a);
            return new ResourceDto
            {
                Id = z.IdZone,
                DisplayName = z.NameZone,
                SortOrder = a?.SortOrder ?? z.IdZone,
                ZoneTypeId = a?.ZoneTypeId,
                ZoneTypeName = a?.TypeName
            };
        }).ToList();
    }

    public async Task<List<ZoneTypeDto>> GetZoneTypesAsync(CancellationToken ct = default)
    {
        var types = await _db.ZoneTypes.AsNoTracking()
            .Where(t => t.Active)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Select(t => new ZoneTypeDto { Id = t.Id, Name = t.Name, SortOrder = t.SortOrder })
            .ToListAsync(ct);
        if (types.Count == 0) return types;

        var resources = await GetResourcesAsync(ct);
        var byType = resources.Where(r => r.ZoneTypeId.HasValue)
            .GroupBy(r => r.ZoneTypeId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.SortOrder).ThenBy(r => r.Id).ToList());

        foreach (var t in types)
            if (byType.TryGetValue(t.Id, out var zs)) t.Zones = zs;

        return types;
    }

    public async Task<List<ClientDto>> GetWaitersAsync(CancellationToken ct = default)
    {
        return await _db.TLogins.AsNoTracking()
            .Where(t => t.Factive && t.Fuser != null && t.Fuser != "")
            .OrderBy(t => t.Fuser)
            .Select(t => new ClientDto { Id = t.Fid, FullName = t.Fuser! })
            .ToListAsync(ct);
    }
}
