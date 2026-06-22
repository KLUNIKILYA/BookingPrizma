using System.Net.Http.Json;
using BookingSystem.Shared.Dtos;

namespace BookingSystem.WebUI.Services;

/// <summary>Типизированный клиент WebApi бронирования.</summary>
public class BookingApi
{
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

    public async Task<BookingEventDto?> CreateAsync(BookingUpsertRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/bookings", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<BookingEventDto>() : null;
    }

    public async Task<BookingEventDto?> UpdateAsync(int id, BookingUpsertRequest req)
    {
        var resp = await _http.PutAsJsonAsync($"api/bookings/{id}", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<BookingEventDto>() : null;
    }

    public async Task<bool> DeleteAsync(int id) =>
        (await _http.DeleteAsync($"api/bookings/{id}")).IsSuccessStatusCode;
}
