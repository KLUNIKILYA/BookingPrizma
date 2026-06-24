using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared.Dtos;

/// <summary>Строка выбранной услуги внутри записи (позиция с ценой, без времени).</summary>
public class BookingServiceLineDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>Цена за единицу.</summary>
    public decimal Price { get; set; }
    /// <summary>Количество (по умолчанию 1).</summary>
    public int Quantity { get; set; } = 1;
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

/// <summary>Пересечение брони: комната уже занята другой записью на пересекающееся время.</summary>
public class BookingConflictDto
{
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public int ExistingId { get; set; }
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string? Title { get; set; }
}

/// <summary>Занятость на интервале: id комнаты/официанта, имя, время и кто занял (для подсказок в окне).</summary>
public class BusySlotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string? Title { get; set; }
}

/// <summary>Доступность на выбранный интервал: какие комнаты и официанты заняты.</summary>
public class AvailabilityDto
{
    public List<BusySlotDto> Rooms { get; set; } = new();
    public List<BusySlotDto> Waiters { get; set; } = new();
}

/// <summary>Выбор услуги при создании/редактировании (порядок важен, дубликаты допускаются).</summary>
public class BookingServiceSelection
{
    public int ServiceId { get; set; }
    /// <summary>Количество (по умолчанию 1).</summary>
    public int Quantity { get; set; } = 1;
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
