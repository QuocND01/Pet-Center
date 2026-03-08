using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Text;
using System.Text.Json;

namespace PetCenterClient.Services
{
    public class ImportStockService : IImportStockService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ImportStockService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Api:Url"]);
        }

        public async Task<List<ReadImportHeaderDto>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync("/inventory/importstock");

            if (!res.IsSuccessStatusCode)
                return new List<ReadImportHeaderDto>();

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ReadImportHeaderDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<ReadImportStockDto?> GetByIdAsync(Guid id)
        {
            var res = await _httpClient.GetAsync($"/inventory/importstock/{id}");

            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ReadImportStockDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Guid> CreateAsync(CreateImportStockDto dto)
        {
            var json = JsonSerializer.Serialize(dto);

            var res = await _httpClient.PostAsync(
                "/importstock",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Guid>(result);
        }

        public async Task ConfirmAsync(Guid id)
        {
            await _httpClient.PutAsync($"/inventory/importstock/{id}/confirm", null);
        }

        public async Task CancelAsync(Guid id)
        {
            await _httpClient.PutAsync($"/inventory/importstock/{id}/cancel", null);
        }
    }
}
