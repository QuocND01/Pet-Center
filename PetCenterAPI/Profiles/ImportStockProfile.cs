using AutoMapper;
using PetCenterAPI.DTOs.Requests.Import;
using PetCenterAPI.DTOs.Responses.Import;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class ImportStockProfile : Profile
    {
        public ImportStockProfile()
        {
            // Read full import (Header + Details)
            CreateMap<ImportStock, ReadImportResponseDTO>()
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.SupplierName))
                .ForMember(dest => dest.Details,
                    opt => opt.MapFrom(src => src.ImportStockDetails));

            // Header list
            CreateMap<ImportStock, ReadImportHeaderResponseDTO>()
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier.SupplierName));

            // Detail read
            CreateMap<ImportStockDetail, ReadImportDetailResponseDTO>();

            // Map snapshot to dto 
            CreateMap<ImportProductSnapshot, ProductSnapshotResponseDTO>();

            // Create import
            CreateMap<CreateImportRequestDTO, ImportStock>()
                .ForMember(dest => dest.ImportId, opt => opt.Ignore())
                .ForMember(dest => dest.ImportDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ImportStockDetails,
                    opt => opt.MapFrom(src => src.Details));

            // Create detail
            CreateMap<CreateImportDetailRequestDTO, ImportStockDetail>()
                .ForMember(dest => dest.ImportStockDetailsId,
                    opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.StockLeft,
                    opt => opt.MapFrom(src => src.Quantity));

            CreateMap<ProductSnapshotRequestDTO, ImportProductSnapshot>()
                .ForMember(dest => dest.ProductSnapshotId,
                    opt => opt.MapFrom(src => src.ProductId))

                .ForMember(dest => dest.ImportStockDetailsId,
                    opt => opt.Ignore());

            
            
        }
    }
}