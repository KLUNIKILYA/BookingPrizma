using BookingSystem.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resources;

    public ResourcesController(IResourceService resources) => _resources = resources;

    /// <summary>Ресурсы (комнаты/столики) для колонок планировщика и фильтра.</summary>
    [HttpGet]
    public async Task<IActionResult> GetResources(CancellationToken ct)
        => Ok(await _resources.GetResourcesAsync(ct));

    /// <summary>Официанты (сотрудники) для привязки к брони.</summary>
    [HttpGet("waiters")]
    public async Task<IActionResult> GetWaiters(CancellationToken ct)
        => Ok(await _resources.GetWaitersAsync(ct));
}
