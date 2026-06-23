namespace BookingSystem.Infrastructure.Legacy;

/// <summary>Read-only проекция dbo.TLogins (сотрудники/учётки). FUSER — имя. Не участвует в миграциях.</summary>
public class TLogin
{
    public int Fid { get; set; }
    public string Flogin { get; set; } = null!;
    public string? Fuser { get; set; }
    public bool Factive { get; set; }
}
