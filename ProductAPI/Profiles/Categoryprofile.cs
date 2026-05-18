using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
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

            // Create DTO -> Entity
            CreateMap<CreateCategoryDTO, Category>()
                .ForMember(dest => dest.CategoryAttributes,
                    opt => opt.MapFrom(src => src.Attributes));

            // Update DTO -> Entity
            CreateMap<UpdateCategoryDTO, Category>()
         .ForMember(dest => dest.CategoryAttributes,
             opt => opt.Ignore()) // update xử lý riêng
         .ForMember(dest => dest.CategoryLogo,
             opt => opt.Ignore())
         .ForMember(dest => dest.PublicId,
             opt => opt.Ignore());
        }
    }
}