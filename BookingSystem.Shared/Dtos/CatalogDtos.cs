namespace BookingSystem.Shared.Dtos;

public class ServiceGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int GroupId { get; set; }
}

public class TariffDto
{
    public int TicketId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Minutes { get; set; }
    public decimal Price { get; set; }
}

public class TicketDto
{
    public int TicketId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class TicketFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TicketDto> Tickets { get; set; } = new();
}

public class ResourceDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int SortOrder { get; set; }

    public int? ZoneTypeId { get; set; }
    public string? ZoneTypeName { get; set; }
}

public class ZoneTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<ResourceDto> Zones { get; set; } = new();
}

public class ClientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CreateClientRequest
{
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
