namespace BookingSystem.Infrastructure.Legacy;

public class Zone
{
    public int IdZone { get; set; }
    public string NameZone { get; set; } = null!;
    public string? ShortNameZone { get; set; }
    public bool Active { get; set; }
}
