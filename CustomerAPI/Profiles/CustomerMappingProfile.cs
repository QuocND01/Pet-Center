using AutoMapper;
using CustomerAPI.DTOs.Request;
using CustomerAPI.DTOs.Response;
using CustomerAPI.Models;

namespace CustomerAPI.Profiles
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Customer → ProfileResponseDto
            CreateMap<Customer, CustomerProfileResponseDto>();

            // UpdateRequest → Customer (chỉ map các field được phép update)
            CreateMap<UpdateCustomerProfileRequestDto, Customer>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                // Không cho update các field nhạy cảm
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Customer, CustomerResponseDto>();
        }
    }
}
