using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Models;

namespace CartAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Cart, CartReadDto>()
            .ForMember(dest => dest.CartDetails,
                opt => opt.MapFrom(src => src.CartDetails));

        CreateMap<CartDetail, CartDetailReadDto>()
            .ForMember(dest => dest.ProductName, opt => opt.Ignore())
            .ForMember(dest => dest.ProductPrice, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImage, opt => opt.Ignore());
    }
}