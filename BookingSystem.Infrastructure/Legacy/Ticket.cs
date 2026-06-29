namespace BookingSystem.Infrastructure.Legacy;

/// <summary>
/// Read-only проекция dbo.Ticket — билеты. Часть из них — тарифы на бронь комнат
/// (связь с зоной и длительность лежат в <see cref="TicketZone"/>), часть — билеты-услуги
/// (взрослый/детский/сопровождающий) с ценой, без времени. Не участвует в миграциях.
/// </summary>
public class Ticket
{
    public int IdTicket { get; set; }
    public string NameTicket { get; set; } = null!;
    public bool Active { get; set; }

    /// <summary>Цена за единицу. В этой БД заполнен именно TotalPrice (OnePrice = 0).</summary>
    public decimal TotalPrice { get; set; }
    public decimal OnePrice { get; set; }
}
