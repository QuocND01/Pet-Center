using System.Net.Http.Headers;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Services
{
    public class DiseaseAPIClient : IDiseaseAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DiseaseAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ReadDiseaseViewModel>?> GetAllDiseasesAsync(string query = "")
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Diseases{query}");
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<List<ReadDiseaseViewModel>>();
            }
            return null;
        }

        public async Task<ReadDiseaseViewModel?> GetDiseaseDetailsAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Diseases/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadDiseaseViewModel>() : null;
        }

        public async Task<(bool success, string message)> AddDiseaseAsync(MutateDiseaseViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PostAsJsonAsync("api/Diseases", dto);
            if (res.IsSuccessStatusCode) return (true, "");

            // Đọc thẳng lỗi từ API trả về
            return (false, await res.Content.ReadAsStringAsync());
        }

        public async Task<(bool success, string message)> UpdateDiseaseAsync(Guid id, MutateDiseaseViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PutAsJsonAsync($"api/Diseases/{id}", dto);
            if (res.IsSuccessStatusCode) return (true, "");
            return (false, await res.Content.ReadAsStringAsync());
        }

        public async Task<(bool success, string message)> DeleteDiseaseAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/Diseases/{id}");
            if (res.IsSuccessStatusCode) return (true, "");
            return (false, await res.Content.ReadAsStringAsync());
        }
    }
}