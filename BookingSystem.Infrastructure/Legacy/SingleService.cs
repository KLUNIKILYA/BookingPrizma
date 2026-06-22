namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Read-only проекция легаси-таблицы dbo.SingleService (каталог услуг/товаров).
/// Только нужные поля. Не участвует в миграциях.
/// </summary>
public class SingleService
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int SingleServiceGroupId { get; set; }
    public int? ServiceColor { get; set; }
    public bool Active { get; set; }
}
