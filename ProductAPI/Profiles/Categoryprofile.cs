using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class Categoryprofile : Profile
    {
        public Categoryprofile()
        {
            // Entity -> DTO
            CreateMap<Category, ReadCategoryDTOs>()
                .ForMember(dest => dest.Attributes,
                    opt => opt.MapFrom(src => src.CategoryAttributes));

            // Create DTO -> Entity
            CreateMap<CreateCategoryDTOs, Category>()
                .ForMember(dest => dest.CategoryAttributes,
                    opt => opt.MapFrom(src => src.Attributes));

            // Update DTO -> Entity
            CreateMap<UpdateCategoryDTOs, Category>()
         .ForMember(dest => dest.CategoryAttributes,
             opt => opt.Ignore()) // update xử lý riêng
         .ForMember(dest => dest.CategoryLogo,
             opt => opt.Ignore())
         .ForMember(dest => dest.PublicId,
             opt => opt.Ignore());
        }
    }
}