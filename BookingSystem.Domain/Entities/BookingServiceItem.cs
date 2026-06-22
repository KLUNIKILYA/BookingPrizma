namespace BookingSystem.Domain.Entities;

/// <summary>
/// Строка выбранной услуги внутри записи (грид «Список выбранных услуг»).
/// Цена/длительность/перерыв фиксируются снапшотом на момент сохранения,
/// чтобы история не «плыла» при изменении прайса. Таблица Booking_BookingService.
/// </summary>
public class BookingServiceItem
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    /// <summary>Ссылка на услугу SingleService.Id.</summary>
    public int ServiceId { get; set; }

    /// <summary>Снапшот названия услуги (для отображения и истории).</summary>
    public string ServiceName { get; set; } = null!;

    public decimal PriceSnapshot { get; set; }

    public int DurationSnapshot { get; set; }

    public int BreakSnapshot { get; set; }

    public int SortOrder { get; set; }

    /// <summary>«Сост.» — отметка о выполнении услуги.</summary>
    public bool IsDone { get; set; }

    public Booking? Booking { get; set; }
}
