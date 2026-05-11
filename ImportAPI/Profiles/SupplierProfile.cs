using AutoMapper;
using ImportAPI.DTOs;
using ImportAPI.Models;

namespace ImportAPI.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, ReadSupplierDto>();
            CreateMap<WriteSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();
        }
    }
}
