using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared.Dtos;

/// <summary>Строка выбранной услуги внутри записи (позиция с ценой, без времени).</summary>
public class BookingServiceLineDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDone { get; set; }
}

/// <summary>Запись бронирования для планировщика (read-модель). Соответствует строке ZoneReservation.</summary>
public class BookingEventDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }          // = ZoneID
    public string Title { get; set; } = string.Empty;
    public DateTime TimeFrom { get; set; }       // = DateFrom
    public DateTime TimeTo { get; set; }         // = DateTo

    public BookingLabel Label { get; set; }
    public string LabelName { get; set; } = string.Empty;

    /// <summary>Цвет записи (по метке), hex.</summary>
    public string Color { get; set; } = string.Empty;

    public string? Note { get; set; }            // = Info
    public int? ClientVisitorId { get; set; }
    public string? ClientName { get; set; }

    public int? WaiterVisitorId { get; set; }    // = TLogins.FID
    public string? WaiterName { get; set; }

    public List<BookingServiceLineDto> Services { get; set; } = new();
    public decimal TotalPrice { get; set; }

    /// <summary>Можно ли редактировать/удалять (своя бронь, без OrderID). Кассовые брони — только чтение.</summary>
    public bool CanEdit { get; set; }
}

/// <summary>Выбор услуги при создании/редактировании (порядок важен, дубликаты допускаются).</summary>
public class BookingServiceSelection
{
    public int ServiceId { get; set; }
    public bool IsDone { get; set; }
}

/// <summary>Запрос на создание/обновление записи (= строка ZoneReservation + companion).</summary>
public class BookingUpsertRequest
{
    public int ResourceId { get; set; }          // = ZoneID

    public DateTime TimeFrom { get; set; }        // = DateFrom

    /// <summary>Время окончания (= DateTo). Задаётся редактором/перетаскиванием.</summary>
    public DateTime? TimeToOverride { get; set; }

    public string Title { get; set; } = string.Empty;
    public int? ClientVisitorId { get; set; }
    public int? WaiterVisitorId { get; set; }     // = TLogins.FID
    public BookingLabel Label { get; set; } = BookingLabel.None;
    public string? Note { get; set; }             // = Info

    public List<BookingServiceSelection> Services { get; set; } = new();
}
