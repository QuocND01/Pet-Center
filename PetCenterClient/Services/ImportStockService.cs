
using DocumentFormat.OpenXml.Office2010.Excel;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PetCenterClient.Services
{
    public class ImportStockService : IImportStockService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public ImportStockService(HttpClient httpClient, IConfiguration config, IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Api:Url"] ?? "");
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        // Hàm helper để gán Token vào header cho mọi request
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

        public async Task<List<ReadImportHeaderDto>> GetAllAsync()
        {
            AddAuthorizationHeader(); // Phải gọi trước khi Send/Get
            var res = await _httpClient.GetAsync("inventory/importstock");

            if (!res.IsSuccessStatusCode) return new List<ReadImportHeaderDto>();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ReadImportHeaderDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<ReadImportStockDetailDto?> GetByIdAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.GetAsync($"inventory/importstock/{id}");

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReadImportStockDetailDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Guid> CreateAsync(CreateImportStockDto dto)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PostAsJsonAsync("inventory/importstock", dto);

            res.EnsureSuccessStatusCode();
            var result = await res.Content.ReadAsStringAsync();

            // Xử lý nếu Guid trả về dạng chuỗi có ngoặc kép
            return JsonSerializer.Deserialize<Guid>(result);
        }

        public async Task ConfirmAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PutAsync($"inventory/importstock/{id}/confirm", null);

            res.EnsureSuccessStatusCode();
            var items = await res.Content.ReadFromJsonAsync<List<IncreaseStockItemDto>>();

            await _productService.IncreaseStockBulk(items);
        }

        public async Task CancelAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PutAsync($"inventory/importstock/{id}/cancel", null);
            res.EnsureSuccessStatusCode();
        }
        public async Task<List<ImportDto>> GetAllByTimeAsync()
        {
            AddAuthorizationHeader();

            var res = await _httpClient.GetAsync("inventory/importstock/export");

            if (!res.IsSuccessStatusCode)
                return new List<ImportDto>();

            var json = await res.Content.ReadAsStringAsync();
            Console.WriteLine(json);

            var data = JsonSerializer.Deserialize<ImportStockResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (data == null) return new List<ImportDto>();

            // 🔥 GÁN DETAILS VÀO IMPORT
            foreach (var import in data.Imports)
            {
                import.Details = data.Details
                    .Where(d => d.ImportId == import.ImportId)
                    .ToList();
            }

            return data.Imports;
        }
        // 🔥 GỌI API TRỪ KHO
        public async Task<string?> DeductFIFO(Guid productId, int quantity)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PostAsJsonAsync($"inventory/importstock/deduct", new { productId, quantity });
            if (!res.IsSuccessStatusCode) return null;

            // Đọc JSON và chỉ lấy giá trị của trường "mapping"
            var result = await res.Content.ReadFromJsonAsync<DeductStockResponse>();
            return result?.Mapping; // Trả về "id:qty,id:qty" (chuỗi sạch)
        }

        // 🔥 GỌI API HOÀN KHO
        public async Task<bool> ReturnStock(string mapping)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PostAsJsonAsync($"inventory/importstock/return", new
            {
                mapping
            });

            return res.IsSuccessStatusCode;
        }
    }
}

