using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Models;

namespace InventoryAPI.Profiles
{
    public class InventoryProfile : Profile
    {
        public InventoryProfile()
        {
            CreateMap<Inventory, ProductInventoryDTO>();
        }
    }
}
