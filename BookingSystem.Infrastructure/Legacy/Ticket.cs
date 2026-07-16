namespace BookingSystem.Infrastructure.Legacy;

public class Ticket
{
    public int IdTicket { get; set; }
    public string NameTicket { get; set; } = null!;
    public bool Active { get; set; }

    public int? TicketFolderId { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal OnePrice { get; set; }
}
