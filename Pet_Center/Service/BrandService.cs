using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        //public async Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync()
        //{
        //    var Brands = await _brandRepository.GetAllBrandAsync();
        // return _mapper.Map<IEnumerable<ReadBrandDTOs>>(Brands);
        //}

        public IQueryable<ReadBrandDTOs> GetAllBrand()
        {
            return _brandRepository.GetAllBrand().ProjectTo<ReadBrandDTOs>(_mapper.ConfigurationProvider);
        }

        public async Task<ReadBrandDTOs?> GetBrandByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);
            return _mapper.Map<ReadBrandDTOs>(brand);
        }

        public async Task AddBrandAsync(CreateBrandDTOs createBrand)
        {
            bool brandHasExist = false;
            brandHasExist = await _brandRepository.CheckBrandExist(createBrand.BrandName);
            if (brandHasExist)
            {
                throw new InvalidOperationException("Brand already exists");
            }
            else
            {
                var brand = _mapper.Map<Brand>(createBrand);

                brand.BrandId = Guid.NewGuid();

                // 4️⃣ Save
                await _brandRepository.AddBrandAsync(brand);
            }
        }

        public async Task UpdateBrandAsync(Guid id , UpdateBrandDTOs updateBrand)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);

            if (brand == null)
                throw new Exception("Brand not found");

            _mapper.Map(updateBrand, brand);
           await _brandRepository.UpdateBrandAsync(brand);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            await _brandRepository.DeleteBrandAsync(id);
        }
    }
}
