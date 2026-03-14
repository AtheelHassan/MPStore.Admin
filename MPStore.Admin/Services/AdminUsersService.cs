using System.Net.Http.Json;
using System.Text.Json;
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
            var response = await _http.GetAsync("api/admin-users");

            if (!response.IsSuccessStatusCode)
                return new List<AdminUserDto>();

            var stream = await response.Content.ReadAsStreamAsync();

            var data = await JsonSerializer.DeserializeAsync<List<AdminUserDto>>(stream, _jsonOptions);

            return data ?? new List<AdminUserDto>();
        }

        public async Task<AdminUserDto?> GetByIdAsync(long id)
        {
            var response = await _http.GetAsync($"api/admin-users/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<AdminUserDto>(stream, _jsonOptions);
        }

        public async Task<bool> CreateAsync(CreateAdminUserRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/admin-users", request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(long id, UpdateAdminUserRequest request)
        {
            var response = await _http.PutAsJsonAsync($"api/admin-users/{id}", request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var response = await _http.DeleteAsync($"api/admin-users/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetActiveAsync(long id, bool isActive)
        {
            var response = await _http.PutAsync($"api/admin-users/{id}/active/{isActive}", null);

            return response.IsSuccessStatusCode;
        }
    }
}