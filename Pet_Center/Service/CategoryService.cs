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

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task AddAttribute(CreateCategoryAttributeDTOs attributeValue)
        {
            var addtribute = _mapper.Map<CategoryAttribute>(attributeValue);
            await _categoryRepository.AddAttribute(addtribute);
        }

        public async Task AddCategoryAsync(CreateCategoryDTOs createCategory)
        {
            bool hasExist = false;
            hasExist = await _categoryRepository.CheckCategoryExist(createCategory.CategoryName);
            if (hasExist) {
                throw new InvalidOperationException("Category already exists");
            }

            var category = _mapper.Map<Category>(createCategory);
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

        //public async Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync()
        //{
        //    var categories = await _categoryRepository.GetAllCategoryAsync();
        //   return _mapper.Map<IEnumerable<ReadCategoryDTOs>>(categories);
        //}

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
                throw new Exception("Category not found");

            bool hasExist = await _categoryRepository.CheckCategoryExist(category.CategoryName);

            if (hasExist && existingCategory.CategoryName != category.CategoryName)
                throw new InvalidOperationException("Category already exists");

            // update category info
            _mapper.Map(category, existingCategory);

            // update attributes
            if (category.Attributes != null)
            {
                await _categoryRepository.DeleteAttributeByCategoryID(id);

                existingCategory.CategoryAttributes ??= new List<CategoryAttribute>();

                foreach (var attr in category.Attributes)
                {
                    var entity = _mapper.Map<CategoryAttribute>(attr);
                    entity.CategoryId = id;

                    existingCategory.CategoryAttributes.Add(entity);
                }
            }

            await _categoryRepository.UpdateCategoryAsync(existingCategory);
        }
    }
}
