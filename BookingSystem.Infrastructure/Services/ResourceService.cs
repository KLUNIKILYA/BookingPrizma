using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Ресурсы планировщика — комнаты из dbo.Zones (без «Касса»/«Парк»);
/// официанты — сотрудники из dbo.TLogins.
/// </summary>
public class ResourceService : IResourceService
{
    private static readonly string[] NonRoomZones = { "Касса", "Парк" };

    private readonly BookingDbContext _db;

    public ResourceService(BookingDbContext db) => _db = db;

    public async Task<List<ResourceDto>> GetResourcesAsync(CancellationToken ct = default)
    {
        return await _db.Zones.AsNoTracking()
            .Where(z => z.Active && !NonRoomZones.Contains(z.NameZone))
            .OrderBy(z => z.IdZone)
            .Select(z => new ResourceDto
            {
                Id = z.IdZone,
                DisplayName = z.NameZone,
                SortOrder = z.IdZone
            })
            .ToListAsync(ct);
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
