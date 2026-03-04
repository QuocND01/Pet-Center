using AutoMapper;
using ProductAPI.Models;
using ProductAPI.DTOs;

namespace ProductAPI.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, ReadSupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();
        }
    }
}
