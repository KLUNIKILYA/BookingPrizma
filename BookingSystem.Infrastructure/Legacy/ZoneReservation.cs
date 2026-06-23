namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Боевая таблица броней dbo.ZoneReservation. Пишем сюда (вставка/удаление),
/// но в миграциях не участвует. ID — identity. OrderID != null → бронь кассовой системы (read-only).
/// </summary>
public class ZoneReservation
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public int? OrderId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string? Info { get; set; }
}
