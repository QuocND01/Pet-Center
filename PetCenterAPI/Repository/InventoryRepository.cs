using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly PetCenterContext _db;

        public InventoryRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            // Truy vấn database để lấy ra thông tin kho của sản phẩm
            return await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }

        //Inventory module
        public async Task<(List<Inventory> Items, int TotalRecords)> GetPagedAsync(
            InventoryQueryRequestDTO request)
        {
            var query = _db.Inventories

                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)

                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)

                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductImages)

                .AsNoTracking()
                .AsQueryable();

            #region Search

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();

                query = query.Where(i =>
                    i.Product.ProductName.ToLower().Contains(keyword)
                    || i.SKU.ToLower().Contains(keyword));
            }

            #endregion

            #region Category

            if (request.CategoryId.HasValue)
            {
                query = query.Where(i =>
                    i.Product.CategoryId == request.CategoryId.Value);
            }

            #endregion

            #region Brand

            if (request.BrandId.HasValue)
            {
                query = query.Where(i =>
                    i.Product.BrandId == request.BrandId.Value);
            }

            #endregion
            //Check if the request has LowStock or OutOfStock filter, if both are true, return all items
            #region Low Stock

            if (request.LowStock == true)
            {
                query = query.Where(i =>
                    i.QuantityAvailable > 0 &&
                    i.QuantityAvailable <= 10);
            }

            #endregion

            #region Out Of Stock

            if (request.OutOfStock == true)
            {
                query = query.Where(i =>
                    i.QuantityAvailable == 0);
            }

            #endregion

            var totalRecords = await query.CountAsync();

            var items = await query

                .OrderBy(i => i.Product.ProductName)

                .Skip((request.Page - 1) * request.PageSize)

                .Take(request.PageSize)

                .ToListAsync();

            return (items, totalRecords);
        }

        public async Task<Inventory?> GetByIdAsync(Guid inventoryId)
        {
            return await _db.Inventories

                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)

                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)

                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductImages)

                .Include(i => i.Product)
                    .ThenInclude(p => p.ImportStockDetails)
                .Include(i => i.InventoryTransactions)


                .AsNoTracking()


                .FirstOrDefaultAsync(i =>
                    i.InventoryId == inventoryId);
        }
        

        public async Task<List<ImportStockDetail>>
    GetAvailableBatchesByProductIdAsync(Guid productId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            return await _db.ImportStockDetails
                .Where(x =>
                    x.ProductId == productId &&
                    x.StockLeft > 0 &&
                    x.BatchStatus == BatchStatus.Active &&
                    (
                        x.ExpiryDate == null ||
                        x.ExpiryDate > today
                    ))
                .OrderBy(x => x.ExpiryDate.HasValue ? 0 : 1)
                .ThenBy(x => x.ExpiryDate)
                .ThenBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetInventoriesByProductIdsAsync(List<Guid> productIds)
        {
            return await _db.Inventories
                .Include(inv => inv.Product)
                .Where(inv => productIds.Contains(inv.ProductId))
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryReportDto>> GetInventorySummaryReportAsync()
        {
            return await _db.Inventories
                .AsNoTracking()
                .Select(i => new InventoryReportDto
                {
                    SKU = i.SKU,
                    ProductName = i.Product.ProductName,
                    QuantityAvailable = i.QuantityAvailable,
                    QuantityReserved = i.QuantityReserved,
                    AvgImportPrice = _db.ImportStockDetails
                        .Where(d => d.ProductId == i.ProductId)
                        .Select(d => (decimal?)d.ImportPrice)
                        .Average() ?? 0,
                    LastUpdated = i.LastUpdated
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BatchExpiryReportDto>> GetBatchExpiryReportAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return await _db.ImportStockDetails
                .AsNoTracking()
                .Include(d => d.Import)
                .Include(d => d.Product)
                .Where(d => d.StockLeft > 0) // Chỉ lấy các lô còn tồn
                .Select(d => new BatchExpiryReportDto
                {
                    BatchCode = d.BatchCode,
                    InvoiceNumber = d.Import.InvoiceNumber,
                    SKU = d.SKU,
                    ProductName = d.Product.ProductName,
                    ImportPrice = d.ImportPrice,
                    StockLeft = d.StockLeft,
                    ManufacturingDate = d.ManufacturingDate,
                    ExpiryDate = d.ExpiryDate,
                    Status = d.BatchStatus.ToString(),
                    ExpiryAlert = d.ExpiryDate.HasValue
                        ? (d.ExpiryDate.Value < today
                            ? "ĐÃ HẾT HẠN"
                            : (d.ExpiryDate.Value.DayNumber - today.DayNumber <= 30
                                ? "SẮP HẾT HẠN"
                                : "BÌNH THƯỜNG"))
                        : "KHÔNG CÓ HSD"
                })
                .ToListAsync();
        }
        public async Task<int> UpdateExpiredBatchesAsync(CancellationToken cancellationToken = default)
        {
            var todayUtc = DateOnly.FromDateTime(DateTime.Now);

            // 1. Lấy danh sách các lô hàng đã hết hạn
            var expiredBatches = await _db.ImportStockDetails
                .Where(b => b.BatchStatus == BatchStatus.Active
                         && b.ExpiryDate.HasValue
                         && b.ExpiryDate.Value < todayUtc)
                .ToListAsync(cancellationToken);

            if (!expiredBatches.Any()) return 0;

            // 2. Gom nhóm số lượng hết hạn theo ProductId (hoặc SKU) để trừ kho 1 lần
            // Giả sử ImportStockDetail có thuộc tính ProductId và Quantity (hoặc RemainingQuantity)
            var expiredQuantitiesByProduct = expiredBatches
                .GroupBy(b => b.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalExpiredQuantity = g.Sum(b => b.Quantity) // Thay b.Quantity bằng trường chứa số lượng lô của bạn
                })
                .ToList();

            // 3. Cập nhật trạng thái từng lô hàng thành Expired
            foreach (var batch in expiredBatches)
            {
                batch.BatchStatus = BatchStatus.Expired;
            }

            // 4. Lấy các bản ghi Inventory tương ứng để trừ QuantityAvailable
            var productIds = expiredQuantitiesByProduct.Select(p => p.ProductId).ToList();
            var inventories = await _db.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync(cancellationToken);

            foreach (var invItem in expiredQuantitiesByProduct)
            {
                var inventory = inventories.FirstOrDefault(i => i.ProductId == invItem.ProductId);
                if (inventory != null)
                {
                    // Trừ số lượng khả dụng (đảm bảo không âm nếu có sai số)
                    inventory.QuantityAvailable = Math.Max(0, inventory.QuantityAvailable - invItem.TotalExpiredQuantity);
                    inventory.LastUpdated = DateTime.Now;
                }
            }

            // 5. Lưu tất cả thay đổi vào Database trong 1 Transaction duy nhất
            return await _db.SaveChangesAsync(cancellationToken);
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}