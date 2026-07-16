namespace BookingSystem.Infrastructure.Legacy;

public class SingleService
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int SingleServiceGroupId { get; set; }
    public int? ServiceColor { get; set; }
    public bool Active { get; set; }
}
