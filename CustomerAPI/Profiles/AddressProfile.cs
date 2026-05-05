using AutoMapper;
using CustomerAPI.DTOs.Address;
using CustomerAPI.Models;

namespace CustomerAPI.Profiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, ViewAddressDto>();

            CreateMap<CreateAddressDto, Address>();
            CreateMap<UpdateAddressDto, Address>();
        }
    }
}
