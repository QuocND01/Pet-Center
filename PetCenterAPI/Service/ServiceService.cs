using AutoMapper;
using AutoMapper.QueryableExtensions;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using System.Net;
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
            bool serviceHasExist = await _ServiceRepository.CheckServiceExistAsync(createService.ServiceName);

            if (serviceHasExist)
            {
                throw new InvalidOperationException("Service already exists");
            }

            if (createService.ImageFiles?.Count > 10)
            {
                throw new BadHttpRequestException("Maximum 10 images are allowed.");
            }

            var service = _mapper.Map<Models.Service>(createService);

            service.ServiceId = Guid.NewGuid();
            service.ServiceImages ??= new List<ServiceImage>();

            var uploadedImages = new List<ImageUploadResult>();

            try
            {
                if (createService.ImageFiles != null && createService.ImageFiles.Any())
                {
                    var uploadTasks = createService.ImageFiles
                        .Select(file => _cloudinaryService.UploadImageAsync(file, "services"));

                    uploadedImages = (await Task.WhenAll(uploadTasks)).ToList();

                    foreach (var uploadResult in uploadedImages)
                    {
                        if (uploadResult == null ||
                            uploadResult.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Upload ảnh thất bại");
                        }

                        service.ServiceImages.Add(new ServiceImage
                        {
                            ImageId = Guid.NewGuid(),
                            ImageUrl = uploadResult.SecureUrl.ToString(),
                            PublicId = uploadResult.PublicId,
                            IsActive = true
                        });
                    }
                }

                await _ServiceRepository.AddServiceAsync(service);
            }
            catch
            {
                var deleteTasks = uploadedImages
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.PublicId))
                    .Select(x => _cloudinaryService.DeleteImageAsync(x.PublicId));

                await Task.WhenAll(deleteTasks);

                throw;
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
                            await _cloudinaryService.DeleteImageAsync(image.PublicId);

                            _ServiceRepository.DeleteServiceImage(image);
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

            var finalImageCount =
               (updateService.ExistingImages?.Count ?? 0) +
               (updateService.ImageFiles?.Count ?? 0);

            if (finalImageCount > 10)
                throw new BadHttpRequestException("Maximum 10 images are allowed.");

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

                    _ServiceRepository.DeleteServiceImage(img);
                }

            }

            // 2️⃣ upload ảnh mới
            if (updateService.ImageFiles != null && updateService.ImageFiles.Any())
            {
                var uploadTasks = updateService.ImageFiles
     .Select(file => _cloudinaryService.UploadImageAsync(file, "services"));

                var uploadResults = await Task.WhenAll(uploadTasks);

                foreach (var uploadResult in uploadResults)
                {
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
