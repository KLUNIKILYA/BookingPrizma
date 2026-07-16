namespace BookingSystem.Infrastructure.Legacy;

public class TicketFolder
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
}
