using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ReadProductDTO>()
            .ForMember(dest => dest.BrandName,
                opt => opt.MapFrom(src => src.Brand.BrandName))
            .ForMember(dest => dest.BrandLogo,
                opt => opt.MapFrom(src => src.Brand.BrandLogo))
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images.Select(i => i.ImageUrl)))
            .ForMember(dest => dest.Attributes,
                opt => opt.MapFrom(src => src.ProductAttributes));


            CreateMap<ProductAttribute, ProductAttributedto>()
       .ForMember(dest => dest.CategoryAttributeId,
           opt => opt.MapFrom(src => src.CategoryAttributeId))
       .ForMember(dest => dest.AttributeName,
           opt => opt.MapFrom(src => src.CategoryAttribute.AttributeName))
       .ForMember(dest => dest.AttributeValue,
           opt => opt.MapFrom(src => src.AttributeValue));


            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes,
                     opt => opt.MapFrom(src =>
                         src.Attributes == null
                             ? new List<ProductAttribute>()
                             : src.Attributes.Select(a => new ProductAttribute
                             {
                                 CategoryAttributeId = a.CategoryAttributeId,
                                 AttributeValue = a.AttributeValue
                             })
                     )
                );


            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes, opt => opt.Ignore());

            CreateMap<Product, SelectProductDto>();
        }
    }

}
