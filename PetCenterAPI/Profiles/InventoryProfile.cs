using AutoMapper;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class InventoryProfile : Profile
    {
        public InventoryProfile()
        {
            CreateMap<Inventory, InventoryItemResponseDTO>()

                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product.ProductName))

                .ForMember(d => d.Brand,
                    o => o.MapFrom(s => s.Product.Brand.BrandName))

                .ForMember(d => d.Category,
                    o => o.MapFrom(s => s.Product.Category.CategoryName))

                .ForMember(d => d.ProductImage,
                    o => o.MapFrom(s =>
                        s.Product.ProductImages
                            .Where(i => i.IsActive == true)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()));



            CreateMap<Inventory, InventoryDetailResponseDTO>()

                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product.ProductName))

                .ForMember(d => d.Brand,
                    o => o.MapFrom(s => s.Product.Brand.BrandName))

                .ForMember(d => d.Category,
                    o => o.MapFrom(s => s.Product.Category.CategoryName))

                .ForMember(d => d.ProductImage,
                    o => o.MapFrom(s =>
                        s.Product.ProductImages
                            .Where(i => i.IsActive == true)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()));

                CreateMap<Inventory, InventoryDetailResponseDTO>()

                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product.ProductName))

                .ForMember(d => d.Brand,
                    o => o.MapFrom(s => s.Product.Brand.BrandName))

                .ForMember(d => d.Category,
                    o => o.MapFrom(s => s.Product.Category.CategoryName))

                .ForMember(d => d.ProductImage,
                    o => o.MapFrom(s =>
                        s.Product.ProductImages
                            .Where(i => i.IsActive == true)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()))

                .ForMember(d => d.Batches,
                    o => o.MapFrom(s =>
                        s.Product.ImportStockDetails
                            .OrderBy(x => x.ExpiryDate)))
                .ForMember(dest => dest.Batches,
                    opt => opt.MapFrom(src =>
                        src.Product.ImportStockDetails
                            .Where(x => x.StockLeft > 0)
                            .OrderBy(x => x.ExpiryDate)
                            .ThenBy(x => x.CreatedAt)))
                .ForMember(
                    dest => dest.Transactions,
                    opt => opt.MapFrom(src => src.InventoryTransactions));



            CreateMap<ImportStockDetail, InventoryBatchResponseDTO>();
            CreateMap<InventoryTransaction,InventoryTransactionResponseDTO>();
        }
    }
}