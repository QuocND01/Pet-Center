using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            CreateMap<Brand, ReadBrandDTO>();
            CreateMap<Brand, ReadBrandDTOForCustomer>();

            CreateMap<CreateBrandDTO, Brand>();

            CreateMap<UpdateBrandDTO, Brand>()
         .ForMember(dest => dest.BrandId,
             opt => opt.Ignore())
         .ForMember(dest => dest.BrandLogo,
             opt => opt.Ignore())
         .ForMember(dest => dest.PublicId,
             opt => opt.Ignore());
        }
    }
}
