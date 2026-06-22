using BookingSystem.Shared.Dtos;

namespace BookingSystem.Shared.Services;

/// <summary>Каталог: группы услуг и услуги с настройками бронирования.</summary>
public interface ICatalogService
{
    Task<List<ServiceGroupDto>> GetGroupsAsync(CancellationToken ct = default);

    /// <summary>Услуги группы; groupId = null → полный список бронируемых услуг.</summary>
    Task<List<ServiceDto>> GetServicesAsync(int? groupId, CancellationToken ct = default);
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
}
