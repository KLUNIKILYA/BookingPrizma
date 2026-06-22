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
        services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
