using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using ProductAPI.DTOs;

namespace PetCenterClient.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public async Task<OdataResponse<ReadProductDTO>> GetAllProductAsync(
     string? search,
     bool? isActive,
     decimal? minPrice,
     decimal? maxPrice,
     DateTime? fromDate,
     DateTime? toDate,
     string? sortBy,
     string sortOrder = "asc",
     int page = 1)
        {
            int pageSize = 10;

            if (page < 1)
                page = 1;

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("'", "''");
                filters.Add($"contains(ProductName,'{search}')");
            }

            if (isActive.HasValue)
                filters.Add($"IsActive eq {isActive.Value.ToString().ToLower()}");

            if (minPrice.HasValue)
                filters.Add($"ProductPrice ge {minPrice.Value}");

            if (maxPrice.HasValue)
                filters.Add($"ProductPrice le {maxPrice.Value}");

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

            var response = await _http.GetFromJsonAsync<OdataResponse<ReadProductDTO>>(
                "odata/products" + url
            );

            return response;
        }

        public async Task<ReadProductDTO> GetProductByIdAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadProductDTO>($"api/Products/{id}");
        }

        public async Task AddProductAsync(CreateProductDTO model)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.ProductName), "ProductName");
            content.Add(new StringContent(model.ProductPrice.ToString()), "ProductPrice");

            if (model.ProductDescription != null)
                content.Add(new StringContent(model.ProductDescription), "ProductDescription");

            if (model.StockQuantity != null)
                content.Add(new StringContent(model.StockQuantity.ToString()), "StockQuantity");

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

            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Response: " + result);
        }

        public async Task UpdateProductAsync(Guid? id, UpdateProductDTO model)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(model.ProductName), "ProductName");
            form.Add(new StringContent(model.ProductPrice.ToString()), "ProductPrice");

            if (!string.IsNullOrEmpty(model.ProductDescription))
                form.Add(new StringContent(model.ProductDescription), "ProductDescription");

            if (model.StockQuantity != null)
                form.Add(new StringContent(model.StockQuantity.ToString()), "StockQuantity");

            if (model.BrandId != null)
                form.Add(new StringContent(model.BrandId.ToString()), "BrandId");

            if (model.CategoryId != null)
                form.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

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
            foreach (var attr in model.Attributes)
            {
                Console.WriteLine(attr.CategoryAttributeId);
            }

            var response = await _http.PutAsync($"api/Products/{id}", form);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductAsync(Guid? id)
        {
            await _http.DeleteAsync($"api/Products/{id}");
        }
    }

}
