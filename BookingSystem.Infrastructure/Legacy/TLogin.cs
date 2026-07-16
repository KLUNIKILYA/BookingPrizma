namespace BookingSystem.Infrastructure.Legacy;

public class TLogin
{
    public int Fid { get; set; }
    public string Flogin { get; set; } = null!;
    public string? Fuser { get; set; }
    public bool Factive { get; set; }
}
