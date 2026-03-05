using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Models;

namespace InventoryAPI.Profiles
{
    public class ImportStockProfile : Profile
    {
        public ImportStockProfile()
        {
            CreateMap<ImportStock, ImportStockDto>()
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportStockDetails));

            CreateMap<ImportStockDetail, ImportStockDetailDto>();

            CreateMap<CreateImportStockDto, ImportStock>();
            CreateMap<CreateImportStockDetailDto, ImportStockDetail>();
        }
    }
}
