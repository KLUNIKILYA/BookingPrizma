namespace BookingSystem.Infrastructure.Legacy;

/// <summary>Read-only проекция dbo.Zones (комнаты). Не участвует в миграциях.</summary>
public class Zone
{
    public int IdZone { get; set; }
    public string NameZone { get; set; } = null!;
    public string? ShortNameZone { get; set; }
    public bool Active { get; set; }
}
