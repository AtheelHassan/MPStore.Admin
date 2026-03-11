using System.Net.Http.Json;
using System.Text.Json.Serialization;
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
        var result = await _http.GetFromJsonAsync<StoresListResponse>("api/stores");
        return result?.Items ?? new List<StoreDto>();
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

    private sealed class StoresListResponse
    {
        [JsonPropertyName("items")]
        public List<StoreDto>? Items { get; set; }
    }
}