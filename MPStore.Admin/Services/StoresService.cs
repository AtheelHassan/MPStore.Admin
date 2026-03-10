using System.Net.Http.Json;
using MPStore.Admin.Models.Stores;

namespace MPStore.Admin.Services;

public class StoresService
{
    private readonly HttpClient _http;

    public StoresService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<StoreDto>?> GetStoresAsync()
    {
        return await _http.GetFromJsonAsync<List<StoreDto>>("api/stores");
    }

    public async Task<StoreDto?> GetStoreAsync(long storeId)
    {
        return await _http.GetFromJsonAsync<StoreDto>($"api/stores/{storeId}");
    }

    public async Task<bool> CreateStoreAsync(CreateStoreRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/stores", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStoreAsync(UpdateStoreRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/stores/{request.StoreId}", request);
        return response.IsSuccessStatusCode;
    }
}