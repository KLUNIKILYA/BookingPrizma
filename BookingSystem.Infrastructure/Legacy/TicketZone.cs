namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Read-only проекция dbo.TicketZone — связь билета (Ticket) и зоны (Zones).
/// Для тарифов на бронь: <see cref="Reservation"/> = 1, а <see cref="ReservationTime"/> —
/// длительность брони в минутах (например 120 = 2 часа, 720 = целый день).
/// Не участвует в миграциях.
/// </summary>
public class TicketZone
{
    public int IdTicket { get; set; }
    public int IdZone { get; set; }

    /// <summary>Этот билет — тариф на бронирование данной зоны.</summary>
    public bool Reservation { get; set; }

    /// <summary>Длительность брони в минутах (имеет смысл при Reservation = 1).</summary>
    public int ReservationTime { get; set; }

    public int? FreeTime { get; set; }
}
