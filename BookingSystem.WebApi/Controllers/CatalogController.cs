using BookingSystem.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    /// <summary>Группы услуг («группа продаж»).</summary>
    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups(CancellationToken ct)
        => Ok(await _catalog.GetGroupsAsync(ct));

    /// <summary>Услуги. groupId не задан → полный список бронируемых услуг.</summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices([FromQuery] int? groupId, CancellationToken ct)
        => Ok(await _catalog.GetServicesAsync(groupId, ct));
}
