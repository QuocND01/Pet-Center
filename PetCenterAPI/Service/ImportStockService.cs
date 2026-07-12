using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Requests.Import;
using PetCenterAPI.DTOs.Responses.Import;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.Models.ImportStock;

namespace PetCenterAPI.Service
{
    public class ImportStockService : IImportStockService
    {
        private readonly IImportStockRepository _repo;
        private readonly PetCenterContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        public ImportStockService(
            IImportStockRepository repo,
            PetCenterContext context,
            IMapper mapper,
            IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _context = context;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Guid> CreateAsync(
    CreateImportRequestDTO dto,
    Guid staffGuid)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            //Get product details 
            var productIds = dto.Details
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var importStock =
                _mapper.Map<ImportStock>(dto);

            importStock.ImportId = Guid.NewGuid();

            importStock.ImportDate = DateTime.UtcNow;

            importStock.Status = ImportStatus.Pending;

            importStock.StaffId = staffGuid;

            importStock.TotalAmount =
                importStock.ImportStockDetails
                    .Sum(x => x.Quantity * x.ImportPrice);
            foreach (var detail in importStock.ImportStockDetails)
            {
                var product = products[detail.ProductId];

                detail.ImportProductSnapshot = new ImportProductSnapshot
                {
                    ProductSnapshotId = Guid.NewGuid(),

                    ProductName = product.ProductName,

                    ProductBrand = product.Brand.BrandName,

                    ProductCategory = product.Category.CategoryName,

                    ProductImage = product.ProductImages
                        .FirstOrDefault(i => i.IsActive == true)?
                        .ImageUrl ?? string.Empty
                };
            }

            await _repo.AddAsync(importStock);

            await _repo.SaveChangesAsync();

            await transaction.CommitAsync();

            return importStock.ImportId;
        }

        public async Task<ReadImportResponseDTO?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetWithDetailsAsync(id);

            if (entity == null)
                return null;

            return _mapper.Map<ReadImportResponseDTO>(entity);
        }

        public async Task ConfirmAsync(Guid importId, Guid staffId)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var importStock = await _repo.GetWithDetailsAsync(importId);

            if (importStock == null)
                throw new Exception("Import not found");

            if (importStock.Status != ImportStatus.Pending)
                throw new Exception("Only pending imports can be confirmed");

            // Lấy toàn bộ ProductId cần xử lý
            var productIds = importStock.ImportStockDetails
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            // Load inventory một lần để tránh N+1 query
            var inventories = await _context.Inventories
                .Where(x => productIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId);

            foreach (var detail in importStock.ImportStockDetails)
            {
                // Draft batch -> batch thật
                detail.StockLeft = detail.Quantity;
                detail.QuantitySold = 0;
                detail.BatchStatus = BatchStatus.Active;
                detail.CreatedAt = DateTime.UtcNow;

                // Tìm inventory theo Product
                if (!inventories.TryGetValue(detail.ProductId, out var inventory))
                {
                    inventory = new Inventory
                    {
                        InventoryId = Guid.NewGuid(),
                        ProductId = detail.ProductId,
                        SKU = detail.SKU,

                        QuantityAvailable = 0,
                        QuantityReserved = 0,
                        QuantityDamaged = 0,

                        LastUpdated = DateTime.UtcNow,
                        UpdatedBy = staffId
                    };

                    inventories.Add(detail.ProductId, inventory);
                    _context.Inventories.Add(inventory);
                }

                var quantityBefore = inventory.QuantityAvailable;

                // Tăng tồn kho tổng
                inventory.QuantityAvailable += detail.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                inventory.UpdatedBy = staffId;

                // Audit transaction theo từng batch
                _context.InventoryTransactions.Add(
                    new InventoryTransaction
                    {
                        TransactionId = Guid.NewGuid(),

                        InventoryId = inventory.InventoryId,

                        QuantityChange = detail.Quantity,
                        QuantityBefore = quantityBefore,
                        QuantityAfter = inventory.QuantityAvailable,

                        TransactionType = TransactionType.StockIn,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = staffId,

                        ReferenceId = importStock.ImportId,
                        ReferenceType = ReferenceType.ImportStock,

                        ImportStockDetailId = detail.ImportStockDetailsId,

                        Note =
                            $"Import {importStock.InvoiceNumber} - Batch {detail.BatchCode}"
                    });
            }

            importStock.Status = ImportStock.ImportStatus.Confirmed;
            importStock.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task CancelAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = await _repo.GetByIdAsync(id);

            if (importStock == null)
                throw new Exception("Import not found");

            if (importStock.Status != ImportStatus.Pending)
                throw new Exception("Only pending import can be cancelled");

            importStock.Status = ImportStatus.Cancelled;

            await _repo.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<List<ReadImportHeaderResponseDTO>> GetAllImportsAsync()
        {
            var imports = await _repo.GetAllAsync();

            return _mapper.Map<List<ReadImportHeaderResponseDTO>>(imports);
        }
        public async Task<ExportResponseDTO> Export(DateTime? fromDate, DateTime? toDate)
        {
            var (imports, details) = await _repo.GetExportData(fromDate, toDate);

            return new ExportResponseDTO
            {
                Imports = _mapper.Map<List<ReadImportHeaderResponseDTO>>(imports),
                Details = _mapper.Map<List<ReadImportDetailResponseDTO>>(details)
            };
        }
        


        public async Task<bool> HasProductInImportsAsync(Guid productId)
        {
            return await _repo
                .CheckProductInImportsAsync(productId);
        }
    }
}