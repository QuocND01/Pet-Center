using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PetCenterAPI.Models.ImportStock;

namespace PetCenterTestProject.ImportStockTest
{
    public class ImportStockTest_ConfirmAsync : IDisposable
    {
        private readonly PetCenterContext _context;
        private readonly ImportStockRepository _repository;
        private readonly IMapper _mapper;
        private readonly ImportStockService _service;

        public ImportStockTest_ConfirmAsync()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;")
                .Options;

            _context = new PetCenterContext(options);

            _repository = new ImportStockRepository(_context);

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImportStockProfile>();   // hoặc MappingProfile của bạn
            }, NullLoggerFactory.Instance)
            .CreateMapper();

            _service = new ImportStockService(
                _repository,
                _context,
                _mapper);

            EnsureSeedData();
        }

        private void EnsureSeedData()
        {
            if (!_context.Staffs.Any(x => x.IsActive))
            {
                var id = Guid.NewGuid();
                _context.Staffs.Add(new Staff
                {
                    StaffId = id,
                    FullName = "Test Staff",
                    PhoneNumber = "0123456789",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    Gender = "Male",
                    HireDate = DateTime.UtcNow,
                    Email = $"confirm-{id:N}@test.com",
                    PasswordHash = "hashed-password",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!_context.Suppliers.Any(x => x.IsActive))
            {
                _context.Suppliers.Add(new Supplier
                {
                    SupplierId = Guid.NewGuid(),
                    SupplierName = "Confirm Test Supplier",
                    SupplierEmail = $"supplier-{Guid.NewGuid():N}@test.com",
                    SupplierPhoneNumber = "0912345678",
                    SupplierAddress = "Can Tho",
                    TaxId = Guid.NewGuid().ToString("N")[..10],
                    IsActive = true
                });
            }

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task UTCID01_ConfirmAsync_PendingImport_ShouldConfirmSuccessfully()
        {
            // ============================
            // Arrange
            // ============================

            var staff = _context.Staffs.First();
            var supplier = _context.Suppliers.First();

            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = $"Test Brand {Guid.NewGuid():N}",
                Status = PetCenterAPI.Common.Status.Active
            };

            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = $"Test Category {Guid.NewGuid():N}",
                Status = PetCenterAPI.Common.Status.Active
            };

            _context.Brands.Add(brand);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Dog Food",
                ProductPrice = 50000,
                BrandId = brand.BrandId,
                CategoryId = category.CategoryId,
                Status = PetCenterAPI.Common.Status.Active
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var import = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = supplier.SupplierId,
                StaffId = staff.StaffId,
                InvoiceNumber = "IMP-001",
                Status = ImportStatus.Pending,
                ImportDate = DateTime.UtcNow,
                TotalAmount = 500000
            };

            var detail = new ImportStockDetail
            {
                ImportStockDetailsId = Guid.NewGuid(),
                ImportId = import.ImportId,
                ProductId = product.ProductId,
                Quantity = 10,
                ImportPrice = 50000,
                SKU = "DOG001",
                BatchCode = "BATCH001",
                StockLeft = 0,
                QuantitySold = 0,
                BatchStatus = BatchStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            import.ImportStockDetails = new List<ImportStockDetail>
    {
        detail
    };

            _context.ImportStocks.Add(import);

            await _context.SaveChangesAsync();

            // ============================
            // Act
            // ============================

            await _service.ConfirmAsync(import.ImportId, staff.StaffId);

            // ============================
            // Assert
            // ============================

            var confirmedImport = await _context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstAsync(x => x.ImportId == import.ImportId);

            Assert.Equal(
                ImportStatus.Confirmed,
                confirmedImport.Status);

            Assert.NotNull(confirmedImport.UpdatedAt);

            var confirmedDetail =
                confirmedImport.ImportStockDetails.First();

            Assert.Equal(10, confirmedDetail.StockLeft);

            Assert.Equal(0, confirmedDetail.QuantitySold);

            Assert.Equal(
                BatchStatus.Active,
                confirmedDetail.BatchStatus);

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x =>
                    x.ProductId == product.ProductId);

            Assert.NotNull(inventory);

            Assert.Equal(
                10,
                inventory.QuantityAvailable);

            Assert.Equal(
                staff.StaffId,
                inventory.UpdatedBy);

            var transaction = await _context.InventoryTransactions
                .FirstOrDefaultAsync(x =>
                    x.ImportStockDetailId ==
                    detail.ImportStockDetailsId);

            Assert.NotNull(transaction);

            Assert.Equal(
                TransactionType.StockIn,
                transaction.TransactionType);

            Assert.Equal(10, transaction.QuantityChange);

            Assert.Equal(0, transaction.QuantityBefore);

            Assert.Equal(10, transaction.QuantityAfter);

            Assert.Equal(
                import.ImportId,
                transaction.ReferenceId);

            // ============================
            // Cleanup
            // ============================

            _context.InventoryTransactions.Remove(transaction);
            _context.Inventories.Remove(inventory);
            _context.ImportStockDetails.Remove(confirmedDetail);
            _context.ImportStocks.Remove(confirmedImport);
            _context.Products.Remove(product);
            _context.Brands.Remove(brand);
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
        }
        [Fact]
        public async Task UTCID02_ConfirmAsync_ImportNotFound_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ConfirmAsync(id, Guid.NewGuid()));

            // Assert
            Assert.Equal("Import not found", ex.Message);
        }
        [Fact]
        public async Task UTCID03_ConfirmAsync_ImportNotPending_ShouldThrowException()
        {
            // Arrange
            var supplier = _context.Suppliers.First();

            var staff = _context.Staffs.First();

            var import = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = supplier.SupplierId,
                StaffId = staff.StaffId,
                InvoiceNumber = "TEST001",
                Status = ImportStatus.Confirmed,
                TotalAmount = 100,
                ImportDate = DateTime.UtcNow
            };

            _context.ImportStocks.Add(import);

            await _context.SaveChangesAsync();

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ConfirmAsync(import.ImportId, staff.StaffId));

            // Assert
            Assert.Equal(
                "Only pending imports can be confirmed",
                ex.Message);

            // Cleanup
            _context.ImportStocks.Remove(import);
            await _context.SaveChangesAsync();
        }
    }
}
