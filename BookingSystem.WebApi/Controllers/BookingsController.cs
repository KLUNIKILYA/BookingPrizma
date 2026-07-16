using BookingSystem.Shared;
using BookingSystem.Shared.Dtos;
using BookingSystem.Infrastructure.Services;
using BookingSystem.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> Report(
        int id,
        [FromServices] BookingReportService reports,
        [FromServices] IResourceService resources,
        CancellationToken ct)
    {
        var dto = await _bookings.GetByIdAsync(id, ct);
        if (dto is null) return NotFound();

        var roomName = (await resources.GetResourcesAsync(ct))
            .FirstOrDefault(r => r.Id == dto.ResourceId)?.DisplayName ?? $"#{dto.ResourceId}";

        var pdf = reports.BuildBookingPdf(dto, roomName);
        Response.Headers.ContentDisposition = $"inline; filename=booking-{id}.pdf";
        return File(pdf, "application/pdf");
    }

    [HttpGet("report")]
    public async Task<IActionResult> ReportList(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromServices] BookingReportService reports,
        [FromServices] IResourceService resources,
        CancellationToken ct)
    {
        if (to <= from) return BadRequest(new { error = "Параметр 'to' должен быть больше 'from'." });

        var bookings = await _bookings.GetBookingsAsync(from, to, null, null, ct);
        var roomNames = (await resources.GetResourcesAsync(ct)).ToDictionary(r => r.Id, r => r.DisplayName);

        var pdf = reports.BuildBookingsListPdf(bookings, from, to, roomNames);
        Response.Headers.ContentDisposition =
            $"attachment; filename=bookings-report_{from:yyyy-MM-dd}_{to.AddDays(-1):yyyy-MM-dd}.pdf";
        return File(pdf, "application/pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int? resourceId, [FromQuery] int? groupId, CancellationToken ct)
    {
        if (to <= from) return BadRequest(new { error = "Параметр 'to' должен быть больше 'from'." });
        return Ok(await _bookings.GetBookingsAsync(from, to, resourceId, groupId, ct));
    }

    [HttpGet("conflicts")]
    public async Task<IActionResult> Conflicts(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int[] resourceIds, [FromQuery] int? excludeId, CancellationToken ct)
    {
        if (to <= from) return BadRequest(new { error = "Параметр 'to' должен быть больше 'from'." });
        return Ok(await _bookings.CheckConflictsAsync(excludeId, resourceIds ?? Array.Empty<int>(), from, to, ct));
    }

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

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var dto = await _bookings.CancelAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        try
        {
            var dto = await _bookings.RestoreAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new { error = ex.Message, conflicts = ex.Conflicts });
        }
    }
}
