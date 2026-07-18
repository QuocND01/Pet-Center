using AutoMapper;
using AutoMapper.QueryableExtensions;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using System.Xml.Linq;
using static PetCenterAPI.DTOs.Requests.Category.CategoryAttributeRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterAPI.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }


        public async Task AddCategoryAsync(CreateCategoryDTO createCategory)
        {

            if (string.IsNullOrWhiteSpace(createCategory.CategoryName))
            {
                throw new Exception("Category name is required");
            }

            createCategory.CategoryName = createCategory.CategoryName.Trim();

            if (createCategory.Attributes != null && createCategory.Attributes.Any())
            {
                // Tối đa 10 attributes
                if (createCategory.Attributes.Count > 10)
                {
                    throw new InvalidOperationException("Maximum 10 attributes are allowed.");
                }

                // Validate và Trim
                foreach (var attribute in createCategory.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                    {
                        throw new InvalidOperationException("Attribute name is required.");
                    }

                    attribute.AttributeName = attribute.AttributeName.Trim();
                }

                // Kiểm tra trùng
                var duplicate = createCategory.Attributes
                    .GroupBy(x => x.AttributeName.ToLowerInvariant())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicate.Any())
                {
                    throw new InvalidOperationException(
                        $"Duplicate attributes: {string.Join(", ", duplicate)}");
                }
            }

            bool hasExist = await _categoryRepository
                .CheckCategoryExistAsync(createCategory.CategoryName);

            if (hasExist)
            {
                throw new InvalidOperationException("Category already exists");
            }

            var category = _mapper.Map<Category>(createCategory);

            category.CategoryId = Guid.NewGuid();

            // 👇 xử lý upload ảnh
            if (createCategory.CategoryLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(createCategory.CategoryLogo.FileName).ToLowerInvariant();

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

                var uploadResult = await _cloudinaryService.UploadImageAsync(createCategory.CategoryLogo, "categories");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload category logo");
                }

                category.CategoryLogo = uploadResult.SecureUrl.ToString();
                category.PublicId = uploadResult.PublicId;
            }
            Console.WriteLine(category.CategoryAttributes?.Count);

            foreach (var attr in category.CategoryAttributes ?? [])
            {
                Console.WriteLine(attr.AttributeName);
            }

            await _categoryRepository.AddCategoryAsync(category);
        }


        public async Task ChangeCategoryStatusAsync(
      Guid id,
      Status status)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
                throw new Exception("Category not found");

            await _categoryRepository.ChangeCategoryStatusAsync(id, status);
        }

        public IQueryable<ReadCategoryDTOForCustomer> GetAllCategory()
        {
            return _categoryRepository.GetAllCategory().ProjectTo<ReadCategoryDTOForCustomer>(_mapper.ConfigurationProvider);
        }

        public async Task<PagedResult<ReadCategoryDTO>> GetAllCategoryAdminAsync(
    CategorySpecification spec)
        {
            var (items, total) = await _categoryRepository.GetAllCategoryAdminAsync(spec);

            return new PagedResult<ReadCategoryDTO>(
                _mapper.Map<IEnumerable<ReadCategoryDTO>>(items),
                total,
                spec.Page,
                spec.PageSize);
        }


        public async Task<IEnumerable<ReadCategoryAttributeDTO>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            var attributes = await _categoryRepository.GetAllCategoryAttributeByCategoryIDAsync(id);

            var readattribute = _mapper.Map<IEnumerable<ReadCategoryAttributeDTO>>(attributes);

            return readattribute;
        }

        public async Task<ReadCategoryDTO?> GetCategoryByIdAsync(Guid id)
        {

            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
                throw new KeyNotFoundException("Category not found");

            return _mapper.Map<ReadCategoryDTO>(category);
        }

        public async Task UpdateCategoryAsync(Guid id, UpdateCategoryDTO category)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);

            if (existingCategory == null)
                throw new KeyNotFoundException("Category not found");

            if (category.Attributes != null && category.Attributes.Any())
            {
                if (category.Attributes.Any(a => string.IsNullOrWhiteSpace(a.AttributeName)))
                {
                    throw new InvalidOperationException("Attribute name cannot be empty");
                }
                var duplicate = category.Attributes
                    .GroupBy(x => x.AttributeName.Trim().ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.First().AttributeName)
                    .ToList();

                if (duplicate.Any())
                {
                    throw new InvalidOperationException(
                        $"Duplicate attributes: {string.Join(", ", duplicate)}"
                    );
                }
            }

            bool hasExist = await _categoryRepository.CheckCategoryExistAsync(category.CategoryName, id);

            if (hasExist)
            {
                throw new InvalidOperationException("Category already exists");
            }

            _mapper.Map(category, existingCategory);

            if (category.CategoryLogo != null)
            {
                if (!string.IsNullOrEmpty(existingCategory.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(existingCategory.PublicId);
                }
                var uploadResult = await _cloudinaryService
                    .UploadImageAsync(category.CategoryLogo, "categories");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload category logo");
                }

                existingCategory.CategoryLogo = uploadResult.SecureUrl.ToString();
                existingCategory.PublicId = uploadResult.PublicId;
            }
            if (category.Attributes != null)
            {
                existingCategory.CategoryAttributes ??= new List<CategoryAttribute>();

                var existingAttrs = existingCategory.CategoryAttributes;
                foreach (var oldAttr in existingAttrs)
                {
                    if (!category.Attributes.Any(a =>
                        a.CategoryAttributeId == oldAttr.CategoryAttributeId))
                    {
                        oldAttr.IsActive = false;
                    }
                }

                foreach (var attrDto in category.Attributes)
                {
                    var existingAttr = existingAttrs.FirstOrDefault(a =>
                        a.CategoryAttributeId == attrDto.CategoryAttributeId);
                    if (existingAttr != null)
                    {
                        existingAttr.AttributeName = attrDto.AttributeName;
                        existingAttr.IsActive = true;
                    }
                    else
                    {
                        existingAttrs.Add(new CategoryAttribute
                        {
                            CategoryId = id,
                            AttributeName = attrDto.AttributeName,
                            IsActive = true
                        });
                    }
                }
            }
            await _categoryRepository.UpdateCategoryAsync(existingCategory);
        }
    }
    }
