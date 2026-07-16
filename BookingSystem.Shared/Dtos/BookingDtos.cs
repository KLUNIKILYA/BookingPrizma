using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared.Dtos;

public class BookingServiceLineDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsDone { get; set; }
    public bool IsTicket { get; set; }
}

public class BookingEventDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }

    public BookingLabel Label { get; set; }
    public string LabelName { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string? Note { get; set; }
    public int? ClientVisitorId { get; set; }
    public string? ClientName { get; set; }

    public int? WaiterVisitorId { get; set; }
    public string? WaiterName { get; set; }

    public List<BookingServiceLineDto> Services { get; set; } = new();
    public decimal TotalPrice { get; set; }

    public int? TariffTicketId { get; set; }
    public string? TariffName { get; set; }
    public decimal? TariffPrice { get; set; }

    public string? CelebrantName { get; set; }
    public DateTime? CelebrantBirthDate { get; set; }

    public bool IsPrepaid { get; set; }
    public decimal? PrepaidAmount { get; set; }

    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    public bool CanEdit { get; set; }
}

public class BookingConflictDto
{
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public int ExistingId { get; set; }
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string? Title { get; set; }
}

public class BusySlotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string? Title { get; set; }
}

public class AvailabilityDto
{
    public List<BusySlotDto> Rooms { get; set; } = new();
    public List<BusySlotDto> Waiters { get; set; } = new();
}

public class BookingServiceSelection
{
    public int ServiceId { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsDone { get; set; }
    public bool IsTicket { get; set; }
}

public class BookingUpsertRequest
{
    public int ResourceId { get; set; }

    public DateTime TimeFrom { get; set; }

    public DateTime? TimeToOverride { get; set; }

    public string Title { get; set; } = string.Empty;
    public int? ClientVisitorId { get; set; }
    public int? WaiterVisitorId { get; set; }
    public BookingLabel Label { get; set; } = BookingLabel.None;
    public string? Note { get; set; }

    public string? CelebrantName { get; set; }
    public DateTime? CelebrantBirthDate { get; set; }

    public bool IsPrepaid { get; set; }
    public decimal? PrepaidAmount { get; set; }

    public List<BookingServiceSelection> Services { get; set; } = new();
}
