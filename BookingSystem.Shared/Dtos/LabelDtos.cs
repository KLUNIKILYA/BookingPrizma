namespace BookingSystem.Shared.Dtos;

public class LabelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#E3E8EF";
}

public class LabelUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#E3E8EF";
}
