using System.Net.Http.Json;
using MPStore.Admin.Models.StoreRoles;

namespace MPStore.Admin.Services;

public class StoreRolesService
{
    private readonly HttpClient _http;

    public StoreRolesService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<StoreRoleDto>?> GetStoreRolesAsync(long storeId, bool onlyActive = true)
    {
        return await _http.GetFromJsonAsync<List<StoreRoleDto>>(
            $"api/storeroles/store/{storeId}?onlyActive={onlyActive.ToString().ToLower()}");
    }

    public async Task<StoreRoleDetailsDto?> GetStoreRoleAsync(long id)
    {
        return await _http.GetFromJsonAsync<StoreRoleDetailsDto>($"api/storeroles/{id}");
    }
}