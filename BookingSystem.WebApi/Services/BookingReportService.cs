using System.Data;
using BookingSystem.Shared.Dtos;
using FastReport;
using FastReport.Export.PdfSimple;

namespace BookingSystem.WebApi.Services;

public class BookingReportService
{
    private readonly string _templatePath;
    private readonly string _listTemplatePath;

    public BookingReportService(IWebHostEnvironment env)
    {
        _templatePath = Path.Combine(env.ContentRootPath, "Reports", "booking.frx");
        _listTemplatePath = Path.Combine(env.ContentRootPath, "Reports", "bookings-list.frx");
    }

    public byte[] BuildBookingPdf(BookingEventDto b, string roomName)
    {
        var services = new DataTable("Services");
        services.Columns.Add("Name", typeof(string));
        services.Columns.Add("Qty", typeof(int));
        services.Columns.Add("Price", typeof(decimal));
        services.Columns.Add("Sum", typeof(decimal));
        foreach (var s in b.Services)
            services.Rows.Add(s.ServiceName, s.Quantity, s.Price, s.Price * s.Quantity);

        using var report = new Report();
        report.Load(_templatePath);
        report.RegisterData(services, "Services");
        report.GetDataSource("Services").Enabled = true;

        var period = b.TimeFrom.Date == b.TimeTo.Date
            ? $"{b.TimeFrom:dd.MM.yyyy HH:mm}–{b.TimeTo:HH:mm}"
            : $"{b.TimeFrom:dd.MM.yyyy HH:mm} – {b.TimeTo:dd.MM.yyyy HH:mm}";

        var celebrant = "—";
        if (!string.IsNullOrWhiteSpace(b.CelebrantName))
            celebrant = b.CelebrantBirthDate is { } bd
                ? $"{b.CelebrantName} (д.р. {bd:dd.MM.yyyy})"
                : b.CelebrantName!;

        var client = !string.IsNullOrWhiteSpace(b.ClientName) ? b.ClientName!
            : (!string.IsNullOrWhiteSpace(b.Title) ? b.Title : "—");

        report.SetParameterValue("Number", b.Id.ToString());
        report.SetParameterValue("Room", roomName);
        report.SetParameterValue("Period", period);
        report.SetParameterValue("Client", client);
        report.SetParameterValue("Waiter", string.IsNullOrWhiteSpace(b.WaiterName) ? "—" : b.WaiterName!);
        report.SetParameterValue("Celebrant", celebrant);
        report.SetParameterValue("Tariff", string.IsNullOrWhiteSpace(b.TariffName) ? "—" : b.TariffName!);
        report.SetParameterValue("Total", b.TotalPrice.ToString("0.00"));
        report.SetParameterValue("Prepaid", b.IsPrepaid ? (b.PrepaidAmount ?? 0m).ToString("0.00") : "—");

        report.Prepare();

        using var ms = new MemoryStream();
        report.Export(new PDFSimpleExport(), ms);
        return ms.ToArray();
    }

    public byte[] BuildBookingsListPdf(
        IReadOnlyList<BookingEventDto> bookings, DateTime from, DateTime to, IReadOnlyDictionary<int, string> roomNames)
    {
        var table = new DataTable("Bookings");
        table.Columns.Add("DateTime", typeof(string));
        table.Columns.Add("Room", typeof(string));
        table.Columns.Add("Client", typeof(string));
        table.Columns.Add("Waiter", typeof(string));
        table.Columns.Add("Status", typeof(string));

        foreach (var b in bookings.OrderBy(x => x.TimeFrom))
        {
            var room = roomNames.TryGetValue(b.ResourceId, out var rn) ? rn : $"#{b.ResourceId}";
            var client = !string.IsNullOrWhiteSpace(b.ClientName) ? b.ClientName! : b.Title;
            var status = b.IsCancelled ? "✖ Отменена" : b.LabelName;
            table.Rows.Add(
                $"{b.TimeFrom:dd.MM.yyyy HH:mm}–{b.TimeTo:HH:mm}",
                room, client, b.WaiterName ?? "—", status);
        }

        using var report = new Report();
        report.Load(_listTemplatePath);
        report.RegisterData(table, "Bookings");
        report.GetDataSource("Bookings").Enabled = true;

        report.SetParameterValue("Period", $"{from:dd.MM.yyyy} – {to.AddDays(-1):dd.MM.yyyy}");
        report.SetParameterValue("Total", bookings.Count.ToString());
        report.SetParameterValue("Active", bookings.Count(b => !b.IsCancelled).ToString());
        report.SetParameterValue("Cancelled", bookings.Count(b => b.IsCancelled).ToString());

        report.Prepare();

        using var ms = new MemoryStream();
        report.Export(new PDFSimpleExport(), ms);
        return ms.ToArray();
    }
}
