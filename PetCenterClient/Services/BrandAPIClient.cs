using Humanizer;
using Microsoft.Data.SqlClient;
using PetCenterClient.Common;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;
using PetCenterClient.ViewModels.Brand;
using PetCenterClient.ViewModels.Common;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class BrandAPIClient : IBrandAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public BrandAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = configuration["Api:url"];
        }

        public async Task AddBrandAsync(CreateBrandViewModel createBrand)
        {
            AddAuthorizationHeader();

            // Validate BrandName
            if (string.IsNullOrWhiteSpace(createBrand.BrandName))
            {
                throw new InvalidOperationException("Brand name is required.");
            }

            createBrand.BrandName = createBrand.BrandName.Trim();

            // Validate BrandLogo
            if (createBrand.BrandLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(createBrand.BrandLogo.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!createBrand.BrandLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (createBrand.BrandLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException(
                        "Image size cannot exceed 5 MB.");
                }
            }

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(createBrand.BrandName), "BrandName");

            if (!string.IsNullOrWhiteSpace(createBrand.BrandDescription))
            {
                content.Add(new StringContent(createBrand.BrandDescription.Trim()), "BrandDescription");
            }

            if (createBrand.BrandLogo != null)
            {
                var streamContent = new StreamContent(createBrand.BrandLogo.OpenReadStream());
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(createBrand.BrandLogo.ContentType);

                content.Add(streamContent, "BrandLogo", createBrand.BrandLogo.FileName);
            }

            var response = await _http.PostAsync("api/Brands", content);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiResponseViewModel<object>>();

                throw new InvalidOperationException(
                    error?.Message ?? "Create brand failed.");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeBrandStatusAsync(Guid id, Status status)
        {
            AddAuthorizationHeader();

            var response = await _http.PatchAsJsonAsync(
                $"api/Brands/{id}/status",
                status);

            response.EnsureSuccessStatusCode();
        }

        public async Task<OdataResponse<ReadBrandViewModelForCustomer>> GetAllBrandAsync()
        {
            return await _http.GetFromJsonAsync<
                OdataResponse<ReadBrandViewModelForCustomer>>(
                    "odata/Brands?$count=true");
        }


        public async Task<PagedResponse<ReadBrandViewModel>> GetAllBrandAdminAsync(
      string? search, Status? status, int page = 1, int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                query.Add($"search={search}");
            }

            if (status.HasValue)
                query.Add($"status={status}");

            query.Add($"page={page}");
            query.Add($"pageSize={pageSize}");

            var url = "api/Brands/admin?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<PagedResponse<ReadBrandViewModel>>(url);

            return response;
        }



        public async Task<ReadBrandViewModel> DetailsBrandAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadBrandViewModel>($"api/Brands/{id}");
        }


        public async Task UpdateBrandAsync(Guid? id, UpdateBrandViewModel updateBrand)
        {
            AddAuthorizationHeader();

            // Validate BrandName
            if (string.IsNullOrWhiteSpace(updateBrand.BrandName))
            {
                throw new InvalidOperationException("Brand name is required.");
            }

            updateBrand.BrandName = updateBrand.BrandName.Trim();

            // Validate BrandLogo
            if (updateBrand.BrandLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(updateBrand.BrandLogo.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!updateBrand.BrandLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (updateBrand.BrandLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException(
                        "Image size cannot exceed 5 MB.");
                }
            }

            var form = new MultipartFormDataContent();

            form.Add(new StringContent(updateBrand.BrandName), "BrandName");

            if (!string.IsNullOrWhiteSpace(updateBrand.BrandDescription))
            {
                form.Add(new StringContent(updateBrand.BrandDescription.Trim()), "BrandDescription");
            }

            if (updateBrand.BrandLogo != null)
            {
                var stream = updateBrand.BrandLogo.OpenReadStream();
                var content = new StreamContent(stream);

                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(updateBrand.BrandLogo.ContentType);

                form.Add(content, "BrandLogo", updateBrand.BrandLogo.FileName);
            }

            var response = await _http.PutAsync($"api/Brands/{id}", form);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiResponseViewModel<object>>();

                throw new InvalidOperationException(
                    error?.Message ?? "Update brand failed.");
            }

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
