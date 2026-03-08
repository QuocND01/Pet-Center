using System.Net.Http.Headers;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class AddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Đường dẫn qua Gateway (phải khớp với cấu hình Ocelot/YARP của bạn)
        private readonly string _route = "address-service/Addresses";

        public AddressServiceClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // Hàm hỗ trợ đính kèm Token vào Header
        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<AddressResponseDTO>> GetAllAsync()
        {
            AddAuthHeader();
            return await _http.GetFromJsonAsync<List<AddressResponseDTO>>(_route)
                   ?? new List<AddressResponseDTO>();
        }

        public async Task<AddressResponseDTO?> GetByIdAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _http.GetAsync($"{_route}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AddressResponseDTO>();
            }
            return null;
        }

        public async Task<bool> CreateAsync(AddressCreateDTO dto)
        {
            AddAuthHeader();
            // CustomerId sẽ được API tự lấy từ Token, nhưng gửi kèm vẫn tốt
            var response = await _http.PostAsJsonAsync(_route, dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, AddressCreateDTO dto)
        {
            AddAuthHeader();
            var response = await _http.PutAsJsonAsync($"{_route}/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _http.DeleteAsync($"{_route}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}