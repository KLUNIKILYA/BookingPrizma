using System.Net;
using System.Net.Http.Json;
using BookingSystem.Shared.Dtos;

namespace BookingSystem.WebUI.Services;

public class BookingSaveResult
{
    public BookingEventDto? Booking { get; set; }
    public List<BookingConflictDto> Conflicts { get; set; } = new();
    public string? Error { get; set; }
    public bool Ok => Booking is not null;
}

public class BookingApi
{
    private class ConflictResponse
    {
        public string? Error { get; set; }
        public List<BookingConflictDto>? Conflicts { get; set; }
    }

    private readonly HttpClient _http;

    public BookingApi(HttpClient http) => _http = http;

    public async Task<List<ResourceDto>> GetResourcesAsync() =>
        await _http.GetFromJsonAsync<List<ResourceDto>>("api/resources") ?? new();

    public async Task<List<ZoneTypeDto>> GetZoneTypesAsync() =>
        await _http.GetFromJsonAsync<List<ZoneTypeDto>>("api/resources/zone-types") ?? new();

    public async Task<List<ClientDto>> GetWaitersAsync() =>
        await _http.GetFromJsonAsync<List<ClientDto>>("api/resources/waiters") ?? new();

    public async Task<List<ServiceGroupDto>> GetGroupsAsync() =>
        await _http.GetFromJsonAsync<List<ServiceGroupDto>>("api/catalog/groups") ?? new();

    public async Task<List<ServiceDto>> GetServicesAsync(int? groupId)
    {
        var url = groupId.HasValue ? $"api/catalog/services?groupId={groupId}" : "api/catalog/services";
        return await _http.GetFromJsonAsync<List<ServiceDto>>(url) ?? new();
    }

    public async Task<List<TariffDto>> GetTariffsAsync(int zoneId) =>
        await _http.GetFromJsonAsync<List<TariffDto>>($"api/catalog/tariffs?zoneId={zoneId}") ?? new();

    public async Task<List<TicketDto>> SearchTicketsAsync(string search, int take = 20) =>
        await _http.GetFromJsonAsync<List<TicketDto>>(
            $"api/catalog/tickets?search={Uri.EscapeDataString(search)}&take={take}") ?? new();

    public async Task<List<TicketFolderDto>> GetTicketFoldersAsync() =>
        await _http.GetFromJsonAsync<List<TicketFolderDto>>("api/catalog/ticket-folders") ?? new();

    public async Task<List<ClientDto>> SearchClientsAsync(string search, int take = 20) =>
        await _http.GetFromJsonAsync<List<ClientDto>>(
            $"api/clients?search={Uri.EscapeDataString(search)}&take={take}") ?? new();

    public async Task<ClientDto?> CreateClientAsync(CreateClientRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/clients", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<ClientDto>() : null;
    }

    public async Task<List<BookingEventDto>> GetBookingsAsync(
        DateTime from, DateTime to, int? resourceId, int? groupId)
    {
        var url = $"api/bookings?from={from:s}&to={to:s}";
        if (resourceId.HasValue) url += $"&resourceId={resourceId}";
        if (groupId.HasValue) url += $"&groupId={groupId}";
        return await _http.GetFromJsonAsync<List<BookingEventDto>>(url) ?? new();
    }

    public async Task<BookingSaveResult> CreateAsync(BookingUpsertRequest req) =>
        await ParseSaveAsync(await _http.PostAsJsonAsync("api/bookings", req));

    public async Task<BookingSaveResult> UpdateAsync(int id, BookingUpsertRequest req) =>
        await ParseSaveAsync(await _http.PutAsJsonAsync($"api/bookings/{id}", req));

    private static async Task<BookingSaveResult> ParseSaveAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
            return new BookingSaveResult { Booking = await resp.Content.ReadFromJsonAsync<BookingEventDto>() };

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await resp.Content.ReadFromJsonAsync<ConflictResponse>();
            return new BookingSaveResult
            {
                Conflicts = body?.Conflicts ?? new(),
                Error = body?.Error ?? "Выбранное время уже занято."
            };
        }
        return new BookingSaveResult { Error = "Не удалось сохранить запись." };
    }

    public async Task<bool> DeleteAsync(int id) =>
        (await _http.DeleteAsync($"api/bookings/{id}")).IsSuccessStatusCode;

    public string ReportUrl(int id) =>
        new Uri(_http.BaseAddress!, $"api/bookings/{id}/report").ToString();

    public string ReportListUrl(DateTime from, DateTime to) =>
        new Uri(_http.BaseAddress!, $"api/bookings/report?from={from:s}&to={to:s}").ToString();

    public async Task<BookingSaveResult> CancelBookingAsync(int id) =>
        await ParseSaveAsync(await _http.PostAsync($"api/bookings/{id}/cancel", null));

    public async Task<BookingSaveResult> RestoreBookingAsync(int id) =>
        await ParseSaveAsync(await _http.PostAsync($"api/bookings/{id}/restore", null));

    public async Task<List<LabelDto>> GetLabelsAsync() =>
        await _http.GetFromJsonAsync<List<LabelDto>>("api/labels") ?? new();

    public async Task<LabelDto?> CreateLabelAsync(LabelUpsertRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/labels", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<LabelDto>() : null;
    }

    public async Task<LabelDto?> UpdateLabelAsync(int id, LabelUpsertRequest req)
    {
        var resp = await _http.PutAsJsonAsync($"api/labels/{id}", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<LabelDto>() : null;
    }

    public async Task<bool> DeleteLabelAsync(int id) =>
        (await _http.DeleteAsync($"api/labels/{id}")).IsSuccessStatusCode;

    public async Task<AvailabilityDto> GetAvailabilityAsync(DateTime from, DateTime to, int? excludeId)
    {
        var url = $"api/bookings/availability?from={from:s}&to={to:s}";
        if (excludeId.HasValue) url += $"&excludeId={excludeId}";
        return await _http.GetFromJsonAsync<AvailabilityDto>(url) ?? new();
    }

    public async Task<List<BookingConflictDto>> CheckConflictsAsync(
        int? excludeId, IEnumerable<int> resourceIds, DateTime from, DateTime to)
    {
        var ids = string.Concat(resourceIds.Select(id => $"&resourceIds={id}"));
        if (ids.Length == 0) return new();
        var url = $"api/bookings/conflicts?from={from:s}&to={to:s}{ids}";
        if (excludeId.HasValue) url += $"&excludeId={excludeId}";
        return await _http.GetFromJsonAsync<List<BookingConflictDto>>(url) ?? new();
    }
}
