using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.ManageVoucher;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PetCenterClient.Services
{
    public class VoucherApiService : IVoucherApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VoucherApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ============================================================
        // HELPER
        // ============================================================
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        public async Task<List<VoucherViewModel>> GetAllAsync()
        {
            try
            {
                AttachToken();

                var response = await _http.GetAsync("api/voucher/vouchers");
                if (!response.IsSuccessStatusCode)
                    return new List<VoucherViewModel>();

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<VoucherViewModel>>(content)
                       ?? new List<VoucherViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VoucherApiService] GetAllAsync error: {ex.Message}");
                return new List<VoucherViewModel>();
            }
        }

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        public async Task<VoucherViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync($"api/voucher/vouchers/{id}");

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VoucherViewModel>(content);
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        public async Task<(bool Success, string Message, VoucherViewModel? Data)> CreateAsync(CreateVoucherViewModel dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync($"api/voucher/vouchers", content);
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
                var data = JsonConvert.DeserializeObject<VoucherViewModel>(dataJson);

                return (true, message, data);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }       

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================

        public async Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive)
        {
            try
            {
                AttachToken();

                var response = await _http.PatchAsync(
                    $"api/voucher/vouchers/{id}/toggle?isActive={isActive.ToString().ToLower()}",
                    null
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