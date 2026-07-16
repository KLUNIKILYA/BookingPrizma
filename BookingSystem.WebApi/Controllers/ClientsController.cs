using BookingSystem.Shared.Dtos;
using BookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients) => _clients = clients;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _clients.SearchAsync(search, take, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Surname) && string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Укажите фамилию или имя клиента." });
        return Ok(await _clients.CreateAsync(request, ct));
    }
}
