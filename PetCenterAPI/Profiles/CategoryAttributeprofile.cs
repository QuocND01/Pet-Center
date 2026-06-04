using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class CategoryAttributeProfile : Profile
    {
        public CategoryAttributeProfile()
        {
            // Entity -> DTO
            CreateMap<CategoryAttribute, ReadCategoryAttributeDTOs>();

            // Create DTO -> Entity
            CreateMap<CreateCategoryAttributeDTOs, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CategoryId,
                    opt => opt.Ignore());

            // Update DTO -> Entity
            CreateMap<UpdateCategoryAttributeDTOs, CategoryAttribute>()
                .ForMember(dest => dest.CategoryAttributeId, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore());
        }
    }
}