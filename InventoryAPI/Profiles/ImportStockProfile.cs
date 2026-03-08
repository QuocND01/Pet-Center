using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Models;

namespace InventoryAPI.Profiles
{
    public class ImportStockProfile : Profile
    {
        public ImportStockProfile()
        {
            // Read full import
            CreateMap<ImportStock, ReadImportStockDto>()
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportStockDetails));

            // Header list
            CreateMap<ImportStock, ReadImportHeaderDto>();

            // Detail read
            CreateMap<ImportStockDetail, ImportStockDetailDto>();

            // Create import
            CreateMap<CreateImportStockDto, ImportStock>()
                .ForMember(dest => dest.ImportId, opt => opt.Ignore())
                .ForMember(dest => dest.ImportDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ImportStockDetails,
                           opt => opt.MapFrom(src => src.Details));

            // Create detail
            CreateMap<CreateImportStockDetailDto, ImportStockDetail>()
                .ForMember(dest => dest.ImportStockDetailId,
                           opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.StockLeft,
                           opt => opt.MapFrom(src => src.Quantity));
        }
    }
}
