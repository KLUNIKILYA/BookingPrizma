using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BookingSystem.Infrastructure.Configuration;

/// <summary>
/// Строит строку подключения к SQL Server. Если задан ConnectionStrings:DefaultConnection
/// с именованным инстансом — берёт его как есть. Иначе собирает из переменных
/// SQL_HOST / SQL_PORT / SQL_INSTANCE / SQL_DATABASE / SQL_USER / SQL_PASSWORD
/// (по умолчанию — боевой сервер и база PPS_Prizma).
/// </summary>
public static class SqlServerConnectionFactory
{
    public static string Build(IConfiguration configuration)
    {
        var explicitConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(explicitConnection) && explicitConnection.Contains('\\'))
            return explicitConnection;

        var host = configuration["SQL_HOST"] ?? "86.57.242.48";
        var port = configuration["SQL_PORT"] ?? "64978";
        var instance = configuration["SQL_INSTANCE"] ?? "SQLEXPRESS";
        var database = configuration["SQL_DATABASE"] ?? "PPS_Prizma_2026_07_01";
        var user = configuration["SQL_USER"] ?? "sa";
        var password = configuration["SQL_PASSWORD"]
            ?? throw new InvalidOperationException("SQL_PASSWORD не задан (env или appsettings).");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{host}\\{instance},{port}",
            InitialCatalog = database,
            UserID = user,
            Password = password,
            TrustServerCertificate = true,
            Encrypt = false
        };
        return builder.ConnectionString;
    }
}
