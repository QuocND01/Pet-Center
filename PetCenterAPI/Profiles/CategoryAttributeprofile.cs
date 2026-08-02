using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Category.CategoryAttributeRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryAttributeResponseDTO;

namespace PetCenterAPI.Profiles
{
    public class CategoryAttributeProfile : Profile
    {
        public CategoryAttributeProfile()
        {
            // Entity -> DTO
            CreateMap<CategoryAttribute, ReadCategoryAttributeDTO>();


            // DTO -> Entity
            CreateMap<CreateCategoryAttributeDTO, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CategoryId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Category,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes,
                    opt => opt.Ignore());


            CreateMap<UpdateCategoryAttributeDTO, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Category,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes,
                    opt => opt.Ignore());
        }
    }
}