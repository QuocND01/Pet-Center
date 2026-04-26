using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class Brandprofile : Profile
    {
        public Brandprofile()
        {
            CreateMap<Brand, ReadBrandDTOs>();

            CreateMap<CreateBrandDTOs, Brand>();

            CreateMap<UpdateBrandDTOs, Brand>()
                .ForMember(dest => dest.BrandLogo,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PublicId,
                    opt => opt.Ignore());
        }
    }
}
