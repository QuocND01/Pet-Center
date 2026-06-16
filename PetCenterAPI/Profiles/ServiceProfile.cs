using AutoMapper;
using CloudinaryDotNet;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Responses.Service.ServiceResponseDTO;

namespace PetCenterAPI.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            // Entity -> Read DTO
            CreateMap<Models.Service, ReadServiceDTO>()
                .ForMember(dest => dest.ImageFiles,
                    opt => opt.MapFrom(src =>
                        src.ServiceImages.Select(i => i.ImageUrl)));

            // Entity -> Read DTO For Customer
            CreateMap<Models.Service, ReadServiceDTOForCustomer>()
                .ForMember(dest => dest.ImageFiles,
                    opt => opt.MapFrom(src =>
                        src.ServiceImages.Select(i => i.ImageUrl)));

            // Create Service
            CreateMap<CreateServiceDTO, Models.Service>()
                .ForMember(dest => dest.ServiceId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ServiceImages,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.Ignore());

            // Update Service
            CreateMap<UpdateServiceDTO, Models.Service>()
                .ForMember(dest => dest.ServiceId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ServiceImages,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.Ignore());
        }
    }
}
