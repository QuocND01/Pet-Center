using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class BrandService : IBrandService
    {
        private readonly HttpClient _http;

        public BrandService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<ReadBrandDTOs>>("api/Brands");
        }
    }
}
