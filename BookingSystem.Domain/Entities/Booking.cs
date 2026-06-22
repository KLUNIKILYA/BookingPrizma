namespace BookingSystem.Domain.Entities;

/// <summary>
/// Запись на бронирование. Таблица Booking_Booking.
/// Время «по» = TimeFrom + Σ(длительность + перерыв выбранных услуг).
/// </summary>
public class Booking
{
    public int Id { get; set; }

    /// <summary>Сотрудник/ресурс (Booking_Resource).</summary>
    public int ResourceId { get; set; }

    /// <summary>Дата записи (без времени).</summary>
    public DateTime Date { get; set; }

    public DateTime TimeFrom { get; set; }

    public DateTime TimeTo { get; set; }

    /// <summary>ФИО (свободный текст из поля «ФИО»).</summary>
    public string Title { get; set; } = null!;

    /// <summary>Привязка к клиенту в БД (CashboxVisitor.IdVisitor), необязательная.</summary>
    public int? ClientVisitorId { get; set; }

    /// <summary>Официант, обслуживающий бронь (CashboxVisitor.IdVisitor, сотрудник), необязательно.</summary>
    public int? WaiterVisitorId { get; set; }

    public BookingLabel Label { get; set; } = BookingLabel.None;

    /// <summary>Примечание.</summary>
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>Мягкое удаление: false = запись отменена/удалена.</summary>
    public bool IsActive { get; set; } = true;

    public BookingResource? Resource { get; set; }

    public ICollection<BookingServiceItem> Services { get; set; } = new List<BookingServiceItem>();
}
