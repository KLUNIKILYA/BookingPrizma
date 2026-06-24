using System.Net;
using System.Net.Http.Json;
using BookingSystem.Shared.Dtos;

namespace BookingSystem.WebUI.Services;

/// <summary>Результат сохранения брони: успех, либо конфликт по времени (409), либо ошибка.</summary>
public class BookingSaveResult
{
    public BookingEventDto? Booking { get; set; }
    public List<BookingConflictDto> Conflicts { get; set; } = new();
    public string? Error { get; set; }
    public bool Ok => Booking is not null;
}

/// <summary>Типизированный клиент WebApi бронирования.</summary>
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

    public async Task<List<ClientDto>> GetWaitersAsync() =>
        await _http.GetFromJsonAsync<List<ClientDto>>("api/resources/waiters") ?? new();

    public async Task<List<ServiceGroupDto>> GetGroupsAsync() =>
        await _http.GetFromJsonAsync<List<ServiceGroupDto>>("api/catalog/groups") ?? new();

    public async Task<List<ServiceDto>> GetServicesAsync(int? groupId)
    {
        var url = groupId.HasValue ? $"api/catalog/services?groupId={groupId}" : "api/catalog/services";
        return await _http.GetFromJsonAsync<List<ServiceDto>>(url) ?? new();
    }

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

    /// <summary>Занятость комнат и официантов на интервале (для блокировки в окне).</summary>
    public async Task<AvailabilityDto> GetAvailabilityAsync(DateTime from, DateTime to, int? excludeId)
    {
        var url = $"api/bookings/availability?from={from:s}&to={to:s}";
        if (excludeId.HasValue) url += $"&excludeId={excludeId}";
        return await _http.GetFromJsonAsync<AvailabilityDto>(url) ?? new();
    }

    /// <summary>Пересечения по времени в указанных комнатах (исключая бронь excludeId).</summary>
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
