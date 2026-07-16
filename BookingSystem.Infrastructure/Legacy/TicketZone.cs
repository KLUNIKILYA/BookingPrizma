namespace BookingSystem.Infrastructure.Legacy;

public class TicketZone
{
    public int IdTicket { get; set; }
    public int IdZone { get; set; }

    public bool Reservation { get; set; }

    public int ReservationTime { get; set; }

    public int? FreeTime { get; set; }
}
