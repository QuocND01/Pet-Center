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

        public async Task ConfirmAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var importStock = await _repo.GetWithDetailsAsync(id);

            if (importStock == null)
                throw new Exception("Import not found");

            if (importStock.Status != ImportStatus.Pending)
                throw new Exception("Only pending import can be confirmed");

            // Nếu muốn update stock product
            // foreach (var detail in importStock.ImportStockDetails)
            // {
            //     var product = await _context.Products
            //         .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);
            //
            //     if (product == null)
            //         throw new Exception("Product not found");
            //
            //     product.StockQuantity += detail.Quantity;
            // }

            importStock.Status = ImportStatus.Confirmed;

            await _repo.SaveChangesAsync();
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

        public async Task<List<ReadImportHeaderDto>> GetAllImportsAsync()
        {
            var imports = await _repo.GetAllAsync();

            return _mapper.Map<List<ReadImportHeaderDto>>(imports);
        }
    }
}