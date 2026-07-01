using System.Data;
using BookingSystem.Shared.Dtos;
using FastReport;
using FastReport.Export.PdfSimple;

namespace BookingSystem.WebApi.Services;

/// <summary>
/// Генерация PDF-отчёта по брони из шаблона FastReport (Reports/booking.frx).
/// Шаблон рисуется в визуальном FastReport Designer и правится без пересборки —
/// важно только сохранять имена: источник данных "Services" (Name/Qty/Price/Sum)
/// и параметры Number/Room/Period/Client/Waiter/Celebrant/Tariff/Total/Prepaid.
/// </summary>
public class BookingReportService
{
    private readonly string _templatePath;

    public BookingReportService(IWebHostEnvironment env) =>
        _templatePath = Path.Combine(env.ContentRootPath, "Reports", "booking.frx");

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
}
