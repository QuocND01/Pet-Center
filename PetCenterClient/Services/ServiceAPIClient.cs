using PetCenterClient.Common;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Common;
using PetCenterClient.ViewModels.Service;
using System.Net.Http.Headers;
using System.Text.Json;
using static PetCenterClient.ViewModels.Service.ServiceViewModel;

namespace PetCenterClient.Services
{
    public class ServiceAPIClient : IServiceAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OdataResponse<ReadServiceViewModelForCustomer>> GetAllServiceAsync(
     string? search,
     decimal? minPrice,
     decimal? maxPrice,
     int? serviceType,
     int page = 1)
        {

            int pageSize = 24;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(ServiceName,'{search}')");
            }

            if (minPrice.HasValue)
                filters.Add($"ServicePrice ge {minPrice.Value}");

            if (maxPrice.HasValue)
                filters.Add($"ServicePrice le {maxPrice.Value}");

            var query = new List<string>();

            if (serviceType.HasValue)
            {
                query.Add($"serviceType={serviceType.Value}");
            }

            if (filters.Any())
            {
                query.Add("$filter=" + string.Join(" and ", filters));
            }

            query.Add("$count=true");

            int skip = (page - 1) * pageSize;

            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadServiceViewModelForCustomer>>(
                "odata/Services" + url
            );

            return response;
        }


        public async Task<PagedResponse<ReadServiceViewModel>> GetAllServiceAdminAsync(
     string? search,
     Status? status,
     decimal? minPrice,
     decimal? maxPrice,
     int? serviceType,
     int page = 1,
     int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            if (status.HasValue)
            {
                query.Add($"status={status.Value}");
            }

            if (minPrice.HasValue)
            {
                query.Add($"minPrice={minPrice.Value}");
            }

            if (maxPrice.HasValue)
            {
                query.Add($"maxPrice={maxPrice.Value}");
            }

            if (serviceType.HasValue)
            {
                query.Add($"serviceType={serviceType.Value}");
            }

            query.Add($"page={page}");
            query.Add($"pageSize={pageSize}");

            var url = "api/Services/admin";

            if (query.Any())
            {
                url += "?" + string.Join("&", query);
            }

            return await _http.GetFromJsonAsync<PagedResponse<ReadServiceViewModel>>(url);
        }


        public async Task<ReadServiceViewModel> DetailsServiceAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadServiceViewModel>($"api/Services/{id}");
        }

        public async Task AddServiceAsync(CreateServiceViewModel model)
        {
            AddAuthorizationHeader();
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.ServiceName), "ServiceName");
            content.Add(new StringContent(model.Price.ToString()), "Price");

            if (!string.IsNullOrEmpty(model.ServiceDescription))
                content.Add(new StringContent(model.ServiceDescription), "ServiceDescription");

            content.Add(new StringContent(model.ServiceType.ToString()), "ServiceType");
            content.Add(new StringContent(model.Duration.ToString()), "Duration");



            // gửi ảnh
            if (model.ImageFiles != null)
            {
                foreach (var file in model.ImageFiles)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    content.Add(streamContent, "ImageFiles", file.FileName);
                }
            }

            var response = await _http.PostAsync("api/Services", content);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
        }

        public async Task UpdateServiceAsync(Guid? id, UpdateServiceViewModel model)
        {
            AddAuthorizationHeader();
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(model.ServiceName), "ServiceName");
            form.Add(new StringContent(model.Price.ToString()), "Price");

            if (!string.IsNullOrEmpty(model.ServiceDescription))
                form.Add(new StringContent(model.ServiceDescription), "ServiceDescription");

            form.Add(new StringContent(model.ServiceType.ToString()), "ServiceType");
            form.Add(new StringContent(model.Duration.ToString()), "Duration");


            // Existing Images (ảnh còn giữ lại)
            if (model.ExistingImages != null)
            {
                foreach (var img in model.ExistingImages)
                {
                    form.Add(new StringContent(img), "ExistingImages");
                }
            }

            // Upload images
            if (model.ImageFiles != null)
            {
                foreach (var file in model.ImageFiles)
                {
                    var stream = file.OpenReadStream();
                    var content = new StreamContent(stream);
                    content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    form.Add(content, "ImageFiles", file.FileName);
                }
            }


            var response = await _http.PutAsync($"api/Services/{id}", form);

            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeServiceStatusAsync(
     Guid id,
     Status status)
        {
            AddAuthorizationHeader();

            var response = await _http.PatchAsJsonAsync(
                $"api/Services/{id}/status",
                status
            );

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
