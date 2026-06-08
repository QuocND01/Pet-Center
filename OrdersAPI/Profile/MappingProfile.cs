using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderResponseDTO>();
        CreateMap<OrderRequestDTO, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now));
        CreateMap<OrderDetail, OrderDetailResponseDTO>();
        CreateMap<OrderDetailRequestDTO, OrderDetail>()
            .ForMember(dest => dest.OrderDetailsId, opt => opt.Ignore()); 
    }
}