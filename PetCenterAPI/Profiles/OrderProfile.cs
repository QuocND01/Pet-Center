using AutoMapper;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterAPI.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, ReadOrderListDTO>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.Customer.PhoneNumber));
        }
    }
}