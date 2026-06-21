using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Json;

namespace PetCenterClient.Services
{
    public class AddressAPIClient : IAddressAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddressAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // Nhớ hàm này để nhét Token vào Header trước khi gọi API bảo mật
        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ReadAddressViewModel>?> GetMyAddressesAsync()
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync("api/Addresses/my-addresses");
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<List<ReadAddressViewModel>>();
            }
            return new List<ReadAddressViewModel>();
        }

        public async Task<bool> AddAddressAsync(MutateAddressViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PostAsJsonAsync("api/Addresses", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAddressAsync(Guid id, MutateAddressViewModel dto)
        {
            AddAuthorizationHeader();
            var res = await _http.PutAsJsonAsync($"api/Addresses/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAddressAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/Addresses/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}