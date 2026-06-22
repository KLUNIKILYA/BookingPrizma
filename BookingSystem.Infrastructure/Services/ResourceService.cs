using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Ресурсы планировщика — комнаты и столики (Booking_Resource);
/// плюс список официантов (сотрудники из CashboxVisitor) для привязки к брони.
/// </summary>
public class ResourceService : IResourceService
{
    private readonly BookingDbContext _db;

    public ResourceService(BookingDbContext db) => _db = db;

    public async Task<List<ResourceDto>> GetResourcesAsync(CancellationToken ct = default)
    {
        return await _db.Resources.AsNoTracking()
            .Where(r => r.Active)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.DisplayName)
            .Select(r => new ResourceDto
            {
                Id = r.Id,
                Kind = r.Kind,
                DisplayName = r.DisplayName,
                Color = r.Color,
                SortOrder = r.SortOrder
            })
            .ToListAsync(ct);
    }

    public async Task<List<ClientDto>> GetWaitersAsync(CancellationToken ct = default)
    {
        return await _db.CashboxVisitors.AsNoTracking()
            .Where(v => v.Active && v.SotrudnikStatus)
            .OrderBy(v => v.Surname).ThenBy(v => v.Name)
            .Select(v => new ClientDto
            {
                Id = v.IdVisitor,
                FullName = (v.Surname + " " + v.Name).Trim()
            })
            .ToListAsync(ct);
    }
}
