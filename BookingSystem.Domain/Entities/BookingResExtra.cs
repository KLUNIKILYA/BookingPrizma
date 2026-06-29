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

    /// <summary>Выбранный тариф на бронь — Ticket.IdTicket (необязательно).</summary>
    public int? TariffTicketId { get; set; }

    /// <summary>Снапшот названия тарифа.</summary>
    public string? TariffName { get; set; }

    /// <summary>Снапшот цены тарифа.</summary>
    public decimal? TariffPrice { get; set; }

    /// <summary>Имя именинника.</summary>
    public string? CelebrantName { get; set; }

    /// <summary>Дата рождения именинника.</summary>
    public DateTime? CelebrantBirthDate { get; set; }

    /// <summary>Внесена ли предоплата.</summary>
    public bool IsPrepaid { get; set; }

    /// <summary>Сумма предоплаты.</summary>
    public decimal? PrepaidAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BookingResServiceItem> Services { get; set; } = new List<BookingResServiceItem>();
}
