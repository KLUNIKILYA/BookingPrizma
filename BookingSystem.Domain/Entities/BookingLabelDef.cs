namespace BookingSystem.Domain.Entities;

/// <summary>
/// Настраиваемая метка-статус брони (таблица Booking_Label). Id хранится в Booking_ResExtra.Label.
/// Первые 6 (id 0..5) засеяны из встроенного набора; админ может добавлять/менять/красить свои.
/// </summary>
public class BookingLabelDef
{
    /// <summary>Id метки (= значение Booking_ResExtra.Label). Не identity — задаём сами.</summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Цвет hex, напр. #A8D5A2.</summary>
    public string Color { get; set; } = "#E3E8EF";

    /// <summary>Скрытая метка не показывается в легенде/дропдауне (мягкое удаление).</summary>
    public bool IsActive { get; set; } = true;
}
