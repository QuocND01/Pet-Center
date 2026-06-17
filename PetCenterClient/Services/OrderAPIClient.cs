using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Common;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class OrderAPIClient : IOrderAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<OdataResponse<ReadOrderListViewModel>> GetOrderListAdminAsync(
            string? search,
            int? status,
            string? paymentMethod,
            string? sortBy,
            string sortOrder = "desc",
            int page = 1)
        {
            AddAuthorizationHeader();
            int pageSize = 10; // Admin thường để 10-20 record mỗi trang
            if (page < 1) page = 1;

            var filters = new List<string>();

            // Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(CustomerName,'{search}')");
            }

            // Lọc theo trạng thái
            if (status.HasValue)
                filters.Add($"Status eq {status.Value}");

            // Lọc theo phương thức thanh toán
            if (!string.IsNullOrEmpty(paymentMethod))
                filters.Add($"PaymentMethod eq '{paymentMethod}'");

            var query = new List<string>();
            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));

            // Sắp xếp
            if (!string.IsNullOrEmpty(sortBy))
            {
                query.Add($"$orderby={sortBy} {sortOrder}");
            }
            else
            {
                // Mặc định đơn mới nhất lên đầu
                query.Add($"$orderby=OrderDate desc");
            }

            query.Add("$count=true");
            int skip = (page - 1) * pageSize;
            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadOrderListViewModel>>(
                "odata/Orders" + url
            );

            return response ?? new OdataResponse<ReadOrderListViewModel>();
        }

        public async Task<ReadOrderDetailViewModel?> GetOrderDetailsAsync(Guid id)
        {
            var response = await _http.GetAsync($"api/Orders/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReadOrderDetailViewModel>();
            }
            return null;
        }

        public async Task<bool> CancelOrderAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _http.PatchAsync($"api/Orders/{id}/cancel", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AdvanceOrderStatusAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _http.PatchAsync($"api/Orders/{id}/advance", null);
            return response.IsSuccessStatusCode;
        }
    }
}