using BookingSystem.Shared.Dtos;
using BookingSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabelsController : ControllerBase
{
    private readonly ILabelService _labels;

    public LabelsController(ILabelService labels) => _labels = labels;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeInactive, CancellationToken ct)
        => Ok(await _labels.GetAllAsync(includeInactive, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LabelUpsertRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Название метки обязательно." });
        return Ok(await _labels.CreateAsync(request, ct));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LabelUpsertRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Название метки обязательно." });
        var updated = await _labels.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _labels.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
