using PetCenterClient.Common;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;
using PetCenterClient.ViewModels.Category;
using PetCenterClient.ViewModels.Common;
using System.Net;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class CategoryAPIClient : ICategoryAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddCategoryAsync(CreateCategoryViewModel createCategory)
        {
            AddAuthorizationHeader();

            // Validate CategoryName
            if (string.IsNullOrWhiteSpace(createCategory.CategoryName))
            {
                throw new InvalidOperationException("Category name is required.");
            }

            createCategory.CategoryName = createCategory.CategoryName.Trim();

            // Validate Attributes
            if (createCategory.Attributes != null)
            {
                foreach (var attribute in createCategory.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                    {
                        throw new InvalidOperationException("Attribute name is required.");
                    }

                    attribute.AttributeName = attribute.AttributeName.Trim();
                }
            }

            // Validate CategoryLogo
            if (createCategory.CategoryLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(createCategory.CategoryLogo.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!createCategory.CategoryLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (createCategory.CategoryLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image size cannot exceed 5 MB.");
                }
            }

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(createCategory.CategoryName), "CategoryName");

            if (!string.IsNullOrWhiteSpace(createCategory.CategoryDescription))
            {
                content.Add(new StringContent(createCategory.CategoryDescription.Trim()), "CategoryDescription");
            }

            if (createCategory.CategoryLogo != null)
            {
                var streamContent = new StreamContent(createCategory.CategoryLogo.OpenReadStream());
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(createCategory.CategoryLogo.ContentType);

                content.Add(streamContent, "CategoryLogo", createCategory.CategoryLogo.FileName);
            }

            if (createCategory.Attributes != null)
            {
                for (int i = 0; i < createCategory.Attributes.Count; i++)
                {
                    content.Add(
                        new StringContent(createCategory.Attributes[i].AttributeName),
                        $"Attributes[{i}].AttributeName"
                    );
                }
            }

            var response = await _http.PostAsync("api/Categories", content);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiResponseViewModel<object>>();

                throw new InvalidOperationException(
                    error?.Message ?? "Create category failed.");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeCategoryStatusAsync(Guid id, Status status)
        {
            AddAuthorizationHeader();

            var response = await _http.PatchAsJsonAsync(
                $"api/Categories/{id}/status",
                status);

            response.EnsureSuccessStatusCode();
        }
        public async Task<OdataResponse<ReadCategoryViewModelForCustomer>> GetAllCategoryAsync()
        {
            return await _http.GetFromJsonAsync<
                OdataResponse<ReadCategoryViewModelForCustomer>>(
                    "odata/Categories?$count=true");
        }


        public async Task<PagedResponse<ReadCategoryViewModel>> GetAllCategoryAdminAsync(
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

            var url = "api/Categories/admin?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<PagedResponse<ReadCategoryViewModel>>(url);
        }

        public async Task<ReadCategoryViewModel> DetailsCategoryAsync(Guid? id)
        {
            return await _http.GetFromJsonAsync<ReadCategoryViewModel>($"api/Categories/{id}");
        }

        public async Task UpdateCategoryAsync(Guid? id, UpdateCategoryViewModel updateCategory)
        {
            AddAuthorizationHeader();

            // Validate CategoryName
            if (string.IsNullOrWhiteSpace(updateCategory.CategoryName))
            {
                throw new InvalidOperationException("Category name is required.");
            }

            updateCategory.CategoryName = updateCategory.CategoryName.Trim();

            // Validate Attributes
            if (updateCategory.Attributes != null)
            {
                foreach (var attribute in updateCategory.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                    {
                        throw new InvalidOperationException("Attribute name is required.");
                    }

                    attribute.AttributeName = attribute.AttributeName.Trim();
                }
            }

            // Validate CategoryLogo
            if (updateCategory.CategoryLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(updateCategory.CategoryLogo.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!updateCategory.CategoryLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (updateCategory.CategoryLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image size cannot exceed 5 MB.");
                }
            }

            var form = new MultipartFormDataContent();

            form.Add(new StringContent(updateCategory.CategoryName), "CategoryName");

            if (!string.IsNullOrWhiteSpace(updateCategory.CategoryDescription))
            {
                form.Add(new StringContent(updateCategory.CategoryDescription.Trim()), "CategoryDescription");
            }

            if (updateCategory.CategoryLogo != null)
            {
                var stream = updateCategory.CategoryLogo.OpenReadStream();
                var content = new StreamContent(stream);

                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(updateCategory.CategoryLogo.ContentType);

                form.Add(content, "CategoryLogo", updateCategory.CategoryLogo.FileName);
            }

            if (updateCategory.Attributes != null)
            {
                for (int i = 0; i < updateCategory.Attributes.Count; i++)
                {
                    form.Add(
                        new StringContent(updateCategory.Attributes[i].CategoryAttributeId.ToString()),
                        $"Attributes[{i}].CategoryAttributeId"
                    );

                    form.Add(
                        new StringContent(updateCategory.Attributes[i].AttributeName),
                        $"Attributes[{i}].AttributeName"
                    );
                }
            }

            var response = await _http.PutAsync($"api/Categories/{id}", form);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiResponseViewModel<object>>();

                throw new InvalidOperationException(
                    error?.Message ?? "Update category failed.");
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
