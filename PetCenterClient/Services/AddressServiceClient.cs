using System.Net.Http.Headers;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class AddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string Route = "address-service/Addresses";

        public AddressServiceClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<AddressResponseDTO>> GetAllAsync()
        {
            AddAuthHeader();
            return await _http.GetFromJsonAsync<List<AddressResponseDTO>>(Route)
                   ?? new List<AddressResponseDTO>();
        }

        // Lấy tất cả address active của customer — dùng endpoint mới
        public async Task<List<AddressResponseDTO>> GetByCustomerIdAsync(Guid customerId)
        {
            AddAuthHeader();
            var response = await _http.GetAsync($"{Route}/customer/{customerId}");
            if (!response.IsSuccessStatusCode)
                return new List<AddressResponseDTO>();
            return await response.Content.ReadFromJsonAsync<List<AddressResponseDTO>>()
                   ?? new List<AddressResponseDTO>();
        }

        public async Task<AddressResponseDTO?> GetByIdAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _http.GetAsync($"{Route}/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AddressResponseDTO>()
                : null;
        }

        public async Task<bool> CreateAsync(AddressCreateDTO dto)
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync(Route, dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, AddressCreateDTO dto)
        {
            AddAuthHeader();
            var response = await _http.PutAsJsonAsync($"{Route}/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _http.DeleteAsync($"{Route}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}