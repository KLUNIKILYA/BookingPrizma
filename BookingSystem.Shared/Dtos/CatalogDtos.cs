namespace BookingSystem.Shared.Dtos;

/// <summary>Группа услуг («группа продаж»).</summary>
public class ServiceGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

/// <summary>Услуга каталога. Позиция с ценой (без времени).</summary>
public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int GroupId { get; set; }
}

/// <summary>Тариф на бронь комнаты: билет из dbo.Ticket + длительность из dbo.TicketZone.</summary>
public class TariffDto
{
    public int TicketId { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Длительность брони в минутах (TicketZone.ReservationTime).</summary>
    public int Minutes { get; set; }
    /// <summary>Цена тарифа (Ticket.TotalPrice).</summary>
    public decimal Price { get; set; }
}

/// <summary>Билет-услуга (взрослый/детский/сопровождающий) — позиция с ценой, без времени.</summary>
public class TicketDto
{
    public int TicketId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>Ресурс — комната (колонка планировщика / пункт фильтра «Комната»).</summary>
public class ResourceDto
{
    public int Id { get; set; }
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
