using PetCenterClient.Services.Interface;
using System.Text;
using System.Text.Json;
using PetCenterClient.DTOs;
namespace PetCenterClient.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _httpClient;

        public SupplierService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ReadSupplierDto>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync("https://localhost:5000/inventory/Suppliers");
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ReadSupplierDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<ReadSupplierDto?> GetByIdAsync(Guid id)
        {
            var res = await _httpClient.GetAsync($"https://localhost:5000/inventory/Suppliers/{id}");

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ReadSupplierDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task CreateAsync(CreateSupplierDto dto)
        {
            var json = JsonSerializer.Serialize(dto);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("https://localhost:5000/inventory/Suppliers", content);
        }

        public async Task UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            var json = JsonSerializer.Serialize(dto);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"https://localhost:5000/inventory/Suppliers/{id}", content);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"https://localhost:5000/inventory/Suppliers/{id}");
        }
    }
}
