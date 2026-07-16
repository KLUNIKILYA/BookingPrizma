namespace BookingSystem.Infrastructure.Legacy;

public class CashboxVisitor
{
    public int IdVisitor { get; set; }
    public string Surname { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string? Info { get; set; }
    public bool SotrudnikStatus { get; set; }
    public bool Active { get; set; }
}
