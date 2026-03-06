using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Repository;
using ProductAPI.Repository.Interface;
using ProductAPI.Service.Interface;

namespace ProductAPI.Service
{
    public class BrandService : IBrandService
    {
        private IBrandRepository _brandRepository;
        private readonly IMapper _mapper;

        public BrandService(IBrandRepository brandRepository, IMapper mapper) {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync()
        {
            var Brands = await _brandRepository.GetAllBrandAsync();
         return _mapper.Map<IEnumerable<ReadBrandDTOs>>(Brands);
        }
    }
}
