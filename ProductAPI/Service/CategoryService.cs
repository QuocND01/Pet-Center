using AutoMapper;
using AutoMapper.QueryableExtensions;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Repository;
using ProductAPI.Repository.Interface;
using ProductAPI.Service.Interface;
using System.Xml.Linq;

namespace ProductAPI.Service
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

        public async Task AddAttribute(CreateCategoryAttributeDTOs attributeValue)
        {
            var addtribute = _mapper.Map<CategoryAttribute>(attributeValue);
            await _categoryRepository.AddAttribute(addtribute);
        }

        public async Task AddCategoryAsync(CreateCategoryDTOs createCategory)
        {

            if (createCategory.Attributes != null && createCategory.Attributes.Any())
            {
                if (createCategory.Attributes.Any(a => string.IsNullOrWhiteSpace(a.AttributeName)))
                {
                    throw new InvalidOperationException("Attribute name cannot be empty");
                }

                var duplicate = createCategory.Attributes
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

            bool hasExist = await _categoryRepository
                .CheckCategoryExist(createCategory.CategoryName);

            if (hasExist)
            {
                throw new InvalidOperationException("Category already exists");
            }

            var category = _mapper.Map<Category>(createCategory);

            category.CategoryId = Guid.NewGuid();

            // 👇 xử lý upload ảnh
            if (createCategory.CategoryLogo != null)
            {
                var uploadResult = await _cloudinaryService
                    .UploadImageAsync(createCategory.CategoryLogo, "categories");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload category logo");
                }

                category.CategoryLogo = uploadResult.SecureUrl.ToString();
                category.PublicId = uploadResult.PublicId;
            }

            await _categoryRepository.AddCategoryAsync(category);
        }


        public async Task DeleteCategoryAsync(Guid id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public IQueryable<ReadCategoryDTOs> GetAllCategory()
        {
            return _categoryRepository.GetAllCategory().ProjectTo<ReadCategoryDTOs>(_mapper.ConfigurationProvider);
        }

        public async Task<IEnumerable<ReadCategoryAttributeDTOs>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            var attributes = await _categoryRepository.GetAllCategoryAttributeByCategoryIDAsync(id);

            var readattribute = _mapper.Map<IEnumerable<ReadCategoryAttributeDTOs>>(attributes);

            return readattribute;
        }

        public async Task<ReadCategoryDTOs?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            return _mapper.Map<ReadCategoryDTOs>(category);
        }

        public async Task UpdateCategoryAsync(Guid id, UpdateCategoryDTOs category)
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

            bool hasExist = await _categoryRepository
                .CheckCategoryExist(category.CategoryName);

            if (hasExist &&
                !string.Equals(existingCategory.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase))
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
                        string.Equals(a.AttributeName, oldAttr.AttributeName, StringComparison.OrdinalIgnoreCase)))
                    {
                        oldAttr.IsActive = false;
                    }
                }
                foreach (var newAttr in category.Attributes)
                {
                    var match = existingAttrs.FirstOrDefault(a =>
                        string.Equals(a.AttributeName, newAttr.AttributeName, StringComparison.OrdinalIgnoreCase));

                    if (match == null)
                    {
                        existingAttrs.Add(new CategoryAttribute
                        {
                            CategoryId = id,
                            AttributeName = newAttr.AttributeName,
                            IsActive = true
                        });
                    }
                    else
                    {
                        match.IsActive = true;
                    }
                }
            }
            await _categoryRepository.UpdateCategoryAsync(existingCategory);
        }
    }
}
