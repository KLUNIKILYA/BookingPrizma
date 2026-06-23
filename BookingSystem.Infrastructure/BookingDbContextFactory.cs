using BookingSystem.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BookingSystem.Infrastructure;

/// <summary>
/// Design-time фабрика для `dotnet ef`. Строку подключения берёт из переменных окружения
/// (SQL_HOST/SQL_PORT/SQL_INSTANCE/SQL_DATABASE/SQL_USER/SQL_PASSWORD) через SqlServerConnectionFactory.
/// По умолчанию — боевой сервер и база PPS_Prizma; SQL_PASSWORD обязателен.
/// </summary>
public class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var connection = SqlServerConnectionFactory.Build(config);

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlServer(connection, sql => sql.UseCompatibilityLevel(120))
            .Options;

        return new BookingDbContext(options);
    }
}
