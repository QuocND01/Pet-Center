using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Services
{
    public class PetAPIClient : IPetAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PetAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ReadPetListViewModel>?> GetMyPetsAsync(string query = "")
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Pets/my-pets{query}");
            if (res.IsSuccessStatusCode)
            {
                // Đọc thẳng ra List vì Backend (Standard Route) trả về mảng trực tiếp
                return await res.Content.ReadFromJsonAsync<List<ReadPetListViewModel>>();
            }
            return null;
        }

        public async Task<ReadPetDetailViewModel?> GetPetDetailsAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadPetDetailViewModel>() : null;
        }

        // ================= CRUD CỦA CUSTOMER =================
        public async Task<bool> AddPetAsync(MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PostAsJsonAsync("api/Pets", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePetAsync(Guid id, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PutAsJsonAsync($"api/Pets/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/Pets/{id}");
            return res.IsSuccessStatusCode;
        }

        // ================= GET CỦA VET =================
        public async Task<List<ReadVetPetListViewModel>?> GetAllPetsForVetAsync(string query = "")
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/vet/pets{query}");
            if (res.IsSuccessStatusCode)
            {
                // Đọc thẳng ra List vì Backend (Standard Route) trả về mảng trực tiếp
                return await res.Content.ReadFromJsonAsync<List<ReadVetPetListViewModel>>();
            }
            return null;
        }

        public async Task<ReadVetPetDetailViewModel?> GetPetDetailsForVetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/vet/pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadVetPetDetailViewModel>() : null;
        }

        public async Task<bool> AddPetForVetAsync(Guid customerId, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PostAsJsonAsync($"api/vet/pets/add-for-customer/{customerId}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePetForVetAsync(Guid id, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PutAsJsonAsync($"api/vet/pets/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePetForVetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/vet/pets/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}