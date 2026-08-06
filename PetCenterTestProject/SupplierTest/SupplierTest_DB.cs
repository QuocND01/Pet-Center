using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.SupplierTest
{
    public class SupplierTest_DB
    {
        private readonly IMapper _mapper;

        //=========================================================
        // Constructor
        //=========================================================

        public SupplierTest_DB()
        {

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SupplierProfile>();   // hoặc MappingProfile của bạn
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

            return new PetCenterContext(options);
        }

        private async Task<Supplier> EnsureActiveSupplierAsync(PetCenterContext context, string? taxId = null)
        {
            var supplier = await context.Suppliers.FirstOrDefaultAsync(x => x.IsActive && (taxId == null || x.TaxId == taxId));
            if (supplier != null)
            {
                return supplier;
            }

            supplier = new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierName = "Integration Seed Supplier",
                SupplierEmail = $"supplier-{Guid.NewGuid():N}@test.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                TaxId = taxId ?? Guid.NewGuid().ToString("N")[..10],
                IsActive = true
            };

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();
            return supplier;
        }

        [Fact]
        public async Task ITCID01_GetAllAsync_ShouldReturnSuppliers()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var result = await service.GetAllAsync();

            Assert.NotNull(result);
        }
        [Fact]
        public async Task ITCID02_GetAllAsync_ShouldMapCorrectly()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var result = await service.GetAllAsync();

            if (result.Any())
            {
                var supplier = result.First();

                Assert.NotEqual(Guid.Empty, supplier.SupplierId);
                Assert.False(string.IsNullOrWhiteSpace(supplier.SupplierName));
            }
        }
        [Fact]
        public async Task ITCID03_GetByIdAsync_ExistingSupplier_ShouldReturnSupplier()
        {
            using var context = CreateContext();

            var supplier = await EnsureActiveSupplierAsync(context);

            var repository = new SupplierRepository(context);
            var service = new SupplierService(repository, _mapper);

            var result = await service.GetByIdAsync(supplier.SupplierId);

            Assert.NotNull(result);
            Assert.Equal(supplier.SupplierId, result.SupplierId);
        }
        [Fact]
        public async Task ITCID04_GetByIdAsync_NotFound_ShouldReturnNull()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
        [Fact]
        public async Task ITCID05_CreateAsync_ShouldInsertSupplier()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var dto = new CreateSupplierRequestDTO
            {
                SupplierName = "Integration Test",
                SupplierEmail = "integration@test.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                TaxId = Guid.NewGuid().ToString("N")[..10]
            };

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);

            Assert.True(context.Suppliers.Any(x =>
                x.SupplierId == result.SupplierId));
        }
        [Fact]
        public async Task ITCID06_CreateAsync_DuplicateTaxId_ShouldThrowException()
        {
            using var context = CreateContext();

            var exist = await EnsureActiveSupplierAsync(context, Guid.NewGuid().ToString("N")[..10]);

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var dto = new CreateSupplierRequestDTO
            {
                SupplierName = "Duplicate",
                SupplierEmail = "duplicate@test.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                TaxId = exist.TaxId
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }
        [Fact]
        public async Task ITCID07_UpdateAsync_ShouldUpdateSupplier()
        {
            using var context = CreateContext();

            var supplier = await EnsureActiveSupplierAsync(context);
            var supplierId = supplier.SupplierId;

            var repository = new SupplierRepository(context);
            var service = new SupplierService(repository, _mapper);

            Random rnd = new Random();
            long randomTaxId = rnd.NextInt64(1000000000L, 10000000000L);

            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "Updated Name",
                SupplierEmail = supplier.SupplierEmail,
                SupplierPhoneNumber = supplier.SupplierPhoneNumber,
                SupplierAddress = supplier.SupplierAddress,
                TaxId = randomTaxId.ToString()
            };

            var success = await service.UpdateAsync(supplierId, dto);

            Assert.True(success);

            using var verifyContext = CreateContext();
            var updated = await verifyContext.Suppliers.FindAsync(supplierId);

            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.SupplierName);
            Assert.Equal(dto.TaxId, updated.TaxId);
        }



        [Fact]
        public async Task ITCID08_UpdateAsync_NotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = await service.UpdateAsync(Guid.NewGuid(), dto);

            Assert.False(result);
        }
        [Fact]
        public async Task ITCID09_DeleteAsync_ShouldSetInactive()
        {
            using var context = CreateContext();

            var supplier = await EnsureActiveSupplierAsync(context);

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var result = await service.DeleteAsync(supplier.SupplierId);

            Assert.True(result);

            var deleted = await context.Suppliers.FindAsync(supplier.SupplierId);

            Assert.False(deleted!.IsActive);
        }
        [Fact]
        public async Task ITCID10_DeleteAsync_NotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();

            var repository = new SupplierRepository(context);

            var service = new SupplierService(repository, _mapper);

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
