namespace BookingSystem.Domain.Entities;

/// <summary>
/// Настройки услуги для бронирования — добивают то, чего нет в легаси SingleService
/// (длительность и перерыв в минутах). Таблица Booking_ServiceSetting.
/// </summary>
public class BookingServiceSetting
{
    public int Id { get; set; }

    /// <summary>Ссылка на услугу SingleService.Id (read-only легаси-каталог).</summary>
    public int ServiceId { get; set; }

    /// <summary>Длительность услуги, минут.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Перерыв после услуги, минут.</summary>
    public int BreakMinutes { get; set; }

    /// <summary>Переопределение цвета услуги в планировщике, hex (#RRGGBB).</summary>
    public string? ColorOverride { get; set; }

    /// <summary>Доступна ли услуга для записи.</summary>
    public bool IsBookable { get; set; } = true;
}
