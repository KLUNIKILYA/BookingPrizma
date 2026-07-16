namespace BookingSystem.Infrastructure.Legacy;

public class SingleServiceGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool Active { get; set; }
}
