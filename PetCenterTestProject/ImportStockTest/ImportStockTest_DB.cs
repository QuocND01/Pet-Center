using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.DTOs.Requests.Import;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PetCenterAPI.Models.ImportStock;

namespace PetCenterTestProject.ImportStockTest
{
    public class ImportStockTest_DB
    {
        private readonly IMapper _mapper;

        //=========================================================
        // Constructor
        //=========================================================

        public ImportStockTest_DB()
        {

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ImportStockProfile>();   // hoặc MappingProfile của bạn
            }, NullLoggerFactory.Instance)
            .CreateMapper();
        }

        //=========================================================
        // Create SQL Server Context
        //=========================================================

        private PetCenterContext CreateContext()
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

            var context = new PetCenterContext(options);
            EnsureSeedData(context);
            return context;
        }

        private static Staff BuildStaff(string prefix = "import-stock")
        {
            var id = Guid.NewGuid();
            return new Staff
            {
                StaffId = id,
                FullName = "Test Staff",
                PhoneNumber = "0123456789",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Gender = "Male",
                HireDate = DateTime.UtcNow,
                Email = $"{prefix}-{id:N}@test.com",
                PasswordHash = "hashed-password",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void EnsureSeedData(PetCenterContext context)
        {
            var staff = context.Staffs.FirstOrDefault(x => x.IsActive);
            if (staff == null)
            {
                staff = BuildStaff();
                context.Staffs.Add(staff);
            }

            var supplier = context.Suppliers.FirstOrDefault(x => x.IsActive);
            if (supplier == null)
            {
                supplier = new Supplier
                {
                    SupplierId = Guid.NewGuid(),
                    SupplierName = "Import Test Supplier",
                    SupplierEmail = $"supplier-{Guid.NewGuid():N}@test.com",
                    SupplierPhoneNumber = "0912345678",
                    SupplierAddress = "Can Tho",
                    TaxId = Guid.NewGuid().ToString("N")[..10],
                    IsActive = true
                };
                context.Suppliers.Add(supplier);
            }

            if (!context.Products.Any())
            {
                var brand = new Brand
                {
                    BrandId = Guid.NewGuid(),
                    BrandName = $"Import Test Brand {Guid.NewGuid():N}",
                    Status = PetCenterAPI.Common.Status.Active
                };

                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = $"Import Test Category {Guid.NewGuid():N}",
                    Status = PetCenterAPI.Common.Status.Active
                };

                var product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Import Test Product",
                    ProductPrice = 100000,
                    BrandId = brand.BrandId,
                    CategoryId = category.CategoryId,
                    Status = PetCenterAPI.Common.Status.Active
                };

                var secondProduct = new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Import Test Product 2",
                    ProductPrice = 150000,
                    BrandId = brand.BrandId,
                    CategoryId = category.CategoryId,
                    Status = PetCenterAPI.Common.Status.Active
                };

                context.Brands.Add(brand);
                context.Categories.Add(category);
                context.Products.Add(product);
                context.Products.Add(secondProduct);
                context.ProductImages.Add(new ProductImage
                {
                    ImageId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    ImageUrl = "https://example.com/import-product.jpg",
                    IsActive = true
                });
                context.ProductImages.Add(new ProductImage
                {
                    ImageId = Guid.NewGuid(),
                    ProductId = secondProduct.ProductId,
                    ImageUrl = "https://example.com/import-product-2.jpg",
                    IsActive = true
                });
            }

            context.SaveChanges();

            if (context.Products.Count() < 2)
            {
                var brand = new Brand
                {
                    BrandId = Guid.NewGuid(),
                    BrandName = $"Import Extra Brand {Guid.NewGuid():N}",
                    Status = PetCenterAPI.Common.Status.Active
                };

                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = $"Import Extra Category {Guid.NewGuid():N}",
                    Status = PetCenterAPI.Common.Status.Active
                };

                var product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Import Extra Product",
                    ProductPrice = 150000,
                    BrandId = brand.BrandId,
                    CategoryId = category.CategoryId,
                    Status = PetCenterAPI.Common.Status.Active
                };

                context.Brands.Add(brand);
                context.Categories.Add(category);
                context.Products.Add(product);
                context.ProductImages.Add(new ProductImage
                {
                    ImageId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    ImageUrl = "https://example.com/import-extra-product.jpg",
                    IsActive = true
                });
                context.SaveChanges();
            }

            if (!context.ImportStocks.Any())
            {
                var product = context.Products.First();
                var pending = new ImportStock
                {
                    ImportId = Guid.NewGuid(),
                    SupplierId = supplier.SupplierId,
                    StaffId = staff.StaffId,
                    InvoiceNumber = $"IMP-SEED-{Guid.NewGuid():N}"[..20],
                    Status = ImportStatus.Pending,
                    ImportDate = DateTime.UtcNow,
                    TotalAmount = 100000
                };

                context.ImportStocks.Add(pending);
                context.ImportStockDetails.Add(new ImportStockDetail
                {
                    ImportStockDetailsId = Guid.NewGuid(),
                    ImportId = pending.ImportId,
                    ProductId = product.ProductId,
                    Quantity = 2,
                    ImportPrice = 50000,
                    SKU = $"SKU-{Guid.NewGuid():N}"[..12],
                    BatchCode = $"BATCH-{Guid.NewGuid():N}"[..16],
                    StockLeft = 0,
                    QuantitySold = 0,
                    BatchStatus = BatchStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });

                context.ImportStocks.Add(new ImportStock
                {
                    ImportId = Guid.NewGuid(),
                    SupplierId = supplier.SupplierId,
                    StaffId = staff.StaffId,
                    InvoiceNumber = $"IMP-CONF-{Guid.NewGuid():N}"[..20],
                    Status = ImportStatus.Confirmed,
                    ImportDate = DateTime.UtcNow,
                    TotalAmount = 50000
                });

                context.SaveChanges();
            }
        }

        [Fact]
        public async Task ITCID01_GetAllImportsAsync_ShouldReturnImportList()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper
                );

            // Act
            var result = await service.GetAllImportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        [Fact]
        public async Task ITCID03_GetAllImportsAsync_ShouldMapStatusCorrectly()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var result = await service.GetAllImportsAsync();

            Assert.All(result, x =>
            {
                Assert.True(
                    Enum.IsDefined(typeof(ImportStatus), x.Status));
            });
        }
        [Fact]
        public async Task ITCID04_DatabaseConnection_ShouldBeAvailable()
        {
            using var context = CreateContext();

            var canConnect = await context.Database.CanConnectAsync();

            Assert.True(canConnect);
        }
        [Fact]
        public async Task ITCID01_GetByIdAsync_ShouldReturnImport()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var entity = context.ImportStocks.First();

            var result = await service.GetByIdAsync(entity.ImportId);

            Assert.NotNull(result);
            Assert.Equal(entity.ImportId, result!.ImportId);
        }
        [Fact]
        public async Task ITCID02_GetByIdAsync_NotFound_ShouldReturnNull()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
        [Fact]
        public async Task ITCID03_GetByIdAsync_ShouldLoadImportDetails()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var entity = context.ImportStocks
                .First(x => x.ImportStockDetails.Any());

            var result = await service.GetByIdAsync(entity.ImportId);

            Assert.NotNull(result);
            Assert.NotEmpty(result!.Details);
        }
        [Fact]
        public async Task ITCID04_GetByIdAsync_ShouldMapOptionalFields()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var entity = context.ImportStocks.First();

            var result = await service.GetByIdAsync(entity.ImportId);

            Assert.NotNull(result);

            Assert.Equal(entity.ImportDate, result!.ImportDate);
            Assert.Equal(entity.Note, result.Note);
        }
        [Fact]
        public async Task UTCID01_CreateAsync_ValidImport_ShouldCreateSuccessfully()
        {
            using var context = CreateContext();

            var repo = new ImportStockRepository(context);

            var service = new ImportStockService(
                repo,
                context,
                _mapper);

            // 1. LẤY MỘT NHÂN VIÊN HỢP LỆ ĐANG CÓ TRONG CƠ SỞ DỮ LIỆU
            var staff = context.Staffs.FirstOrDefault(s => s.IsActive);
            if (staff == null)
            {
                // Dự phòng trường hợp DB test chưa có nhân viên nào, ta tạo tạm 1 người
                staff = BuildStaff("import-create");
                context.Staffs.Add(staff);
                await context.SaveChangesAsync();
            }

            var supplier = context.Suppliers.First(s => s.IsActive);

            var product = context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .First();

            var dto = new CreateImportRequestDTO
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = Guid.NewGuid().ToString(),
                Details =
                [
                    new()
            {
                ProductId = product.ProductId,
                Quantity = 5,
                ImportPrice = 100,
                SKU = "SKU001",
                BatchCode = "B001"
            }
                ]
            };

            // 2. THAY ĐỔI TẠI ĐÂY: Truyền staff.StaffID hợp lệ thay vì Guid.NewGuid()
            var id = await service.CreateAsync(dto, staff.StaffId);

            var created = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .ThenInclude(x => x.ImportProductSnapshot)
                .FirstOrDefaultAsync(x => x.ImportId == id);

            Assert.NotNull(created);

            Assert.Equal(500, created.TotalAmount);

            Assert.Equal(ImportStatus.Pending, created.Status);

            Assert.Single(created.ImportStockDetails);

            Assert.NotNull(created.ImportStockDetails.First().ImportProductSnapshot);
        }

        [Fact]
        public async Task UTCID02_CreateAsync_ProductNotExist_ShouldThrow()
        {
            using var context = CreateContext();

            var repo = new ImportStockRepository(context);

            var service = new ImportStockService(
                repo,
                context,
                _mapper);

            var supplier = context.Suppliers.First();

            var dto = new CreateImportRequestDTO
            {
                SupplierId = supplier.SupplierId,

                Details =
                [
                    new()
            {
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                ImportPrice = 100
            }
                ]
            };

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.CreateAsync(dto, Guid.NewGuid()));
        }
        [Fact]
        public async Task UTCID04_CreateAsync_NullOptionalFields_ShouldCreateSuccessfully()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            // 1. LẤY NHÂN VIÊN HỢP LỆ ĐANG CÓ TRONG CƠ SỞ DỮ LIỆU
            var staff = await context.Staffs.FirstOrDefaultAsync(s => s.IsActive);
            if (staff == null)
            {
                staff = BuildStaff("import-null-optional");
                context.Staffs.Add(staff);
                await context.SaveChangesAsync();
            }

            var supplier = await context.Suppliers
                .FirstAsync(x => x.IsActive);

            var product = await context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .FirstAsync();

            var dto = new CreateImportRequestDTO
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = $"UT-{Guid.NewGuid()}",
                Note = null,
                Details =
                [
                    new()
            {
                ProductId = product.ProductId,
                Quantity = 2,
                ImportPrice = 100000,
                SKU = "SKU001",
                BatchCode = Guid.NewGuid().ToString("N")[..8],
                ManufacturingDate = null,
                ExpiryDate = null
            }
                ]
            };

            // 2. THAY ĐỔI TẠI ĐÂY: Truyền staff.StaffID hợp lệ thay vì Guid.NewGuid()
            var importId = await service.CreateAsync(dto, staff.StaffId);

            var created = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstAsync(x => x.ImportId == importId);

            Assert.NotNull(created);

            Assert.Null(created.Note);

            var detail = created.ImportStockDetails.First();

            Assert.Null(detail.ManufacturingDate);
            Assert.Null(detail.ExpiryDate);
        }

        [Fact]
        public async Task UTCID05_CreateAsync_ProductWithoutActiveImage_ShouldCreateEmptySnapshotImage()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            // 1. LẤY NHÂN VIÊN HỢP LỆ ĐANG CÓ TRONG CƠ SỞ DỮ LIỆU
            var staff = await context.Staffs.FirstOrDefaultAsync(s => s.IsActive);
            if (staff == null)
            {
                staff = BuildStaff("import-no-image");
                context.Staffs.Add(staff);
                await context.SaveChangesAsync();
            }

            var supplier = await context.Suppliers
                .FirstAsync(x => x.IsActive);

            var product = await context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .FirstAsync();

            // Disable all active images
            foreach (var image in product.ProductImages)
            {
                image.IsActive = false;
            }

            await context.SaveChangesAsync();

            var dto = new CreateImportRequestDTO
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = $"UT-{Guid.NewGuid()}",
                Details =
                [
                    new()
            {
                ProductId = product.ProductId,
                Quantity = 1,
                ImportPrice = 100000,
                SKU = "SKU001",
                BatchCode = Guid.NewGuid().ToString("N")[..8]
            }
                ]
            };

            // 2. THAY ĐỔI TẠI ĐÂY: Truyền staff.StaffID hợp lệ thay vì Guid.NewGuid()
            var importId = await service.CreateAsync(dto, staff.StaffId);

            var created = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .ThenInclude(x => x.ImportProductSnapshot)
                .FirstAsync(x => x.ImportId == importId);

            Assert.Equal(
                string.Empty,
                created.ImportStockDetails.First()
                    .ImportProductSnapshot.ProductImage);
        }

        [Fact]
        public async Task UTCID06_CreateAsync_MultipleDetailItems_ShouldCalculateTotalCorrectly()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            // 1. LẤY NHÂN VIÊN HỢP LỆ TỪ DATABASE (TRÁNH LỖI KHÓA NGOẠI)
            var staff = await context.Staffs.FirstOrDefaultAsync(s => s.IsActive);
            if (staff == null)
            {
                staff = BuildStaff("import-multiple");
                context.Staffs.Add(staff);
                await context.SaveChangesAsync();
            }

            var supplier = await context.Suppliers
                .FirstAsync(x => x.IsActive);

            var products = await context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Take(2)
                .ToListAsync();

            Assert.True(products.Count >= 2,
                "Database must contain at least two products.");

            var dto = new CreateImportRequestDTO
            {
                SupplierId = supplier.SupplierId,
                InvoiceNumber = $"UT-{Guid.NewGuid()}",
                Details =
                [
                    new()
            {
                ProductId = products[0].ProductId,
                Quantity = 2,
                ImportPrice = 100000,
                SKU = "SKU001",
                BatchCode = Guid.NewGuid().ToString("N")[..8]
            },
            new()
            {
                ProductId = products[1].ProductId,
                Quantity = 3,
                ImportPrice = 200000,
                SKU = "SKU002",
                BatchCode = Guid.NewGuid().ToString("N")[..8]
            }
                ]
            };

            // 2. THAY ĐỔI TẠI ĐÂY: Sử dụng staff.StaffID thay vì Guid.NewGuid()
            var importId = await service.CreateAsync(dto, staff.StaffId);

            var created = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .ThenInclude(x => x.ImportProductSnapshot)
                .FirstAsync(x => x.ImportId == importId);

            Assert.Equal(800000, created.TotalAmount);

            Assert.Equal(2, created.ImportStockDetails.Count);

            Assert.All(
                created.ImportStockDetails,
                detail => Assert.NotNull(detail.ImportProductSnapshot));
        }
        [Fact]
        public async Task UTCID01_ConfirmAsync_PendingImport_ShouldConfirmSuccessfully()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var staffId = Guid.NewGuid();

            var import = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstAsync(x =>
                    x.Status == ImportStatus.Pending &&
                    x.ImportStockDetails.Any());

            var quantitiesBefore = await context.Inventories
                .ToDictionaryAsync(x => x.ProductId, x => x.QuantityAvailable);

            await service.ConfirmAsync(import.ImportId, staffId);

            var updated = await context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstAsync(x => x.ImportId == import.ImportId);

            Assert.Equal(ImportStatus.Confirmed, updated.Status);

            foreach (var detail in updated.ImportStockDetails)
            {
                Assert.Equal(detail.Quantity, detail.StockLeft);
                Assert.Equal(0, detail.QuantitySold);
                Assert.Equal(BatchStatus.Active, detail.BatchStatus);

                var inventory = await context.Inventories
                    .FirstAsync(x => x.ProductId == detail.ProductId);

                var before = quantitiesBefore.ContainsKey(detail.ProductId)
                    ? quantitiesBefore[detail.ProductId]
                    : 0;

                Assert.Equal(before + detail.Quantity,
                    inventory.QuantityAvailable);

                Assert.True(await context.InventoryTransactions.AnyAsync(
                    x => x.ImportStockDetailId == detail.ImportStockDetailsId));
            }

        }
        
        [Fact]
        public async Task UTCID02_ConfirmAsync_ImportNotFound_ShouldThrow()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var id = Guid.NewGuid();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ConfirmAsync(id, Guid.NewGuid()));

            Assert.Equal("Import not found", ex.Message);
        }
        [Fact]
        public async Task UTCID03_ConfirmAsync_NotPending_ShouldThrow()
        {
            using var context = CreateContext();

            var repository = new ImportStockRepository(context);

            var service = new ImportStockService(
                repository,
                context,
                _mapper);

            var import = await context.ImportStocks
                .FirstAsync(x => x.Status == ImportStatus.Confirmed);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ConfirmAsync(import.ImportId, Guid.NewGuid()));

            Assert.Equal(
                "Only pending imports can be confirmed",
                ex.Message);
        }
    }
}
