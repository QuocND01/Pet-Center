using System.Net.Http.Headers;
using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    /// <summary>
    /// FIX: HttpClient đã có BaseAddress set từ Program.cs (AddHttpClient).
    /// Khi BaseAddress đã set, KHÔNG được gọi _http.GetAsync("https://...")
    /// với absolute URL — sẽ throw InvalidOperationException.
    ///
    /// Giải pháp: dùng relative path trực tiếp, để HttpClient tự combine với BaseAddress.
    /// BaseAddress = "https://localhost:5000/"  (có trailing slash)
    /// Relative    = "orders/Checkout/vouchers/..."
    /// Result      = "https://localhost:5000/orders/Checkout/vouchers/..."
    /// </summary>
    public class CheckoutService : ICheckoutService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckoutService(
            HttpClient http,
            IHttpContextAccessor httpContextAccessor)
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

        // POST orders/Checkout
        // BaseAddress + "orders/Checkout" = "https://localhost:5000/orders/Checkout"
        public async Task<CheckoutResponseDTO?> ProcessCheckoutAsync(CheckoutRequestDTO dto)
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("orders/Checkout", dto);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string msg = "Checkout failed.";
                try
                {
                    var err = JsonSerializer.Deserialize<JsonElement>(json);
                    if (err.TryGetProperty("message", out var m)) msg = m.GetString() ?? msg;
                }
                catch { }
                return new CheckoutResponseDTO { Success = false, Message = msg };
            }

            return JsonSerializer.Deserialize<CheckoutResponseDTO>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // GET orders/Checkout/vouchers/{customerId}?orderAmount={amount}
        public async Task<List<CustomerVoucherDTO>> GetAvailableVouchersAsync(
            Guid customerId, decimal orderAmount)
        {
            AddAuthHeader();

            var amount = orderAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            // Dùng relative path — HttpClient tự combine với BaseAddress
            var relPath = $"orders/Checkout/vouchers/{customerId}?orderAmount={amount}";

            var response = await _http.GetAsync(relPath);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<CustomerVoucherDTO>();

            return JsonSerializer.Deserialize<List<CustomerVoucherDTO>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CustomerVoucherDTO>();
        }

        // POST orders/Checkout/validate-voucher
        public async Task<VoucherValidateResponseDTO?> ValidateVoucherAsync(
            VoucherValidateRequestDTO dto)
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("orders/Checkout/validate-voucher", dto);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<VoucherValidateResponseDTO>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}