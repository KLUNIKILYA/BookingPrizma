namespace BookingSystem.Infrastructure.Legacy;

public class ZoneReservation
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public int? OrderId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string? Info { get; set; }
}
