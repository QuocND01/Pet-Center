using System.Net.Http.Json;
using OrdersAPI.DTOs;
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
    }
}