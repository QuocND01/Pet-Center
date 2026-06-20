using PetCenterClient.Common;
using PetCenterClient.DTOs;

using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Common;
using PetCenterClient.ViewModels.Product;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PetCenterClient.Services
{
    public class ProductAPIClient : IProductAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OdataResponse<ReadProductViewModelForCustomer>> GetAllProductAsync(
     string? search,
     decimal? minPrice,
     decimal? maxPrice,
     DateTime? fromDate,
     DateTime? toDate,
     string? sortBy,
     Guid? categoryid,
     Guid? brandid,
     string sortOrder = "asc",
     int page = 1)
        {

            int pageSize = 24;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(ProductName,'{search}')");
            }


            if (minPrice.HasValue)
                filters.Add($"ProductPrice ge {minPrice.Value}");

            if (maxPrice.HasValue)
                filters.Add($"ProductPrice le {maxPrice.Value}");

            if (categoryid.HasValue)
                filters.Add($"CategoryId eq {categoryid.Value}");

            if (brandid.HasValue)
                filters.Add($"BrandId eq {brandid.Value}");

            var query = new List<string>();

            if (filters.Any())
                query.Add("$filter=" + string.Join(" and ", filters));

            if (!string.IsNullOrEmpty(sortBy))
            {
                var column = sortBy.ToLower() switch
                {
                    "price" => "ProductPrice",
                    "name" => "ProductName",
                    "date" => "AddedAt",
                    _ => "ProductName"
                };

                query.Add($"$orderby={column} {sortOrder}");
            }

            query.Add("$count=true");

            int skip = (page - 1) * pageSize;

            query.Add($"$skip={skip}");
            query.Add($"$top={pageSize}");

            var url = "?" + string.Join("&", query);

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadProductViewModelForCustomer>>(
                "odata/Products" + url
            );

            return response;
        }


        public async Task<PagedResponse<ReadProductViewModel>> GetAllProductAdminAsync(
       string? search,
      Status? status,
       decimal? minPrice,
       decimal? maxPrice,
       Guid? categoryId,
       Guid? brandId,
       string? sortBy,
       DateTime? fromDate,
       DateTime? toDate,
       string sortOrder = "asc",
       int page = 1,
       int pageSize = 10)
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

            if (minPrice.HasValue)
                query.Add($"minPrice={minPrice}");

            if (maxPrice.HasValue)
                query.Add($"maxPrice={maxPrice}");

            if (categoryId.HasValue)
                query.Add($"categoryId={categoryId}");

            if (brandId.HasValue)
                query.Add($"brandId={brandId}");

            if (fromDate.HasValue)
                query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                query.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrEmpty(sortBy))
            {
                query.Add($"sortBy={sortBy}");
                query.Add($"sortOrder={sortOrder}");
            }

            query.Add($"page={page}");
            query.Add($"pageSize={pageSize}");

            var url = "api/Products/admin?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<PagedResponse<ReadProductViewModel>>(url);
        }


        public async Task<ReadProductViewModel> DetailsProductAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadProductViewModel>($"api/Products/{id}");
        }

        public async Task AddProductAsync(CreateProductViewModel model)
        {
            AddAuthorizationHeader();
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.ProductName), "ProductName");
            content.Add(new StringContent(model.ProductPrice.ToString()), "ProductPrice");

            if (model.ProductDescription != null)
                content.Add(new StringContent(model.ProductDescription), "ProductDescription");


            content.Add(new StringContent(model.BrandId.ToString()), "BrandId");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

            // gửi attributes
            if (model.Attributes != null)
            {
                for (int i = 0; i < model.Attributes.Count; i++)
                {
                    content.Add(
                        new StringContent(model.Attributes[i].CategoryAttributeId.ToString()),
                        $"Attributes[{i}].CategoryAttributeId"
                    );

                    content.Add(
                        new StringContent(model.Attributes[i].AttributeValue),
                        $"Attributes[{i}].AttributeValue"
                    );
                }
            }

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

            var response = await _http.PostAsync("api/Products", content);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
        }

        public async Task UpdateProductAsync(Guid? id, UpdateProductViewModel model)
        {
            AddAuthorizationHeader();
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(model.ProductName), "ProductName");
            form.Add(new StringContent(model.ProductPrice.ToString()), "ProductPrice");

            if (!string.IsNullOrEmpty(model.ProductDescription))
                form.Add(new StringContent(model.ProductDescription), "ProductDescription");


            if (model.BrandId != null)
                form.Add(new StringContent(model.BrandId.ToString()), "BrandId");

            if (model.CategoryId != null)
                form.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

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

            // Attributes
            if (model.Attributes != null)
            {
                for (int i = 0; i < model.Attributes.Count; i++)
                {
                    form.Add(new StringContent(model.Attributes[i].CategoryAttributeId.ToString()),
                             $"Attributes[{i}].CategoryAttributeId");

                    form.Add(new StringContent(model.Attributes[i].AttributeValue ?? ""),
                             $"Attributes[{i}].AttributeValue");
                }
            }

            var response = await _http.PutAsync($"api/Products/{id}", form);

            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeProductStatusAsync(
     Guid id,
     Status status)
        {
            AddAuthorizationHeader();

            var response = await _http.PatchAsJsonAsync(
                $"api/Products/{id}/status",
                status
            );

            response.EnsureSuccessStatusCode();
        }
        

        


        public async Task<List<ReadProductViewModelForCustomer>> GetHotProductsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ReadProductViewModelForCustomer>>(
                "api/Products/hot-products");

            return result ?? new List<ReadProductViewModelForCustomer>();
        }

        public async Task<List<ReadProductViewModelForCustomer>> GetNewProductsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ReadProductViewModelForCustomer>>(
                "api/Products/new-products");

            return result ?? new List<ReadProductViewModelForCustomer>();
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
        public async Task<ReadProductViewModel> GetProductByIdIncludeDeletedAsync(Guid? id)
        {
            try
            {
                // Gọi tới Endpoint mới vừa tạo bên Backend
                var response = await _http.GetAsync($"api/Products/{id}/include-deleted");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ReadProductViewModel>();
                }
                return null; // Nếu gặp lỗi (ví dụ sản phẩm thật sự không tồn tại trong DB), trả về null
            }
            catch
            {
                return null;
            }
        }
    }

}
