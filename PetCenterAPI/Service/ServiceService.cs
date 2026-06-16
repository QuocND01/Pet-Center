using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Responses.Service.ServiceResponseDTO;
using static System.Net.Mime.MediaTypeNames;

namespace PetCenterAPI.Service
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _ServiceRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        public ServiceService(IServiceRepository ServiceRepository, IMapper mapper, ICloudinaryService service)
        {
            _ServiceRepository = ServiceRepository;
            _mapper = mapper;
            _cloudinaryService = service;
          }


        public async Task AddServiceAsync(CreateServiceDTO createService)
        {
            bool ServiceHasExist = false;
            ServiceHasExist = await _ServiceRepository.CheckServiceExistAsync(createService.ServiceName);
            if (ServiceHasExist)
            {
                throw new InvalidOperationException("Service already exists");
            }
            else
            {
                var Service = _mapper.Map<Models.Service>(createService);

                Service.ServiceId = Guid.NewGuid();
                Service.ServiceImages ??= new List<ServiceImage>();
                if (createService.ImageFiles != null && createService.ImageFiles.Any())
                {
                    foreach (var file in createService.ImageFiles)
                    {
                        // 1️⃣ Upload trước
                        var uploadResult = await _cloudinaryService
                            .UploadImageAsync(file, "Services");

                        if (uploadResult == null ||
                            uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception("Upload ảnh thất bại");
                        }

                        // 2️⃣ Tạo Image entity
                        var image = new ServiceImage
                        {
                            ImageId = Guid.NewGuid(),
                            ImageUrl = uploadResult.SecureUrl.ToString(),
                            PublicId = uploadResult.PublicId,
                            IsActive = true
                        };

                        // 3️⃣ Gán trực tiếp vào navigation property
                        Service.ServiceImages.Add(image);
                    }
                }

                // 4️⃣ Save
                await _ServiceRepository.AddServiceAsync(Service);
            }
        }



        public async Task ChangeServiceStatusAsync(Guid id, Status status)
        {
            var Service = await _ServiceRepository.GetServiceByIdAsync(id);

            if (Service == null)
                throw new Exception("Service not found");

            switch (status)
            {
                case Status.Active:
                case Status.Inactive:
                    {
                        await _ServiceRepository.ChangeServiceStatusAsync(id, status);
                        break;
                    }

                case Status.Deleted:
                    {
                        // 1️⃣ update Service status only
                        await _ServiceRepository.ChangeServiceStatusAsync(id, Status.Deleted);

                        foreach (var image in Service.ServiceImages.ToList())
                        {
                            await _cloudinaryService.DeleteImageAsync(image.ImageUrl);

                            Service.ServiceImages.Remove(image);
                        }

                        await _ServiceRepository.SaveAsync();

                        break;
                    }

                default:
                    throw new Exception("Invalid status");
            }
        }


        public async Task<List<ReadServiceDTOForCustomer>> GetAllServiceAsync(
     ODataQueryOptions<ReadServiceDTOForCustomer> queryOptions)
        {
            var query = _ServiceRepository
                .GetAllService()
                .ProjectTo<ReadServiceDTOForCustomer>(_mapper.ConfigurationProvider);

            var filtered = (IQueryable<ReadServiceDTOForCustomer>)
                queryOptions.ApplyTo(query);

            return await filtered.ToListAsync();
        }


        public async Task<PagedResult<ReadServiceDTO>> GetAllServiceAdminAsync(
    ServiceSpecification spec)
        {
            var (items, total) = await _ServiceRepository.GetAllServiceAdminAsync(spec);

            var ServiceDTOs = _mapper.Map<IEnumerable<ReadServiceDTO>>(items).ToList();

            return new PagedResult<ReadServiceDTO>(
                ServiceDTOs,
                total,
                spec.Page,
                spec.PageSize);
        }



        public async Task<ReadServiceDTO> GetServiceByIdAsync(Guid id)
        {
            var Service = await _ServiceRepository.GetServiceByIdAsync(id);

            if (Service == null)
                throw new KeyNotFoundException("Service not found");

            var result = _mapper.Map<ReadServiceDTO>(Service);

            return result;
        }

        public async Task UpdateServiceAsync(Guid id, UpdateServiceDTO updateService)
        {
            var Service = await _ServiceRepository.GetServiceByIdAsync(id);

            if (Service == null)
                throw new KeyNotFoundException("Service not found");


            bool ServiceHasExist = await _ServiceRepository.CheckServiceExistAsync(updateService.ServiceName, id);
            Console.WriteLine(ServiceHasExist);
            if (ServiceHasExist)
            {
                throw new InvalidOperationException("Service already exists");
            }

            _mapper.Map(updateService, Service);


            Service.ServiceImages ??= new List<ServiceImage>();

            // 1️⃣ xử lý ảnh bị xoá
            var existingImages = updateService.ExistingImages ?? new List<string>();

            var currentImages = Service.ServiceImages
     .Where(x => x.IsActive == true)
     .ToList();

            foreach (var img in currentImages)
            {
                bool stillExists = existingImages.Any(x =>
                    string.Equals(x, img.ImageUrl, StringComparison.OrdinalIgnoreCase));

                if (!stillExists)
                {
                    await _cloudinaryService.DeleteImageAsync(img.PublicId);

                    Service.ServiceImages.Remove(img);
                }

            }

            // 2️⃣ upload ảnh mới
            if (updateService.ImageFiles != null && updateService.ImageFiles.Any())
            {
                foreach (var file in updateService.ImageFiles)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(file, "Services");

                    if (uploadResult == null ||
                        uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Upload ảnh thất bại");
                    }

                    Service.ServiceImages.Add(new ServiceImage
                    {
                        ImageUrl = uploadResult.SecureUrl.ToString(),
                        PublicId = uploadResult.PublicId,
                        ServiceId = Service.ServiceId,
                        IsActive = true
                    });
                }
            }
            await _ServiceRepository.UpdateServiceAsync(Service);
        }
    }
}
