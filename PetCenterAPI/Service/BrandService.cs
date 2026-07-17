using AutoMapper;
using AutoMapper.QueryableExtensions;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Responses.Brand.BrandResposeDTO;

namespace PetCenterAPI.Service
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

            if (string.IsNullOrWhiteSpace(createBrand.BrandName))
            {
                throw new Exception("Brand name is required");
            }

            createBrand.BrandName = createBrand.BrandName.Trim();

            if (brandHasExist)
            {
                throw new InvalidOperationException("Brand already exists");
            }

            var brand = _mapper.Map<Brand>(createBrand);

            brand.BrandId = Guid.NewGuid();

            // 👇 xử lý upload ảnh giống Product
            if (createBrand.BrandLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(createBrand.BrandLogo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!createBrand.BrandLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (createBrand.BrandLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image size cannot exceed 5 MB.");
                }

                var uploadResult = await _cloudinaryService.UploadImageAsync(createBrand.BrandLogo, "brands");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload brand logo");
                }

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

            if (string.IsNullOrWhiteSpace(updateBrand.BrandName))
            {
                throw new Exception("Brand name is required");
            }

            updateBrand.BrandName = updateBrand.BrandName.Trim();

            bool brandHasExist = await _brandRepository.CheckBrandExistAsync(updateBrand.BrandName, id);

            if (brandHasExist)
            {
                throw new InvalidOperationException("Brand already exists");
            }
            _mapper.Map(updateBrand, brand);

            if (updateBrand.BrandLogo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(updateBrand.BrandLogo.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                }

                if (!updateBrand.BrandLogo.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("Invalid image file.");
                }

                // Giới hạn 5MB
                if (updateBrand.BrandLogo.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image size cannot exceed 5 MB.");
                }

                // Upload ảnh mới trước
                var uploadResult = await _cloudinaryService
                    .UploadImageAsync(updateBrand.BrandLogo, "brands");

                if (uploadResult == null ||
                    uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload brand logo");
                }

                // Upload thành công mới xóa ảnh cũ
                if (!string.IsNullOrEmpty(brand.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(brand.PublicId);
                }

                // Cập nhật thông tin ảnh mới
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
