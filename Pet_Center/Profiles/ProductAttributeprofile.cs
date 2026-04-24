using AutoMapper;
using global::ProductAPI.DTOs;
using global::ProductAPI.Models;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class ProductAttributeProfile : Profile
    {
        public ProductAttributeProfile()
        {
            // Entity -> DTO
            CreateMap<ProductAttribute, ProductAttributedto>()
                .ForMember(dest => dest.CategoryAttributeId,
                    opt => opt.MapFrom(src => src.CategoryAttributeId))
                .ForMember(dest => dest.AttributeName,
                    opt => opt.MapFrom(src => src.CategoryAttribute.AttributeName))
                .ForMember(dest => dest.AttributeValue,
                    opt => opt.MapFrom(src => src.AttributeValue));

            // Create DTO -> Entity
            CreateMap<CreateProductAttributeDTO, ProductAttribute>()
                .ForMember(dest => dest.ProductAttributesId,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.ProductId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => true));

            // Update DTO -> Entity
            CreateMap<UpdateProductAttributeDTO, ProductAttribute>()
                .ForMember(dest => dest.ProductAttributesId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryAttribute, opt => opt.Ignore());
        }
    }
}
