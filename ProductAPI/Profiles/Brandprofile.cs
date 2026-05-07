using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
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
