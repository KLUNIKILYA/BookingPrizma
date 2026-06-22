namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Read-only проекция легаси-таблицы dbo.SingleServiceGroup (группы услуг/«группа продаж»).
/// Не участвует в миграциях.
/// </summary>
public class SingleServiceGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool Active { get; set; }
}
