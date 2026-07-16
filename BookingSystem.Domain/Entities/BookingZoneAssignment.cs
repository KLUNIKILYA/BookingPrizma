namespace BookingSystem.Domain.Entities;

/// <summary>
/// Привязка зоны (Zones.IdZone) к типу зоны. Одна зона принадлежит одному типу.
/// Таблица Booking_ZoneAssignment.
/// </summary>
public class BookingZoneAssignment
{
    /// <summary>= Zones.IdZone (PK, не identity).</summary>
    public int ZoneId { get; set; }

    public int ZoneTypeId { get; set; }

    /// <summary>Порядок зоны внутри типа (колонки сетки).</summary>
    public int SortOrder { get; set; }

    public BookingZoneType? Type { get; set; }
}
