using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;

        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<ReadCategoryDTOs>>("api/Categories");
        }
    }
}
