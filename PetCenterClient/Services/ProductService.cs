using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using ProductAPI.DTOs;

namespace PetCenterClient.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public async Task<OdataResponse<ReadProductDTO>> GetAllProductAsync(
string? search,
bool? isActive,
decimal? minPrice,
decimal? maxPrice,
DateTime? fromDate,
DateTime? toDate,
string? sortBy,
string sortOrder = "asc",
int page = 1)
        {
            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
                filters.Add($"contains(Name,'{search}')");

            if (isActive.HasValue)
                filters.Add($"IsActive eq {isActive.Value.ToString().ToLower()}");

            if (minPrice.HasValue)
                filters.Add($"Price ge {minPrice.Value}");

            if (maxPrice.HasValue)
                filters.Add($"Price le {maxPrice.Value}");

            var query = new List<string>();

            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));

            if (!string.IsNullOrEmpty(sortBy))
                query.Add($"$orderby={sortBy} {sortOrder}");

            query.Add("$count=true");     // BẮT BUỘC nếu muốn Count
            query.Add("$top=10");
            query.Add("$skip={(page - 1) * 10}");

            var url = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;


            return await _http.GetFromJsonAsync<OdataResponse<ReadProductDTO>>(
                "odata/products" //+ url
            );
        }


        public async Task<ReadProductDTO> GetProductByIdAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadProductDTO>($"api/Products/{id}");
        }

        public async Task AddProductAsync(CreateProductDTO createproduct)
        {
            await _http.PostAsJsonAsync("api/Products", createproduct);
        }

        public async Task UpdateProductAsync(Guid? id, UpdateProductDTO updateproduct)
        {
            await _http.PutAsJsonAsync($"api/Products/{id}", updateproduct);
        }

        public async Task DeleteProductAsync(Guid? id)
        {
            await _http.DeleteAsync($"api/Products/{id}");
        }
    }

}
