using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MPStore.Admin.Models.AdminUsers;

namespace MPStore.Admin.Services
{
    public class AdminUsersService
    {
        private readonly HttpClient _http;

        public AdminUsersService(HttpClient http)
        {
            _http = http;
        }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<AdminUserDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/adminusers");

            if (!response.IsSuccessStatusCode)
                return new List<AdminUserDto>();

            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AdminUsersListResponse>(stream, _jsonOptions);

            return data?.Items ?? new List<AdminUserDto>();
        }

        public async Task<AdminUserDto?> GetByIdAsync(long id)
        {
            var response = await _http.GetAsync($"api/adminusers/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AdminUserDetailsResponse>(stream, _jsonOptions);

            return data?.Admin;
        }

        public async Task<bool> CreateAsync(CreateAdminUserRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/adminusers", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(long id, UpdateAdminUserRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/adminusers/{id}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var response = await _http.DeleteAsync($"api/adminusers/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetActiveAsync(long id, bool isActive)
        {
            var admin = await GetByIdAsync(id);
            if (admin == null)
                return false;

            var request = new UpdateAdminUserRequest
            {
                Username = admin.Username ?? string.Empty,
                Role = admin.Role,
                AdminRoleId = admin.AdminRoleId,
                DisplayName = admin.DisplayName,
                Email = admin.Email,
                IsActive = isActive
            };

            var response = await _http.PutAsJsonAsync($"api/adminusers/{id}", request);
            return response.IsSuccessStatusCode;
        }

        private sealed class AdminUsersListResponse
        {
            [JsonPropertyName("items")]
            public List<AdminUserDto>? Items { get; set; }
        }

        private sealed class AdminUserDetailsResponse
        {
            [JsonPropertyName("admin")]
            public AdminUserDto? Admin { get; set; }
        }
    }
}