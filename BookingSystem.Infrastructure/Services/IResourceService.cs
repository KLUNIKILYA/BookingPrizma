using BookingSystem.Shared.Dtos;

namespace BookingSystem.Infrastructure.Services;

public interface IResourceService
{
    Task<List<ResourceDto>> GetResourcesAsync(CancellationToken ct = default);

    Task<List<ZoneTypeDto>> GetZoneTypesAsync(CancellationToken ct = default);

    Task<List<ClientDto>> GetWaitersAsync(CancellationToken ct = default);
}
