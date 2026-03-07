using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service.Interface;
using InventoryAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using static InventoryAPI.Models.ImportStock;

namespace InventoryAPI.Service
{
    public class ImportStockService : IImportStockService
    {
        private readonly IImportStockRepository _repo;
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;

        public ImportStockService(
            IImportStockRepository repo,
            InventoryContext context,
            IMapper mapper)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> CreateAsync(CreateImportStockDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = dto.SupplierId,
                StaffId = dto.StaffId,
                ImportDate = DateTime.UtcNow,
                Status  = ImportStatus.Draft,
            };

            decimal total = 0;

            foreach (var item in dto.Details)
            {
                var detail = new ImportStockDetail
                {
                    ImportStockDetailId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ImportPrice = item.ImportPrice,
                    StockLeft = item.Quantity
                };

                total += item.Quantity * item.ImportPrice;
                importStock.ImportStockDetails.Add(detail);
            }

            importStock.TotalAmount = total;

            await _repo.AddAsync(importStock);
            await _repo.SaveChangesAsync();

            await transaction.CommitAsync();

            return importStock.ImportId;
        }

        public async Task<ImportStockDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ImportStockDto>(entity);
        }

        public async Task ConfirmAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = await _context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstOrDefaultAsync(x => x.ImportId == id);

            if (importStock == null)
                throw new Exception("Import not found");

            // Increae stock quantity of each product in details
            // ❗ Chỉ cho confirm khi đang Draft


            if (importStock.Status != ImportStatus.Draft)
                throw new Exception("Only draft import can be confirmed");

            //foreach (var detail in importStock.ImportStockDetails)
            //{
            //    var product = await _context.Products
            //        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

            //    if (product == null)
            //        throw new Exception("Product not found");

            //    product.StockQuantity += detail.Quantity;
            //}

            // ✅ Set status = Confirmed (1)
            importStock.Status = ImportStatus.Confirmed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        public async Task CancelAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = await _context.ImportStocks
                .FirstOrDefaultAsync(x => x.ImportId == id);

            if (importStock == null)
                throw new Exception("Import not found");

            // ❗ Chỉ cho cancel khi đang Draft
            if (importStock.Status != ImportStatus.Draft)
                throw new Exception("Only draft import can be cancelled");

            importStock.Status = ImportStatus.Cancelled;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
