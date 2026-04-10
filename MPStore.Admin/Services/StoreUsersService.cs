using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MPStore.Admin.Models.StoreUsers;

namespace MPStore.Admin.Services;

public class StoreUsersService
{
    private readonly HttpClient _http;
    private readonly AuthService _authService;

    public StoreUsersService(HttpClient http, AuthService authService)
    {
        _http = http;
        _authService = authService;
    }

    public async Task<List<StoreUserDto>?> GetStoreUsersAsync(long storeId)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/storeusers/store/{storeId}");
        using var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new List<StoreUserDto>();

        return await response.Content.ReadFromJsonAsync<List<StoreUserDto>>();
    }

    public async Task<StoreUserDetailsDto?> GetStoreUserAsync(long id)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"api/storeusers/{id}");
        using var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<StoreUserDetailsDto>();
    }

    public async Task<ServiceResult> CreateStoreUserAsync(CreateStoreUserRequest requestModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(requestModel);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/storeusers", content);
            using var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return ServiceResult.Success();

            var message = await ReadErrorMessageAsync(response);
            return ServiceResult.Fail(message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"تعذر الاتصال بالخادم: {ex.Message}");
        }
    }

    public async Task<ServiceResult> UpdateStoreUserAsync(UpdateStoreUserRequest requestModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(requestModel);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, $"api/storeusers/{requestModel.StoreUserId}", content);
            using var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return ServiceResult.Success();

            var message = await ReadErrorMessageAsync(response);
            return ServiceResult.Fail(message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"تعذر الاتصال بالخادم: {ex.Message}");
        }
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url);

        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (content != null)
            request.Content = content;

        return request;
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return $"فشل الطلب. رمز الحالة: {(int)response.StatusCode}";

            var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(apiError?.Message))
                return apiError.Message!;

            if (!string.IsNullOrWhiteSpace(apiError?.Title))
                return apiError.Title!;

            return content;
        }
        catch
        {
            return $"فشل الطلب. رمز الحالة: {(int)response.StatusCode}";
        }
    }

    private sealed class ApiErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public sealed class ServiceResult
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;

        public static ServiceResult Success(string message = "")
            => new() { IsSuccess = true, Message = message };

        public static ServiceResult Fail(string message)
            => new() { IsSuccess = false, Message = message };
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