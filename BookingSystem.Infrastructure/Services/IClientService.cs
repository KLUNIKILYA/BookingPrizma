using BookingSystem.Shared.Dtos;

namespace BookingSystem.Infrastructure.Services;

public interface IClientService
{
    Task<List<ClientDto>> SearchAsync(string? search, int take = 20, CancellationToken ct = default);

    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
}
