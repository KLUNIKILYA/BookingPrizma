using BookingSystem.Shared.Dtos;

namespace BookingSystem.Infrastructure.Services;

public interface ICatalogService
{
    Task<List<ServiceGroupDto>> GetGroupsAsync(CancellationToken ct = default);

    Task<List<ServiceDto>> GetServicesAsync(int? groupId, CancellationToken ct = default);

    Task<List<TariffDto>> GetTariffsAsync(int zoneId, CancellationToken ct = default);

    Task<List<TicketDto>> SearchTicketsAsync(string? search, int take = 20, CancellationToken ct = default);

    Task<List<TicketFolderDto>> GetTicketFoldersAsync(CancellationToken ct = default);
}
