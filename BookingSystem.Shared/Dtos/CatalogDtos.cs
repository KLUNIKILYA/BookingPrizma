using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared.Dtos;

/// <summary>Группа услуг («группа продаж»).</summary>
public class ServiceGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

/// <summary>Услуга каталога с настройками бронирования (длительность/перерыв/цвет).</summary>
public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int GroupId { get; set; }
    public int DurationMinutes { get; set; }
    public int BreakMinutes { get; set; }

    /// <summary>Цвет услуги в планировщике, hex (#RRGGBB) либо null.</summary>
    public string? Color { get; set; }
}

/// <summary>Ресурс — комната или столик (колонка планировщика / пункт дропдауна «Комната/столик»).</summary>
public class ResourceDto
{
    public int Id { get; set; }
    public ResourceKind Kind { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Клиент из БД (CashboxVisitor).</summary>
public class ClientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>Запрос на создание нового клиента в БД.</summary>
public class CreateClientRequest
{
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
