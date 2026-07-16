using BookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(120)));

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ILabelService, LabelService>();

        return services;
    }
}
