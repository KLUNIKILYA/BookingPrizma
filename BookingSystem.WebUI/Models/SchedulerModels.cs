namespace BookingSystem.WebUI.Models;

public class SchedulerEvent
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ResourceId { get; set; }

    public string CategoryColor { get; set; } = "#E3E8EF";

    public string RoomName { get; set; } = string.Empty;
    public string LabelName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? WaiterName { get; set; }
    public string ServicesSummary { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalPrice { get; set; }

    public bool IsCancelled { get; set; }
}

public class SchedulerResource
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Color { get; set; } = "#4F86C6";
}
