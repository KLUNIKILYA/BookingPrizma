using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookingSystem.Infrastructure;

/// <summary>
/// Design-time фабрика для команд `dotnet ef` (миграции). По умолчанию использует
/// локальную БД PPS_3_1 (Windows-аутентификация); можно переопределить через
/// переменную окружения BOOKING_CONNECTION.
/// </summary>
public class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public const string LocalConnectionString =
        "Server=localhost;Database=PPS_3_1;Trusted_Connection=True;TrustServerCertificate=True;";

    public BookingDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BOOKING_CONNECTION")
                         ?? LocalConnectionString;

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new BookingDbContext(options);
    }
}
