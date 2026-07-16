using BookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resources;

    public ResourcesController(IResourceService resources) => _resources = resources;

    [HttpGet]
    public async Task<IActionResult> GetResources(CancellationToken ct)
        => Ok(await _resources.GetResourcesAsync(ct));

    [HttpGet("zone-types")]
    public async Task<IActionResult> GetZoneTypes(CancellationToken ct)
        => Ok(await _resources.GetZoneTypesAsync(ct));

    [HttpGet("waiters")]
    public async Task<IActionResult> GetWaiters(CancellationToken ct)
        => Ok(await _resources.GetWaitersAsync(ct));
}
