using BookingSystem.Shared.Dtos;

namespace BookingSystem.Infrastructure.Services;

public interface ILabelService
{
    Task<List<LabelDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    Task<LabelDto> CreateAsync(LabelUpsertRequest request, CancellationToken ct = default);

    Task<LabelDto?> UpdateAsync(int id, LabelUpsertRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
