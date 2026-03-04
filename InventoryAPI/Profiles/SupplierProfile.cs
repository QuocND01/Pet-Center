using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Models;

namespace InventoryAPI.Profiles
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
