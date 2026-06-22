using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>
/// Каталог услуг: группы из легаси SingleServiceGroup, услуги — джойн SingleService
/// с Booking_ServiceSetting (длительность/перерыв/цвет). Бронируемы только услуги с настройкой IsBookable.
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
        var q = from s in _db.SingleServices.AsNoTracking().Where(s => s.Active)
                join st in _db.ServiceSettings.AsNoTracking().Where(st => st.IsBookable)
                    on s.Id equals st.ServiceId
                select new { s, st };

        if (groupId.HasValue)
            q = q.Where(x => x.s.SingleServiceGroupId == groupId.Value);

        return await q
            .OrderBy(x => x.s.Name)
            .Select(x => new ServiceDto
            {
                Id = x.s.Id,
                Name = x.s.Name,
                Price = x.s.Price,
                GroupId = x.s.SingleServiceGroupId,
                DurationMinutes = x.st.DurationMinutes,
                BreakMinutes = x.st.BreakMinutes,
                Color = x.st.ColorOverride
            })
            .ToListAsync(ct);
    }
}
