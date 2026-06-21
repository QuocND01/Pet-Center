using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductResponseDTO;

namespace PetCenterAPI.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Entity -> Read DTO
            CreateMap<Product, ReadProductDTO>()
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand.BrandName))
                .ForMember(dest => dest.BrandLogo,
                    opt => opt.MapFrom(src => src.Brand.BrandLogo))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.Images,
                    opt => opt.MapFrom(src => src.ProductImages.Select(i => i.ImageUrl)))
                .ForMember(dest => dest.Attributes,
                    opt => opt.MapFrom(src => src.ProductAttributes))
                .ForMember(dest => dest.StockQuantity,
                    opt => opt.MapFrom(src =>
                        src.Inventory != null
                           ? src.Inventory.QuantityAvailable
                                : 0))
                //GET SKU-Vinh
                .ForMember(dest => dest.SKU,
                    opt => opt.MapFrom(src =>
                        src.Inventory != null
                            ? src.Inventory.SKU
                :               string.Empty));


            CreateMap<Product, ReadProductDTOForCustomer>()
             .ForMember(dest => dest.BrandName,
                 opt => opt.MapFrom(src => src.Brand.BrandName))
             .ForMember(dest => dest.BrandLogo,
                 opt => opt.MapFrom(src => src.Brand.BrandLogo))
             .ForMember(dest => dest.CategoryName,
                 opt => opt.MapFrom(src => src.Category.CategoryName))
             .ForMember(dest => dest.Images,
                 opt => opt.MapFrom(src => src.ProductImages.Select(i => i.ImageUrl)))
             .ForMember(dest => dest.Attributes,
                 opt => opt.MapFrom(src => src.ProductAttributes))
                 .ForMember(dest => dest.StockQuantity,
                        opt => opt.MapFrom(src =>
                            src.Inventory != null
                                ? src.Inventory.QuantityAvailable
                                : 0));

            // Create Product
            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes,
                    opt => opt.MapFrom(src => src.Attributes));

            // Update Product
            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.ProductAttributes, opt => opt.Ignore())
                 .ForMember(dest => dest.Status, opt => opt.Ignore());

            //CreateMap<Product, SelectProductDto>();
            //CreateMap<IncreaseStockItemDto, Product>();
            ////import product snapshot
            //CreateMap<Product, ProductSnapshotResponseDto>()
            //    .ForMember(
            //        dest => dest.CategoryName,
            //        opt => opt.MapFrom(src => src.Category!.CategoryName)
            //    )
            //    .ForMember(
            //        dest => dest.BrandName,
            //        opt => opt.MapFrom(src => src.Brand!.BrandName)
            //    );
        }
    }

}
