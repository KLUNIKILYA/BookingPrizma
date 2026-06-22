using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared.Dtos;

/// <summary>Строка выбранной услуги внутри записи.</summary>
public class BookingServiceLineDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public bool IsDone { get; set; }
}

/// <summary>Запись бронирования для планировщика (read-модель).</summary>
public class BookingEventDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }

    public BookingLabel Label { get; set; }
    public string LabelName { get; set; } = string.Empty;

    /// <summary>Цвет записи (по метке либо по услуге), hex.</summary>
    public string Color { get; set; } = string.Empty;

    public string? Note { get; set; }
    public int? ClientVisitorId { get; set; }
    public string? ClientName { get; set; }

    public int? WaiterVisitorId { get; set; }
    public string? WaiterName { get; set; }

    public List<BookingServiceLineDto> Services { get; set; } = new();

    public decimal TotalPrice { get; set; }
    public int TotalMinutes { get; set; }
}

/// <summary>Выбор услуги при создании/редактировании (порядок важен, дубликаты допускаются).</summary>
public class BookingServiceSelection
{
    public int ServiceId { get; set; }
    public bool IsDone { get; set; }
}

/// <summary>Запрос на создание/обновление записи. Время «по» и снапшоты считает сервер.</summary>
public class BookingUpsertRequest
{
    public int ResourceId { get; set; }

    /// <summary>Дата + время начала.</summary>
    public DateTime TimeFrom { get; set; }

    /// <summary>
    /// Необязательное явное время окончания (для drag/resize в планировщике).
    /// Если null — сервер посчитает как TimeFrom + Σ(длительность+перерыв услуг).
    /// </summary>
    public DateTime? TimeToOverride { get; set; }

    public string Title { get; set; } = string.Empty;
    public int? ClientVisitorId { get; set; }
    public int? WaiterVisitorId { get; set; }
    public BookingLabel Label { get; set; } = BookingLabel.None;
    public string? Note { get; set; }

    public List<BookingServiceSelection> Services { get; set; } = new();
}
