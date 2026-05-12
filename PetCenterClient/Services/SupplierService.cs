using PetCenterClient.Services.Interface;
using PetCenterClient.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PetCenterClient.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupplierService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("JWT");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ReadSupplierDto>> GetAllAsync()
        {
            AddAuthorizationHeader();

            var res = await _httpClient.GetAsync("/suppliers");

            res.EnsureSuccessStatusCode();

            var result = await res.Content
                .ReadFromJsonAsync<ApiResponse<List<ReadSupplierDto>>>();

            return result?.Data ?? new List<ReadSupplierDto>();
        }

        public async Task<ReadSupplierDto?> GetByIdAsync(Guid id)
        {
            AddAuthorizationHeader();

            var res = await _httpClient.GetAsync($"/suppliers/{id}");

            if (!res.IsSuccessStatusCode)
                return null;

            var result = await res.Content
                .ReadFromJsonAsync<ApiResponse<ReadSupplierDto>>();

            return result?.Data;
        }

        public async Task<ReadSupplierDto?> CreateAsync(CreateSupplierDto dto)
        {
            AddAuthorizationHeader();

            var res = await _httpClient
                .PostAsJsonAsync("/suppliers", dto);

            res.EnsureSuccessStatusCode();

            var result = await res.Content
                .ReadFromJsonAsync<ApiResponse<ReadSupplierDto>>();

            return result?.Data;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            AddAuthorizationHeader();

            var res = await _httpClient
                .PutAsJsonAsync($"/suppliers/{id}", dto);

            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthorizationHeader();

            var res = await _httpClient
                .DeleteAsync($"/suppliers/{id}");

            return res.IsSuccessStatusCode;
        }

        public async Task<List<SupplierSelectDto>> GetSupplierSelectAsync()
        {
            var suppliers = await GetAllAsync();

            return suppliers.Select(s => new SupplierSelectDto
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName
            }).ToList();
        }
    }
}