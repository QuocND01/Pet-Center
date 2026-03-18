using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _route = "orders/Orders";

        public OrderServiceClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            // Đảm bảo tên "JWT" khớp với lúc bạn lưu khi Login
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<OrderResponseDTO>> GetAllAsync()
        {
            AddAuthHeader();
            return await _http.GetFromJsonAsync<List<OrderResponseDTO>>(_route) ?? new List<OrderResponseDTO>();
        }

        public async Task<OrderResponseDTO?> GetByIdAsync(Guid id)
        {
            AddAuthHeader();
            return await _http.GetFromJsonAsync<OrderResponseDTO>($"{_route}/{id}");
        }

        public async Task<bool> CreateAsync(OrderRequestDTO dto)
        {
            AddAuthHeader();
            var res = await _http.PostAsJsonAsync(_route, dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, OrderRequestDTO dto)
        {
            AddAuthHeader();
            var res = await _http.PutAsJsonAsync($"{_route}/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _http.DeleteAsync($"{_route}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<OrderDetailVM>> GetOrderDetailsAsync(Guid orderId)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                // Đã sửa: dùng {_route} thay vì chuỗi cứng có chữ gateway/
                var response = await _http.GetFromJsonAsync<List<OrderDetailVM>>($"{_route}/{orderId}/details");
                return response ?? new List<OrderDetailVM>();
            }
            catch
            {
                return new List<OrderDetailVM>();
            }
        }
    }
}