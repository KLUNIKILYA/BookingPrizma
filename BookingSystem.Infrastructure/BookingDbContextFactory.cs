using BookingSystem.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BookingSystem.Infrastructure;

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
