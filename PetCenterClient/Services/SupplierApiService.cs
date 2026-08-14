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

        public async Task<ApiResponseViewModel<ViewSupplierViewModel>?> CreateAsync(
    CreateSupplierViewModel model)
        {
            AddAuthorizationHeader();

            // 1. Gửi request đến API
            var response = await _httpClient.PostAsJsonAsync("/api/suppliers", model);

            // 2. Đọc kết quả JSON trả về (cho cả trường hợp Success lẫn Failure)
            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<ViewSupplierViewModel>>();

            // 3. Nếu API trả về thành công (HTTP status 2xx)
            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            // 4. Nếu thất bại (400, 409, 500...), trả về object chứa Message lỗi từ API
            return result ?? new ApiResponseViewModel<ViewSupplierViewModel>
            {
                Status = false,
                Message = "An error occurred while processing the request."
            };
        }

        public async Task<ApiResponseViewModel<bool>?> UpdateAsync(
    Guid id,
    CreateSupplierViewModel model)
        {
            AddAuthorizationHeader();

            // 1. Gửi request PUT sang API
            var response = await _httpClient.PutAsJsonAsync($"/api/suppliers/{id}", model);

            // 2. Đọc kết quả JSON trả về từ API
            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<bool>>();

            // 3. Nếu thành công (HTTP status 2xx)
            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            // 4. Nếu thất bại (400, 404, 500...), trả về object chứa Message lỗi từ API
            return result ?? new ApiResponseViewModel<bool>
            {
                Status = false,
                Message = "An error occurred while updating the supplier."
            };
        }

        public async Task<ApiResponseViewModel<bool>?> DeleteAsync(Guid id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(
                $"/api/suppliers/{id}");

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<bool>>();

            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            return result ?? new ApiResponseViewModel<bool>
            {
                Status = false,
                Message = "An error occurred while deleting the supplier."
            };
        }
        
    }
}