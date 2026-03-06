using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Profiles
{
    public class Categoryprofile : Profile
    {
        public Categoryprofile()
        {
            CreateMap<Category, ReadCategoryDTOs>();
            CreateMap<UpdateCategoryDTOs, Category>();
            CreateMap<CreateCategoryDTOs, Category>();
        }
    }
}
