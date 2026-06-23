using BookingSystem.Infrastructure.Services;
using BookingSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Регистрирует BookingDbContext и сервисы бронирования.</summary>
    public static IServiceCollection AddBookingInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // CompatibilityLevel(120): легаси SQL Server PPS_Prizma не поддерживает OPENJSON,
        // который EF Core 8 использует для Contains() — иначе ошибка «синтаксис около '$'».
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(120)));

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
