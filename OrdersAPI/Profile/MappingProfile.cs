using AutoMapper;
using OrdersAPI.DTOs;
using OrdersAPI.Models;

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
            .ForMember(dest => dest.OrderDetailId, opt => opt.Ignore()); 
    }
}