using BookingSystem.Shared.Dtos;

namespace BookingSystem.Shared.Services;

/// <summary>Каталог: группы услуг и услуги с настройками бронирования.</summary>
public interface ICatalogService
{
    Task<List<ServiceGroupDto>> GetGroupsAsync(CancellationToken ct = default);

    /// <summary>Услуги группы; groupId = null → полный список бронируемых услуг.</summary>
    Task<List<ServiceDto>> GetServicesAsync(int? groupId, CancellationToken ct = default);

    /// <summary>Тарифы на бронь конкретной комнаты (Ticket+TicketZone, Reservation=1).</summary>
    Task<List<TariffDto>> GetTariffsAsync(int zoneId, CancellationToken ct = default);

    /// <summary>Поиск билетов-услуг по названию (взрослый/детский/сопровождающий и т.п.).</summary>
    Task<List<TicketDto>> SearchTicketsAsync(string? search, int take = 20, CancellationToken ct = default);
}

/// <summary>Ресурсы (комнаты/столики) для колонок планировщика и список официантов.</summary>
public interface IResourceService
{
    Task<List<ResourceDto>> GetResourcesAsync(CancellationToken ct = default);

    /// <summary>Официанты (сотрудники из CashboxVisitor) для привязки к брони.</summary>
    Task<List<ClientDto>> GetWaitersAsync(CancellationToken ct = default);
}

/// <summary>Поиск и создание клиентов в БД (CashboxVisitor).</summary>
public interface IClientService
{
    Task<List<ClientDto>> SearchAsync(string? search, int take = 20, CancellationToken ct = default);

    /// <summary>Создаёт нового клиента в БД и возвращает его.</summary>
    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
}

/// <summary>Брони: чтение диапазона и CRUD.</summary>
public interface IBookingService
{
    Task<List<BookingEventDto>> GetBookingsAsync(
        DateTime from, DateTime to, int? resourceId, int? groupId, CancellationToken ct = default);

    Task<BookingEventDto> CreateAsync(BookingUpsertRequest request, CancellationToken ct = default);

    Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Пересечения брони по времени в указанных комнатах (исключая бронь excludeId).</summary>
    Task<List<BookingConflictDto>> CheckConflictsAsync(
        int? excludeId, IReadOnlyCollection<int> resourceIds, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Занятость комнат и официантов на интервале [from; to) (исключая бронь excludeId).</summary>
    Task<AvailabilityDto> GetAvailabilityAsync(
        DateTime from, DateTime to, int? excludeId, CancellationToken ct = default);
}
