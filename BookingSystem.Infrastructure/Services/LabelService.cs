using System.Text.RegularExpressions;
using BookingSystem.Domain.Entities;
using BookingSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class LabelService : ILabelService
{
    private readonly BookingDbContext _db;

    public LabelService(BookingDbContext db) => _db = db;

    public async Task<List<LabelDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var q = _db.Labels.AsNoTracking();
        if (!includeInactive) q = q.Where(l => l.IsActive);
        return await q.OrderBy(l => l.Id)
            .Select(l => new LabelDto { Id = l.Id, Name = l.Name, Color = l.Color })
            .ToListAsync(ct);
    }

    public async Task<LabelDto> CreateAsync(LabelUpsertRequest request, CancellationToken ct = default)
    {
        var nextId = Math.Max(5, await _db.Labels.MaxAsync(l => (int?)l.Id, ct) ?? 5) + 1;
        var e = new BookingLabelDef
        {
            Id = nextId,
            Name = (request.Name ?? "").Trim(),
            Color = NormalizeColor(request.Color),
            IsActive = true
        };
        _db.Labels.Add(e);
        await _db.SaveChangesAsync(ct);
        return new LabelDto { Id = e.Id, Name = e.Name, Color = e.Color };
    }

    public async Task<LabelDto?> UpdateAsync(int id, LabelUpsertRequest request, CancellationToken ct = default)
    {
        var e = await _db.Labels.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (e is null) return null;
        e.Name = (request.Name ?? "").Trim();
        e.Color = NormalizeColor(request.Color);
        e.IsActive = true;
        await _db.SaveChangesAsync(ct);
        return new LabelDto { Id = e.Id, Name = e.Name, Color = e.Color };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Labels.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (e is null) return false;
        e.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static string NormalizeColor(string? c)
    {
        c = (c ?? "").Trim();
        return Regex.IsMatch(c, "^#[0-9a-fA-F]{6}$") ? c.ToUpperInvariant() : "#E3E8EF";
    }
}
