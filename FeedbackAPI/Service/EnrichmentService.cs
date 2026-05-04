using FeedbackAPI.Service.Interface;
using System.Text.Json;

namespace FeedbackAPI.Service
{
    public class EnrichmentService : IEnrichmentService
    {
        private readonly HttpClient _customerClient;
        private readonly HttpClient _productClient;
        private readonly HttpClient _staffClient;
        private readonly ILogger<EnrichmentService> _logger;

        public EnrichmentService(IHttpClientFactory factory, ILogger<EnrichmentService> logger)
        {
            _customerClient = factory.CreateClient("CustomerAPI");
            _productClient = factory.CreateClient("ProductAPI");
            _staffClient = factory.CreateClient("StaffAPI");
            _logger = logger;
        }

        public async Task<(string? FullName, string? Email)> GetCustomerInfoAsync(Guid customerId)
        {
            try
            {
                var res = await _customerClient.GetAsync($"api/customers/internal/{customerId}");
                if (!res.IsSuccessStatusCode) return (null, null);
                var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                return (
                    root.TryGetProperty("fullName", out var n) ? n.GetString() : null,
                    root.TryGetProperty("email", out var e) ? e.GetString() : null
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetCustomerInfo failed {id}: {msg}", customerId, ex.Message);
                return (null, null);
            }
        }

        public async Task<(string? ProductName, string? ImageUrl)> GetProductInfoAsync(Guid productId)
        {
            try
            {
                var res = await _productClient.GetAsync($"api/Products/internal/{productId}");
                if (!res.IsSuccessStatusCode) return (null, null);
                var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                var name = root.TryGetProperty("productName", out var n) ? n.GetString() : null;
                var img = root.TryGetProperty("imageUrl", out var u) ? u.GetString() : null;
                return (name, img);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetProductInfo failed {id}: {msg}", productId, ex.Message);
                return (null, null);
            }
        }

        public async Task<string?> GetStaffNameAsync(Guid staffId)
        {
            try
            {
                var res = await _staffClient.GetAsync($"api/Staffs/internal/{staffId}");
                if (!res.IsSuccessStatusCode) return null;
                var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                return root.TryGetProperty("fullName", out var n) ? n.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetStaffName failed {id}: {msg}", staffId, ex.Message);
                return null;
            }
        }
    }
}
