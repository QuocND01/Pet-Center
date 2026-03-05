using AutoMapper;
using OrdersAPI.DTOs;
using OrdersAPI.Models; // Thay bằng namespace chứa class Address của bạn

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderResponseDTO>();
        CreateMap<OrderRequestDTO, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
    }
}