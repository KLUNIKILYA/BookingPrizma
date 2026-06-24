using BookingSystem.Shared;
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

    /// <summary>Пересечения брони по времени в указанных комнатах (для предупреждения в окне).</summary>
    [HttpGet("conflicts")]
    public async Task<IActionResult> Conflicts(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int[] resourceIds, [FromQuery] int? excludeId, CancellationToken ct)
    {
        if (to <= from) return BadRequest(new { error = "Параметр 'to' должен быть больше 'from'." });
        return Ok(await _bookings.CheckConflictsAsync(excludeId, resourceIds ?? Array.Empty<int>(), from, to, ct));
    }

    /// <summary>Занятость комнат и официантов на интервале (для блокировки в окне до сохранения).</summary>
    [HttpGet("availability")]
    public async Task<IActionResult> Availability(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? excludeId, CancellationToken ct)
    {
        if (to <= from) return Ok(new AvailabilityDto());
        return Ok(await _bookings.GetAvailabilityAsync(from, to, excludeId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingUpsertRequest request, CancellationToken ct)
    {
        if (request.ResourceId <= 0) return BadRequest(new { error = "Не выбран сотрудник." });
        try
        {
            var created = await _bookings.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { from = created.TimeFrom, to = created.TimeTo }, created);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new { error = ex.Message, conflicts = ex.Conflicts });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BookingUpsertRequest request, CancellationToken ct)
    {
        if (request.ResourceId <= 0) return BadRequest(new { error = "Не выбран сотрудник." });
        try
        {
            var updated = await _bookings.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new { error = ex.Message, conflicts = ex.Conflicts });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _bookings.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
