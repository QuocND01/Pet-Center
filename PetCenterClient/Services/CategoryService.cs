using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddCategoryAsync(CreateCategoryDTOs createCategory)
        {
            AddAuthorizationHeader();
            var response = await _http.PostAsJsonAsync("product-service/Categories", createCategory);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCategoryAsync(Guid? id)
        {
            AddAuthorizationHeader();
            await _http.DeleteAsync($"product-service/Categories/{id}");
        }

        //public async Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync()
        //{
        //    return await _http.GetFromJsonAsync<IEnumerable<ReadCategoryDTOs>>("product-service/Categories");
        //}

        public async Task<OdataResponse<ReadCategoryDTOs>> GetAllCategoryAsync(string? search, int page = 1)
        {
            int pageSize = 10;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(CategoryName,'{search}')");
            }

            var query = new List<string>();

            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));
            query.Add("$count=true");

            int skip = (page - 1) * pageSize;

            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadCategoryDTOs>>(
                "product-service/odata/Categories" + url
            );

            return response;
        }

        public async Task<ReadCategoryDTOs> GetCategoryByIdAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadCategoryDTOs>($"product-service/Categories/{id}");
        }

        public async Task UpdateCategoryAsync(Guid? id, UpdateCategoryDTOs updateCategory)
        {
            AddAuthorizationHeader();
            var response = await _http.PutAsJsonAsync($"product-service/Categories/{id}", updateCategory);
            response.EnsureSuccessStatusCode();
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");

            if (!string.IsNullOrEmpty(token))
            {
                // Xóa các giá trị cũ để tránh cộng dồn header
                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
