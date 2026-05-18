using AutoMapper;
using AutoMapper.QueryableExtensions;
using ProductAPI.Common;
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

        public IQueryable<ReadBrandDTOForCustomer> GetAllBrand()
        {
            return _brandRepository.GetAllBrand().ProjectTo<ReadBrandDTOForCustomer>(_mapper.ConfigurationProvider);
        }

        public async Task<PagedResult<ReadBrandDTO>> GetAllBrandAdminAsync(
     BrandSpecification spec)
        {
            var (items, total) = await _brandRepository.GetAllBrandAdminAsync(spec);

            return new PagedResult<ReadBrandDTO>(
                _mapper.Map<IEnumerable<ReadBrandDTO>>(items),
                total,
                spec.Page,
                spec.PageSize);
        }


        public async Task<ReadBrandDTO?> GetBrandByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);
            return _mapper.Map<ReadBrandDTO>(brand);
        }

        public async Task AddBrandAsync(CreateBrandDTO createBrand)
        {
            bool brandHasExist = await _brandRepository
                .CheckBrandExistAsync(createBrand.BrandName);

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

        public async Task UpdateBrandAsync(Guid id, UpdateBrandDTO updateBrand)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);

            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            bool brandHasExist = await _brandRepository
                .CheckBrandExistAsync(updateBrand.BrandName);

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

        public async Task ChangeBrandStatusAsync(
     Guid id,
     Status status)
        {
            var brand = await _brandRepository.GetBrandByIdAsync(id);

            if (brand == null)
                throw new Exception("Brand not found");

            await _brandRepository.ChangeBrandStatusAsync(id, status);
        }


    }
}
