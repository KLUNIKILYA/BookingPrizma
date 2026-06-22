using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    /// <summary>Брони в диапазоне [from; to) для планировщика.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int? resourceId, [FromQuery] int? groupId, CancellationToken ct)
    {
        if (to <= from) return BadRequest(new { error = "Параметр 'to' должен быть больше 'from'." });
        return Ok(await _bookings.GetBookingsAsync(from, to, resourceId, groupId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingUpsertRequest request, CancellationToken ct)
    {
        if (request.ResourceId <= 0) return BadRequest(new { error = "Не выбран сотрудник." });
        var created = await _bookings.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { from = created.TimeFrom, to = created.TimeTo }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BookingUpsertRequest request, CancellationToken ct)
    {
        if (request.ResourceId <= 0) return BadRequest(new { error = "Не выбран сотрудник." });
        var updated = await _bookings.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _bookings.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
