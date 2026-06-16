using System.Net.Http.Headers;
using PetCenterClient.ViewModels;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Supplier;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PetCenterClient.Services
{
    public class SupplierApiService : ISupplierApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupplierApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("JWT");

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ViewSupplierViewModel>> GetAllAsync()
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync("/api/suppliers");

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<List<ViewSupplierViewModel>>>();

            return result?.Data ?? [];
        }

        public async Task<ViewSupplierViewModel?> GetByIdAsync(Guid id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync($"/api/suppliers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<ViewSupplierViewModel>>();

            return result?.Data;
        }

        public async Task<ViewSupplierViewModel?> CreateAsync(
            CreateSupplierViewModel model)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync(
                "/api/suppliers",
                model);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<ViewSupplierViewModel>>();

            return result?.Data;
        }

        public async Task<bool> UpdateAsync(
            Guid id,
            CreateSupplierViewModel model)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/suppliers/{id}",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(
                $"/api/suppliers/{id}");

            return response.IsSuccessStatusCode;
        }
        public Task? GetSupplierSelectAsync()
        {
            return null;
        }
    }
}