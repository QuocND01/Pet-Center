using AutoMapper;
using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            CreateMap<Supplier, ReadSupplierResponseDTO>();
            CreateMap<CreateSupplierRequestDTO, Supplier>();
        }
    }
}
