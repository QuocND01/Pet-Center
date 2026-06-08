using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, ReadSupplierDto>();
            CreateMap<WriteSupplierDto, Supplier>();
        }
    }
}
