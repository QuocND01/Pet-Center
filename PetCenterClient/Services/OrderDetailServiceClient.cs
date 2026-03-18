using System.Net.Http.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class OrderDetailServiceClient : IOrderDetailServiceClient
    {
        private readonly HttpClient _http;
        // Đường dẫn này phải khớp với Upstream trong ocelot.json của bạn
        private readonly string _route = "orders/OrderDetails";

        public OrderDetailServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OrderDetailResponseDTO>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                // Gọi API: GET https://localhost:5000/orders/OrderDetails/order/{orderId}
                return await _http.GetFromJsonAsync<List<OrderDetailResponseDTO>>($"{_route}/order/{orderId}")
                       ?? new List<OrderDetailResponseDTO>();
            }
            catch
            {
                return new List<OrderDetailResponseDTO>();
            }
        }

        public async Task<bool> UpdateAsync(Guid id, OrderDetailRequestDTO dto)
        {
            try
            {
                // Gọi API: PUT https://localhost:5000/orders/OrderDetails/{id}
                var response = await _http.PutAsJsonAsync($"{_route}/{id}", dto);

                // Trả về true nếu status code là 2xx (ví dụ 200 OK hoặc 204 No Content)
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error ServiceClient] UpdateDetail thất bại: {ex.Message}");
                return false;
            }
        }
    }
}