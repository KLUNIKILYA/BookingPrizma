namespace BookingSystem.WebUI.Models;

/// <summary>Плоская модель события для SfSchedule + поля для всплывающей подсказки.</summary>
public class SchedulerEvent
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ResourceId { get; set; }

    /// <summary>Цвет блока (по метке-статусу), hex.</summary>
    public string CategoryColor { get; set; } = "#E3E8EF";

    // Поля для tooltip / отображения
    public string RoomName { get; set; } = string.Empty;
    public string LabelName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? WaiterName { get; set; }
    public string ServicesSummary { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>Модель ресурса (сотрудник-колонка) для SfSchedule.</summary>
public class SchedulerResource
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Color { get; set; } = "#4F86C6";
}
