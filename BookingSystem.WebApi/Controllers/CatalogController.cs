using BookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups(CancellationToken ct)
        => Ok(await _catalog.GetGroupsAsync(ct));

    [HttpGet("services")]
    public async Task<IActionResult> GetServices([FromQuery] int? groupId, CancellationToken ct)
        => Ok(await _catalog.GetServicesAsync(groupId, ct));

    [HttpGet("tariffs")]
    public async Task<IActionResult> GetTariffs([FromQuery] int zoneId, CancellationToken ct)
        => Ok(await _catalog.GetTariffsAsync(zoneId, ct));

    [HttpGet("tickets")]
    public async Task<IActionResult> SearchTickets([FromQuery] string? search, [FromQuery] int take, CancellationToken ct)
        => Ok(await _catalog.SearchTicketsAsync(search, take, ct));

    [HttpGet("ticket-folders")]
    public async Task<IActionResult> GetTicketFolders(CancellationToken ct)
        => Ok(await _catalog.GetTicketFoldersAsync(ct));
}
