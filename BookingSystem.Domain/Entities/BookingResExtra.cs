namespace BookingSystem.Domain.Entities;

/// <summary>
/// Сопутствующие данные брони (доп-поля, которых нет в ZoneReservation).
/// 1:1 с бронью: ReservationId = ZoneReservation.ID. Таблица Booking_ResExtra.
/// </summary>
public class BookingResExtra
{
    /// <summary>= ZoneReservation.ID (PK, не identity).</summary>
    public int ReservationId { get; set; }

    /// <summary>ФИО / имя брони.</summary>
    public string? Title { get; set; }

    /// <summary>Клиент из CashboxVisitor (необязательно).</summary>
    public int? ClientVisitorId { get; set; }

    /// <summary>Официант — TLogins.FID (необязательно).</summary>
    public int? WaiterLoginId { get; set; }

    public BookingLabel Label { get; set; } = BookingLabel.None;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BookingResServiceItem> Services { get; set; } = new List<BookingResServiceItem>();
}
