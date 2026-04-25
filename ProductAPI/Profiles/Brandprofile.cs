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
            CreateMap<UpdateBrandDTOs, Brand>();
            CreateMap<CreateBrandDTOs, Brand>();
        }
    }
}
