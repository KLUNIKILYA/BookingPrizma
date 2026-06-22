namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Read-only проекция легаси-таблицы dbo.CashboxVisitor (клиенты и сотрудники).
/// Сотрудники — записи с SotrudnikStatus = true. Не участвует в миграциях.
/// </summary>
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
