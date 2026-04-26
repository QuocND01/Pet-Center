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
        private readonly ICloudinaryService _cloudinaryService;

        public BrandService(IBrandRepository brandRepository, IMapper mapper, ICloudinaryService cloudinaryService = null)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
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
            bool brandHasExist = await _brandRepository
                .CheckBrandExist(createBrand.BrandName);

            if (brandHasExist)
            {
                throw new InvalidOperationException("Brand already exists");
            }

            var brand = _mapper.Map<Brand>(createBrand);

            brand.BrandId = Guid.NewGuid();

            // 👇 xử lý upload ảnh giống Product
            if (createBrand.BrandLogo != null)
            {
                var uploadResult = await _cloudinaryService
                    .UploadImageAsync(createBrand.BrandLogo, "brands");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload brand logo");
                }

                // 👇 lưu vào Brand
                brand.BrandLogo = uploadResult.SecureUrl.ToString();
                brand.PublicId = uploadResult.PublicId;
            }

            // Save
            await _brandRepository.AddBrandAsync(brand);
        }

        public async Task UpdateBrandAsync(Guid id, UpdateBrandDTOs updateBrand)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);

            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            bool brandHasExist = await _brandRepository
                .CheckBrandExist(updateBrand.BrandName);

            if (brandHasExist &&
                !string.Equals(brand.BrandName, updateBrand.BrandName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Brand already exists");
            }
            _mapper.Map(updateBrand, brand);

            if (updateBrand.BrandLogo != null)
            {
                if (!string.IsNullOrEmpty(brand.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(brand.PublicId);
                }

                var uploadResult = await _cloudinaryService
                    .UploadImageAsync(updateBrand.BrandLogo, "brands");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload brand logo");
                }

                brand.BrandLogo = uploadResult.SecureUrl.ToString();
                brand.PublicId = uploadResult.PublicId;
            }

            await _brandRepository.UpdateBrandAsync(brand);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            await _brandRepository.DeleteBrandAsync(id);
        }
    }
}
