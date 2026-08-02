using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterAPI.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // Entity -> DTO
            CreateMap<Category, ReadCategoryDTO>()
                .ForMember(dest => dest.Attributes,
                    opt => opt.MapFrom(src => src.CategoryAttributes));

            CreateMap<Category, ReadCategoryDTOForCustomer>()
                .ForMember(dest => dest.Attributes,
                    opt => opt.MapFrom(src => src.CategoryAttributes));


            // DTO -> Entity
            CreateMap<CreateCategoryDTO, Category>()
                .ForMember(dest => dest.CategoryAttributes,
                    opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.PublicId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Products,
                    opt => opt.Ignore());


            CreateMap<UpdateCategoryDTO, Category>()
                .ForMember(dest => dest.CategoryAttributes,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CategoryLogo,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PublicId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Products,
                    opt => opt.Ignore());
        }
    }
}