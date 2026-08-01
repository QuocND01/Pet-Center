using PetCenterClient.Common;
using PetCenterClient.ViewModels;
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

            // Nếu người dùng nhập GUID (OrderId) — cho phép nhập kèm '#' ở đầu (vd: #<guid>)
            // => loại bỏ '#' và khoảng trắng trước khi thử parse GUID
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                if (search.StartsWith("#"))
                {
                    search = search.TrimStart('#').Trim();
                }
            }

            if (!string.IsNullOrEmpty(search) && Guid.TryParse(search, out var parsedGuid))
            {
                // Lấy chi tiết đơn hàng và chuyển thành dạng list trả về cho bảng (tránh lỗi khi OData filter với GUID gây 500)
                var detail = await GetOrderDetailsAsync(parsedGuid);
                var resp = new OdataResponse<ReadOrderListViewModel>
                {
                    Context = string.Empty,
                    Count = detail == null ? 0 : 1,
                    Values = new List<ReadOrderListViewModel>()
                };

                if (detail != null)
                {
                    resp.Values.Add(new ReadOrderListViewModel
                    {
                        OrderId = detail.OrderId,
                        CustomerName = detail.CustomerName,
                        PhoneNumber = detail.PhoneNumber,
                        OrderDate = detail.OrderDate,
                        TotalAmount = detail.TotalAmount,
                        Status = detail.Status,
                        PaymentMethod = detail.PaymentMethod,
                        PaymentStatus = detail.PaymentStatus
                    });
                }

                return resp;
            }

            // Lọc theo tên khách hàng (phi GUID)
            if (!string.IsNullOrEmpty(search))
            {
                // Escape single quotes for OData string literals
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

            // URL-encode the right-hand side of each query param to avoid special characters (like &, ?, =) breaking the query
            var encodedParts = query.Select(q =>
            {
                var idx = q.IndexOf('=');
                if (idx >= 0 && idx < q.Length - 1)
                {
                    var key = q.Substring(0, idx + 1);
                    var value = q.Substring(idx + 1);
                    return key + System.Uri.EscapeDataString(value);
                }
                return System.Uri.EscapeDataString(q);
            });

            var url = "?" + string.Join("&", encodedParts);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadOrderListViewModel>>("odata/Orders" + url);

            var finalResp = response ?? new OdataResponse<ReadOrderListViewModel> { Context = string.Empty, Count = 0, Values = new List<ReadOrderListViewModel>() };

            // Nếu người dùng nhập một chuỗi (không phải GUID) và muốn tìm theo cụm trong OrderId,
            // gọi endpoint server mới để tìm các order có OrderId chứa chuỗi đó, rồi ghép kết quả.
            if (!string.IsNullOrEmpty(search) && search.Length >= 3)
            {
                try
                {
                    var searchById = await _http.GetFromJsonAsync<List<ReadOrderListViewModel>>("api/Orders/search-by-id?term=" + System.Uri.EscapeDataString(search));
                    if (searchById != null && searchById.Any())
                    {
                        // Merge: ưu tiên hiển thị các kết quả khớp OrderId lên đầu, tránh duplicate
                        var merged = new List<ReadOrderListViewModel>();
                        var added = new HashSet<Guid>();

                        foreach (var it in searchById)
                        {
                            if (added.Add(it.OrderId)) merged.Add(it);
                        }

                        foreach (var it in finalResp.Values ?? Enumerable.Empty<ReadOrderListViewModel>())
                        {
                            if (added.Add(it.OrderId)) merged.Add(it);
                        }

                        finalResp.Values = merged.Take(pageSize).ToList();
                        finalResp.Count = merged.Count;
                    }
                }
                catch
                {
                    // Nếu gọi search-by-id lỗi thì bỏ qua, trả về kết quả OData như cũ
                }
            }

            return finalResp;
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

        public async Task<List<ReadOrderListViewModel>?> GetMyOrderHistoryAsync()
        {
            // Đảm bảo hàm này có chèn kèm Token vào Header (như AddAuthorizationHeader() team bạn hay dùng)
            AddAuthorizationHeader();
            var response = await _http.GetAsync("api/Orders/my-orders");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ReadOrderListViewModel>>();
            }
            return new List<ReadOrderListViewModel>(); // Trả về list rỗng nếu lỗi/chưa có đơn
        }
    }
}