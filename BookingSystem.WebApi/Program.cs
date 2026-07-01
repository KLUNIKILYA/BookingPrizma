using BookingSystem.Infrastructure;
using BookingSystem.Infrastructure.Configuration;
using BookingSystem.WebApi.Services;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = SqlServerConnectionFactory.Build(builder.Configuration);

var sqlInfo = new SqlConnectionStringBuilder(connectionString);
Console.WriteLine($"[BookingSystem] SQL DataSource={sqlInfo.DataSource}, Database={sqlInfo.InitialCatalog}");

builder.Services.AddBookingInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddScoped<BookingReportService>();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    dataSource = sqlInfo.DataSource,
    catalog = sqlInfo.InitialCatalog
}));

app.Run();
