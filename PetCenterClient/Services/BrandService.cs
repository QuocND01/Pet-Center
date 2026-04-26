using Humanizer;
using Microsoft.Data.SqlClient;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Globalization;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class BrandService : IBrandService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrandService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddBrandAsync(CreateBrandDTOs createBrand)
        {
            AddAuthorizationHeader();

            var content = new MultipartFormDataContent();

            // 👇 text
            content.Add(new StringContent(createBrand.BrandName), "BrandName");

            if (!string.IsNullOrEmpty(createBrand.BrandDescription))
            {
                content.Add(new StringContent(createBrand.BrandDescription), "BrandDescription");
            }

            // 👇 file (phải đúng tên BrandLogo)
            if (createBrand.BrandLogo != null)
            {
                var streamContent = new StreamContent(createBrand.BrandLogo.OpenReadStream());
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(createBrand.BrandLogo.ContentType);

                content.Add(streamContent, "BrandLogo", createBrand.BrandLogo.FileName);
            }

            var response = await _http.PostAsync("product-service/Brands", content);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
        }

        public async Task DeleteBrandAsync(Guid? id)
        {
            AddAuthorizationHeader();
            await _http.DeleteAsync($"product-service/Brands/{id}");
        }

        public async Task<OdataResponse<ReadBrandDTOs>> GetAllBrandAsync(string? search, int page = 1)
        {
            int pageSize = 10;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(BrandName,'{search}')");
            }

            var query = new List<string>();

            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));
            query.Add("$count=true");

            int skip = (page - 1) * pageSize;

            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadBrandDTOs>>(
                "product-service/odata/Brands" + url
            );

            return response;
        }

        public async Task<ReadBrandDTOs> GetBrandByIdAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadBrandDTOs>($"product-service/Brands/{id}");
        }


        public async Task UpdateBrandAsync(Guid? id, UpdateBrandDTOs updateBrand)
        {
            AddAuthorizationHeader();

            var form = new MultipartFormDataContent();

            form.Add(new StringContent(updateBrand.BrandName), "BrandName");

            if (!string.IsNullOrEmpty(updateBrand.BrandDescription))
            {
                form.Add(new StringContent(updateBrand.BrandDescription), "BrandDescription");
            }

            if (updateBrand.BrandLogo != null)
            {
                var stream = updateBrand.BrandLogo.OpenReadStream();
                var content = new StreamContent(stream);

                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(updateBrand.BrandLogo.ContentType);

                form.Add(content, "BrandLogo", updateBrand.BrandLogo.FileName);
            }

            var response = await _http.PutAsync($"product-service/Brands/{id}", form);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
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
