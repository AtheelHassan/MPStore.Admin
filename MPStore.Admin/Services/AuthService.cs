using System.Net.Http.Json;
using System.Text.Json;
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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AdminLoginResponse>(options);
        }

        var errorText = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(errorText))
        {
            return new AdminLoginResponse
            {
                Message = "فشل تسجيل الدخول."
            };
        }

        try
        {
            var errorResponse = JsonSerializer.Deserialize<AdminLoginResponse>(errorText, options);

            if (errorResponse != null)
                return errorResponse;
        }
        catch
        {
        }

        return new AdminLoginResponse
        {
            Message = "فشل تسجيل الدخول."
        };
    }
}