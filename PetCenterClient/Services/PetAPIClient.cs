using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

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

        public async Task<List<ReadPetListViewModel>?> GetMyPetsAsync()
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync("api/Pets/my-pets");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<List<ReadPetListViewModel>>() : null;
        }

        public async Task<ReadPetDetailViewModel?> GetPetDetailsAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadPetDetailViewModel>() : null;
        }

        public async Task<List<ReadVetPetListViewModel>?> GetAllPetsForVetAsync()
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync("api/vet/pets");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<List<ReadVetPetListViewModel>>() : null;
        }

        public async Task<ReadVetPetDetailViewModel?> GetPetDetailsForVetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/vet/pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadVetPetDetailViewModel>() : null;
        }
    }
}