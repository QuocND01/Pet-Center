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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

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
    }
}