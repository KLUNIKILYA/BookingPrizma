namespace BookingSystem.Domain.Entities;

/// <summary>
/// Тип (группа) зон для планировщика: «1 этаж», «2 этаж», «Комнаты» и т.п.
/// Настраиваемый список. Таблица Booking_ZoneType.
/// </summary>
public class BookingZoneType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<BookingZoneAssignment> Zones { get; set; } = new List<BookingZoneAssignment>();
}
