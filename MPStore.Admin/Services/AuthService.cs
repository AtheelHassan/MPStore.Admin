using System.Net.Http.Json;
using MPStore.Admin.Models.Auth;

namespace MPStore.Admin.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/admin-auth/login", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
    }
}