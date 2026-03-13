using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<string?> UploadLogoAsync(string filePath, string? storeSlug = null, long? storeId = null)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            await using var fileStream = File.OpenRead(filePath);
            using var fileContent = new StreamContent(fileStream);

            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            content.Add(fileContent, "file", Path.GetFileName(filePath));

            if (!string.IsNullOrWhiteSpace(storeSlug))
                content.Add(new StringContent(storeSlug.Trim()), "storeSlug");

            if (storeId.HasValue && storeId.Value > 0)
                content.Add(new StringContent(storeId.Value.ToString()), "storeId");

            var response = await _http.PostAsync("api/stores/upload-logo", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<UploadLogoResponse>();

            return result?.LogoPath;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ServiceResult> CreateStoreAsync(CreateStoreRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/stores", request);

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

    public async Task<ServiceResult> UpdateStoreAsync(UpdateStoreRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/stores/{request.StoreId}", request);

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

    public async Task<ServiceResult> DeleteStoreAsync(long storeId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/stores/{storeId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (!string.IsNullOrWhiteSpace(apiResponse?.Message))
                        return ServiceResult.Success(apiResponse.Message!);
                }

                return ServiceResult.Success("تم حذف المتجر بنجاح.");
            }

            var message = await ReadErrorMessageAsync(response);
            return ServiceResult.Fail(message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"تعذر الاتصال بالخادم: {ex.Message}");
        }
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

    private sealed class StoresListResponse
    {
        [JsonPropertyName("items")]
        public List<StoreDto>? Items { get; set; }
    }

    private sealed class UploadLogoResponse
    {
        [JsonPropertyName("logoPath")]
        public string? LogoPath { get; set; }
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