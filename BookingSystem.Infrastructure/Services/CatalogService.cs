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
}
