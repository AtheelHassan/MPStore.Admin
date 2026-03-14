using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MPStore.Admin.Models.AdminRoles;

namespace MPStore.Admin.Services
{
    public class AdminRolesService
    {
        private readonly HttpClient _http;

        public AdminRolesService(HttpClient http)
        {
            _http = http;
        }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<AdminRoleDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/adminroles");

            if (!response.IsSuccessStatusCode)
                return new List<AdminRoleDto>();

            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AdminRolesListResponse>(stream, _jsonOptions);

            return data?.Items ?? new List<AdminRoleDto>();
        }

        public async Task<AdminRoleDto?> GetByIdAsync(long id)
        {
            var response = await _http.GetAsync($"api/adminroles/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AdminRoleDetailsResponse>(stream, _jsonOptions);

            return data?.Role;
        }

        public async Task<bool> CreateAsync(CreateAdminRoleRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/adminroles", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(long id, UpdateAdminRoleRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/adminroles/{id}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var response = await _http.DeleteAsync($"api/adminroles/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<AdminRolePermissionItemRequest>> GetPermissionsAsync(long id)
        {
            var response = await _http.GetAsync($"api/adminroles/{id}/permissions");

            if (!response.IsSuccessStatusCode)
                return new List<AdminRolePermissionItemRequest>();

            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<List<AdminRolePermissionItemRequest>>(stream, _jsonOptions);

            return data ?? new List<AdminRolePermissionItemRequest>();
        }

        public async Task<bool> UpdatePermissionsAsync(long id, List<AdminRolePermissionItemRequest> request)
        {
            var response = await _http.PutAsJsonAsync($"api/adminroles/{id}/permissions", request);
            return response.IsSuccessStatusCode;
        }

        private sealed class AdminRolesListResponse
        {
            [JsonPropertyName("items")]
            public List<AdminRoleDto>? Items { get; set; }
        }

        private sealed class AdminRoleDetailsResponse
        {
            [JsonPropertyName("role")]
            public AdminRoleDto? Role { get; set; }
        }
    }
}