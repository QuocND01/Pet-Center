using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;
using ProductAPI.Service.Interface;

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
        public async Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync()
        {
            var categories = await _categoryRepository.GetAllCategoryAsync();
           return _mapper.Map<IEnumerable<ReadCategoryDTOs>>(categories);
        }

        public async Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            var attributes = await _categoryRepository.GetAllCategoryAttributeByCategoryIDAsync(id);
            return attributes;
        }
    }
}
