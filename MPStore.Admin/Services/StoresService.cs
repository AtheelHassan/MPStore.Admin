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