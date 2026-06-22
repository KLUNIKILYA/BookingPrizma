namespace BookingSystem.Domain.Entities;

/// <summary>
/// Ресурс расписания — сотрудник/специалист, под которого можно записывать (колонка в планировщике).
/// Имя берётся из CashboxVisitor (SotrudnikStatus=true), а порядок/цвет/доступность храним здесь.
/// Таблица Booking_Resource.
/// </summary>
public class BookingResource
{
    public int Id { get; set; }

    /// <summary>Тип ресурса: комната или столик.</summary>
    public ResourceKind Kind { get; set; }

    /// <summary>Ссылка на сотрудника в CashboxVisitor.IdVisitor (необязательная, историческое поле).</summary>
    public int? VisitorId { get; set; }

    /// <summary>Отображаемое имя в колонке/дропдауне (например, «Комната 1», «Столик 3»).</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>Цвет колонки/ресурса, hex (#RRGGBB).</summary>
    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
