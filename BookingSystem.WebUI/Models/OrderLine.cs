namespace BookingSystem.WebUI.Models;

public class OrderLine
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsDone { get; set; }
    public bool IsTicket { get; set; }
}
