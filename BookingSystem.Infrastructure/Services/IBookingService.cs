using BookingSystem.Shared.Dtos;

namespace BookingSystem.Infrastructure.Services;

public interface IBookingService
{
    Task<List<BookingEventDto>> GetBookingsAsync(
        DateTime from, DateTime to, int? resourceId, int? groupId, CancellationToken ct = default);

    Task<BookingEventDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<BookingEventDto> CreateAsync(BookingUpsertRequest request, CancellationToken ct = default);

    Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task<BookingEventDto?> CancelAsync(int id, CancellationToken ct = default);

    Task<BookingEventDto?> RestoreAsync(int id, CancellationToken ct = default);

    Task<List<BookingConflictDto>> CheckConflictsAsync(
        int? excludeId, IReadOnlyCollection<int> resourceIds, DateTime from, DateTime to, CancellationToken ct = default);

    Task<AvailabilityDto> GetAvailabilityAsync(
        DateTime from, DateTime to, int? excludeId, CancellationToken ct = default);
}
