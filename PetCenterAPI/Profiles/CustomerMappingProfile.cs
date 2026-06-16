using AutoMapper;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Responses.CustomerProfile;
using PetCenterAPI.DTOs.Responses.ManageCustomer;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // ============================================================
            // CUSTOMER — PROFILE
            // ============================================================
            CreateMap<Customer, CustomerProfileResponseDTO>();

            CreateMap<UpdateCustomerProfileRequestDTO, Customer>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))

                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // ============================================================
            // STAFF / ADMIN — CUSTOMER MANAGEMENT
            // ============================================================
            CreateMap<Customer, CustomerResponseDTO>();
        }
    }
}
