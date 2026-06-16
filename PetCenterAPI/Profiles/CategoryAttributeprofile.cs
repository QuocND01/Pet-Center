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

            // Create DTO -> Entity
            CreateMap<CreateCategoryAttributeDTO, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CategoryId,
                    opt => opt.Ignore());

            // Update DTO -> Entity
            CreateMap<UpdateCategoryAttributeDTO, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore());
        }
    }
}