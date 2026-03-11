using System.Net.Http.Json;
using MPStore.Admin.Models.StoreUsers;

namespace MPStore.Admin.Services;

public class StoreUsersService
{
    private readonly HttpClient _http;

    public StoreUsersService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<StoreUserDto>?> GetStoreUsersAsync(long storeId)
    {
        return await _http.GetFromJsonAsync<List<StoreUserDto>>($"api/storeusers/store/{storeId}");
    }

    public async Task<bool> CreateStoreUserAsync(CreateStoreUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/storeusers", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStoreUserAsync(UpdateStoreUserRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/storeusers/{request.StoreUserId}", request);
        return response.IsSuccessStatusCode;
    }
}