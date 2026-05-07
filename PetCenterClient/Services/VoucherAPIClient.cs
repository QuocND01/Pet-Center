using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PetCenterClient.Services
{
    public class VoucherAPIClient : IVoucherService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Ocelot gateway: /voucher-service/{everything} → downstream /api/{everything}
        private const string PREFIX = "voucher-service/voucher";

        public VoucherAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── HELPER: gắn JWT vào header ───────────────────────────
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<List<VoucherDto>> GetAllAsync()
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync($"{PREFIX}/vouchers");

                if (!response.IsSuccessStatusCode)
                    return new List<VoucherDto>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VoucherDto>>(content)
                       ?? new List<VoucherDto>();
            }
            catch
            {
                return new List<VoucherDto>();
            }
        }

        // ── GET BY ID ────────────────────────────────────────────
        public async Task<VoucherDto?> GetByIdAsync(Guid id)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync($"{PREFIX}/vouchers/{id}");

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VoucherDto>(content);
            }
            catch
            {
                return null;
            }
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<(bool Success, string Message, VoucherDto? Data)> CreateAsync(CreateVoucherDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync($"{PREFIX}/vouchers", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var err = JsonConvert.DeserializeObject<dynamic>(body);
                    string msg = err?.message ?? "Failed to create voucher.";
                    return (false, msg, null);
                }

                var result = JsonConvert.DeserializeObject<dynamic>(body);
                string message = result?.message ?? "Voucher created successfully.";
                var dataJson = JsonConvert.SerializeObject(result?.data);
                var data = JsonConvert.DeserializeObject<VoucherDto>(dataJson);

                return (true, message, data);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<(bool Success, string Message, VoucherDto? Data)> UpdateAsync(Guid id, UpdateVoucherDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PutAsync($"{PREFIX}/vouchers/{id}", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var err = JsonConvert.DeserializeObject<dynamic>(body);
                    string msg = err?.message ?? "Failed to update voucher.";
                    return (false, msg, null);
                }

                var result = JsonConvert.DeserializeObject<dynamic>(body);
                string message = result?.message ?? "Voucher updated successfully.";
                var dataJson = JsonConvert.SerializeObject(result?.data);
                var data = JsonConvert.DeserializeObject<VoucherDto>(dataJson);

                return (true, message, data);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        // ── TOGGLE STATUS ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive)
        {
            try
            {
                AttachToken();

                // Gửi PATCH không có body, isActive nằm trên query string
                var response = await _http.PatchAsync(
                    $"{PREFIX}/vouchers/{id}/toggle?isActive={isActive.ToString().ToLower()}",
                    null  // không cần body
                );

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(body);
                string message = result?.message
                                 ?? (response.IsSuccessStatusCode ? "Status updated." : "Failed to toggle status.");

                return (response.IsSuccessStatusCode, message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}