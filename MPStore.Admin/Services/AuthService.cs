using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using MPStore.Admin.Models.Auth;

namespace MPStore.Admin.Services;

public class AuthService
{
    private const string AdminSessionKey = "AdminSession";
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

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return new AdminLoginResponse { Message = "فشل تسجيل الدخول." };

            try
            {
                return JsonSerializer.Deserialize<AdminLoginResponse>(responseText, options)
                       ?? new AdminLoginResponse { Message = "فشل تسجيل الدخول." };
            }
            catch
            {
                return new AdminLoginResponse { Message = "فشل تسجيل الدخول." };
            }
        }

        var loginResponse = string.IsNullOrWhiteSpace(responseText)
            ? null
            : JsonSerializer.Deserialize<AdminLoginResponse>(responseText, options);

        if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Token))
            return new AdminLoginResponse { Message = "فشل تسجيل الدخول." };

        var session = new AdminSession
        {
            Token = loginResponse.Token,
            ExpiresInMinutes = loginResponse.ExpiresInMinutes,
            Id = loginResponse.Admin?.Id ?? 0,
            Username = loginResponse.Admin?.Username ?? string.Empty,
            DisplayName = loginResponse.Admin?.DisplayName,
            Email = loginResponse.Admin?.Email,
            Role = loginResponse.Admin?.Role ?? 0,
            AdminRoleId = loginResponse.Admin?.AdminRoleId,
            AdminRoleName = loginResponse.Admin?.AdminRoleName,
            AdminRoleCode = loginResponse.Admin?.AdminRoleCode,
            Permissions = loginResponse.Admin?.Permissions ?? new List<string>(),
            IsActive = loginResponse.Admin?.IsActive ?? false,
            CreatedAtUtc = loginResponse.Admin?.CreatedAtUtc ?? DateTime.MinValue,
            LastLoginAtUtc = loginResponse.Admin?.LastLoginAtUtc
        };

        await SaveSessionAsync(session);
        return loginResponse;
    }

    public Task SaveSessionAsync(AdminSession session)
    {
        Preferences.Set(AdminSessionKey, JsonSerializer.Serialize(session));
        return Task.CompletedTask;
    }

    public Task<AdminSession?> GetSavedSessionAsync()
    {
        try
        {
            var json = Preferences.Get(AdminSessionKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult<AdminSession?>(null);

            var session = JsonSerializer.Deserialize<AdminSession>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Task.FromResult(session);
        }
        catch
        {
            return Task.FromResult<AdminSession?>(null);
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        var session = await GetSavedSessionAsync();
        return session?.Token;
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        var session = await GetSavedSessionAsync();
        if (session == null || session.Permissions.Count == 0)
            return false;

        return session.Permissions.Any(x =>
            string.Equals(x, permission, StringComparison.OrdinalIgnoreCase));
    }

    public Task LogoutAsync()
    {
        Preferences.Remove(AdminSessionKey);
        return Task.CompletedTask;
    }
}