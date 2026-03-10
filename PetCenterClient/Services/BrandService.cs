using Humanizer;
using Microsoft.Data.SqlClient;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Globalization;

namespace PetCenterClient.Services
{
    public class BrandService : IBrandService
    {
        private readonly HttpClient _http;

        public BrandService(HttpClient http)
        {
            _http = http;
        }
        public async Task AddBrandAsync(CreateBrandDTOs createBrand)
        {
            var response = await _http.PostAsJsonAsync("product-service/Brands", createBrand);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteBrandAsync(Guid? id)
        {
            await _http.DeleteAsync($"product-service/Brands/{id}");
        }

        public async Task<OdataResponse<ReadBrandDTOs>> GetAllBrandAsync(string? search, int page = 1)
        {
            int pageSize = 10;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(BrandName,'{search}')");
            }

            var query = new List<string>();

            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));
            query.Add("$count=true");

            int skip = (page - 1) * pageSize;

            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadBrandDTOs>>(
                "product-service/odata/Brands" + url
            );

            return response;
        }

        public async Task<ReadBrandDTOs> GetBrandByIdAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadBrandDTOs>($"product-service/Brands/{id}");
        }

        public async Task UpdateBrandAsync(Guid? id, UpdateBrandDTOs updateBrand)
        {
            var response = await _http.PutAsJsonAsync($"product-service/Brands/{id}", updateBrand);
            response.EnsureSuccessStatusCode();
        }
    }
}
