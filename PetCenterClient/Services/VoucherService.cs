using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Text;

namespace PetCenterClient.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly HttpClient _httpClient;
        private const string PREFIX = "voucher-service/vouchers/";

        public VoucherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VoucherDTO>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync(PREFIX);
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<VoucherDTO>>(json)
                   ?? new List<VoucherDTO>();
        }

        public async Task<VoucherDTO?> GetByIdAsync(Guid id)
        {
            var res = await _httpClient.GetAsync(PREFIX + id);
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<VoucherDTO>(json);
        }

        public async Task CreateAsync(CreateVoucherDTO dto)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync(PREFIX, content);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _httpClient.DeleteAsync(PREFIX + id);
        }

        public async Task<object> ApplyVoucherAsync(ApplyVoucherDTO dto)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            var res = await _httpClient.PostAsync(PREFIX + "apply", content);
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<object>(json);
        }

        public async Task UpdateAsync(Guid id, CreateVoucherDTO dto)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PutAsync(PREFIX + id, content);
        }

        public async Task<List<VoucherDTO>> SearchAsync(string code)
        {
            var res = await _httpClient.GetAsync(PREFIX + "search?code=" + code);
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<VoucherDTO>>(json)
                   ?? new List<VoucherDTO>();
        }
    }
}