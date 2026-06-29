namespace BookingSystem.Domain.Entities;

/// <summary>
/// Услуга, привязанная к брони (позиция с ценой, без времени). Таблица Booking_ResService.
/// </summary>
public class BookingResServiceItem
{
    public int Id { get; set; }

    /// <summary>= ZoneReservation.ID.</summary>
    public int ReservationId { get; set; }

    /// <summary>SingleService.ID (услуга) либо Ticket.IdTicket (билет) — см. <see cref="IsTicket"/>.</summary>
    public int ServiceId { get; set; }

    /// <summary>Снапшот названия услуги.</summary>
    public string ServiceName { get; set; } = null!;

    public decimal PriceSnapshot { get; set; }

    /// <summary>Количество услуги (по умолчанию 1). Сумма позиции = PriceSnapshot * Quantity.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// true — позиция-билет из dbo.Ticket (взрослый/детский/сопровождающий);
    /// false — услуга из dbo.SingleService.
    /// </summary>
    public bool IsTicket { get; set; }

    public int SortOrder { get; set; }

    /// <summary>«Готово» — отметка о выполнении услуги.</summary>
    public bool IsDone { get; set; }

    public BookingResExtra? Extra { get; set; }
}
