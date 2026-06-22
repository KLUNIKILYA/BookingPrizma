using BookingSystem.Infrastructure.Legacy;
using BookingSystem.Shared.Dtos;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

/// <summary>Поиск клиентов в БД (легаси CashboxVisitor) для «Привязка к БД клиентов».</summary>
public class ClientService : IClientService
{
    private readonly BookingDbContext _db;

    public ClientService(BookingDbContext db) => _db = db;

    public async Task<List<ClientDto>> SearchAsync(string? search, int take = 20, CancellationToken ct = default)
    {
        // Без строки поиска не отдаём 8800+ клиентов.
        if (string.IsNullOrWhiteSpace(search))
            return new List<ClientDto>();

        var s = search.Trim();
        if (take is <= 0 or > 100) take = 20;

        return await _db.CashboxVisitors.AsNoTracking()
            .Where(v => v.Active &&
                        (v.Surname.Contains(s) || v.Name.Contains(s) ||
                         (v.Info != null && v.Info.Contains(s))))
            .OrderBy(v => v.Surname).ThenBy(v => v.Name)
            .Take(take)
            .Select(v => new ClientDto
            {
                Id = v.IdVisitor,
                FullName = (v.Surname + " " + v.Name).Trim(),
                Phone = v.Info
            })
            .ToListAsync(ct);
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        // Обязательные NOT NULL без default: Surname, Name, MiddleName, SotrudnikStatus.
        // Остальное (TrainerStatus/ResidentStatus/Active) берёт значения по умолчанию из БД.
        var visitor = new CashboxVisitor
        {
            Surname = (request.Surname ?? string.Empty).Trim(),
            Name = (request.Name ?? string.Empty).Trim(),
            MiddleName = string.Empty,
            Info = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            SotrudnikStatus = false,
            Active = true
        };

        _db.CashboxVisitors.Add(visitor);
        await _db.SaveChangesAsync(ct);

        return new ClientDto
        {
            Id = visitor.IdVisitor,
            FullName = (visitor.Surname + " " + visitor.Name).Trim(),
            Phone = visitor.Info
        };
    }
}
