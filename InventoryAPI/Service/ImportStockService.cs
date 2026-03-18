using AutoMapper;
using InventoryAPI.Data;
using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service.Interface;
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

        public async Task<Guid> CreateAsync(CreateImportStockDto dto, Guid staffGuid)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = _mapper.Map<ImportStock>(dto);

            importStock.ImportId = Guid.NewGuid();
            importStock.ImportDate = DateTime.UtcNow;
            importStock.Status = ImportStatus.Pending;
            importStock.StaffId = staffGuid;

            importStock.TotalAmount = importStock.ImportStockDetails
                .Sum(x => x.Quantity * x.ImportPrice);

            await _repo.AddAsync(importStock);
            await _repo.SaveChangesAsync();

            await transaction.CommitAsync();

            return importStock.ImportId;
        }

        public async Task<ReadImportStockDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetWithDetailsAsync(id);

            if (entity == null)
                return null;

            return _mapper.Map<ReadImportStockDto>(entity);
        }

        public async Task<List<IncreaseStockItemDto>> ConfirmAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = await _repo.GetWithDetailsAsync(id);

            if (importStock == null)
                throw new Exception("Import not found");

            if (importStock.Status != ImportStatus.Pending)
                throw new Exception("Only pending import can be confirmed");

            // 🔥 map data TRƯỚC khi save
            var items = importStock.ImportStockDetails
                .Where(x => x.ProductId.HasValue)
                .GroupBy(x => x.ProductId!.Value)
                .Select(g => new IncreaseStockItemDto
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // 🔥 update status
            importStock.Status = ImportStatus.Confirmed;

            await _repo.SaveChangesAsync();
            await transaction.CommitAsync();

            // 🔥 RETURN cho controller
            return items;
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

        public async Task<List<ReadImportHeaderDto>> GetAllImportsAsync()
        {
            var imports = await _repo.GetAllAsync();

            return _mapper.Map<List<ReadImportHeaderDto>>(imports);
        }
        public async Task<ImportExportResponseDto> Export(DateTime? fromDate, DateTime? toDate)
        {
            var (imports, details) = await _repo.GetExportData(fromDate, toDate);

            return new ImportExportResponseDto
            {
                Imports = _mapper.Map<List<ReadImportHeaderDto>>(imports),
                Details = _mapper.Map<List<ImportStockDetailDto>>(details)
            };
        }
        // Trừ kho FIFO
        public async Task<string> DeductFIFO(Guid productId, int quantity)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            var stocks = await _repo.GetAvailableStock(productId);

            int remain = quantity;
            var map = new List<string>();

            foreach (var s in stocks)
            {
                if (remain <= 0) break;

                int take = Math.Min(s.StockLeft, remain);

                s.StockLeft -= take;
                remain -= take;

                map.Add($"{s.ImportStockDetailId}:{take}");
            }

            if (remain > 0)
                throw new Exception("Not enough stock");

            await _repo.SaveChangesAsync();
            await tran.CommitAsync();

            return string.Join(",", map);
        }

        //  Trả hàng
        public async Task ReturnStock(string mapping)
        {
            if (string.IsNullOrEmpty(mapping)) return;

            using var tran = await _context.Database.BeginTransactionAsync();

            var items = mapping.Split(',');

            foreach (var item in items)
            {
                var parts = item.Split(':');

                var id = Guid.Parse(parts[0]);
                var qty = int.Parse(parts[1]);

                var stock = await _repo.GetById(id);

                if (stock != null)
                    stock.StockLeft += qty;
            }

            await _repo.SaveChangesAsync();
            await tran.CommitAsync();
        }
    }
}