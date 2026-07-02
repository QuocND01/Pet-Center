using Microsoft.AspNetCore.WebUtilities;
using PetCenterClient.ViewModels.Inventory;
using System.Configuration;

namespace PetCenterClient.Services
{
    public class InventoryApiService
    {
        private readonly HttpClient _http;
        

        public InventoryApiService(
            HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _http.BaseAddress = new Uri(configuration["Api:Url"]!);
        }

        public async Task<InventoryListResponseViewModel?> GetPagedAsync(
            string? keyword,
            Guid? categoryId,
            Guid? brandId,
            bool? lowStock,
            bool? outOfStock,
            int page = 1,
            int pageSize = 10)
        {
            var query = new Dictionary<string, string?>()
            {
                ["keyword"] = keyword,
                ["categoryId"] = categoryId?.ToString(),
                ["brandId"] = brandId?.ToString(),
                ["lowStock"] = lowStock?.ToString(),
                ["outOfStock"] = outOfStock?.ToString(),
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            var url = QueryHelpers.AddQueryString(
                "api/inventories",
                query!);

            return await _http.GetFromJsonAsync<InventoryListResponseViewModel>(url);
        }

        public async Task<InventoryDetailViewModel?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<InventoryDetailViewModel>(
                $"api/inventories/{id}");
        }
    }
}