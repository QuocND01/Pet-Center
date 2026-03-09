using PetCenterClient.Services.Interface;
using System.Text;
using System.Text.Json;
using PetCenterClient.DTOs;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupplierService(HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Hàm helper để gán token cho BE
        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");

            if (!string.IsNullOrEmpty(token))
            {
                // Xóa các giá trị cũ để tránh cộng dồn header
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ReadSupplierDto>> GetAllAsync()
        {
            AddAuthorizationHeader();

            var res = await _httpClient.GetAsync("/inventory/Suppliers");
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ReadSupplierDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<ReadSupplierDto?> GetByIdAsync(Guid id)
        {
            AddAuthorizationHeader();

            var res = await _httpClient.GetAsync($"/inventory/Suppliers/{id}");

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ReadSupplierDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<ReadSupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PostAsJsonAsync("/inventory/Suppliers", dto);
            res.EnsureSuccessStatusCode();

            // Trả về object vừa tạo từ server
            return await res.Content.ReadFromJsonAsync<ReadSupplierDto>();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PutAsJsonAsync($"/inventory/Suppliers/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.DeleteAsync($"/inventory/Suppliers/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}