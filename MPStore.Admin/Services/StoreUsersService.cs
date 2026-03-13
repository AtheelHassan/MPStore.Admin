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

    public async Task<StoreUserDetailsDto?> GetStoreUserAsync(long id)
    {
        return await _http.GetFromJsonAsync<StoreUserDetailsDto>($"api/storeusers/{id}");
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

public class StoreUserDetailsDto
{
    public StoreUserDto? User { get; set; }
    public List<StoreUserPermissionDto>? Permissions { get; set; }
    public List<StoreUserDocumentDto>? Documents { get; set; }
}

public class StoreUserPermissionDto
{
    public long Id { get; set; }
    public long StoreUserId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class StoreUserDocumentDto
{
    public long Id { get; set; }
    public long StoreUserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string? Notes { get; set; }
}