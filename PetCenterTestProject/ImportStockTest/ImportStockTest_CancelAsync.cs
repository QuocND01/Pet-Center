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
    public class ImportStockTest_CancelAsync : IDisposable
    {
        private readonly PetCenterContext _context;
        private readonly ImportStockRepository _repository;
        private readonly IMapper _mapper;
        private readonly ImportStockService _service;

        public ImportStockTest_CancelAsync()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
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
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task UTCID01_CancelAsync_PendingImport_ShouldCancelSuccessfully()
        {
            // Arrange

            var import = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = _context.Suppliers.First().SupplierId,
                StaffId = _context.Staffs.First().StaffId,
                InvoiceNumber = "TEST-CANCEL-001",
                Status = ImportStatus.Pending,
                ImportDate = DateTime.UtcNow
            };


            _context.ImportStocks.Add(import);
            await _context.SaveChangesAsync();


            // Act

            await _service.CancelAsync(import.ImportId);


            // Assert

            var result = await _context.ImportStocks
                .FirstAsync(x => x.ImportId == import.ImportId);


            Assert.Equal(
                ImportStatus.Cancelled,
                result.Status);


            // Cleanup

            _context.ImportStocks.Remove(result);
            await _context.SaveChangesAsync();
        }



        [Fact]
        public async Task UTCID02_CancelAsync_ImportNotFound_ShouldThrowException()
        {
            // Arrange

            var id = Guid.NewGuid();


            // Act

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CancelAsync(id));


            // Assert

            Assert.Equal(
                "Import not found",
                ex.Message);
        }



        [Fact]
        public async Task UTCID03_CancelAsync_ConfirmedImport_ShouldThrowException()
        {
            // Arrange

            var import = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = _context.Suppliers.First().SupplierId,
                StaffId = _context.Staffs.First().StaffId,
                InvoiceNumber = "TEST-CANCEL-002",
                Status = ImportStatus.Confirmed,
                ImportDate = DateTime.UtcNow
            };


            _context.ImportStocks.Add(import);
            await _context.SaveChangesAsync();



            // Act

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CancelAsync(import.ImportId));



            // Assert

            Assert.Equal(
                "Only pending import can be cancelled",
                ex.Message);



            // Cleanup

            _context.ImportStocks.Remove(import);
            await _context.SaveChangesAsync();
        }



        [Fact]
        public async Task UTCID04_CancelAsync_ConfirmedImport_ShouldThrowException()
        {
            // Seed Pending
            var import = new ImportStock
            {
                ImportId = Guid.NewGuid(),
                SupplierId = _context.Suppliers.First().SupplierId,
                StaffId = _context.Staffs.First().StaffId,
                InvoiceNumber = "TEST-CANCEL-003",
                Status = ImportStatus.Pending,
                ImportDate = DateTime.UtcNow,
                ImportStockDetails = new List<ImportStockDetail>()
            };

            _context.ImportStocks.Add(import);
            await _context.SaveChangesAsync();


            // Confirm trước
            await _service.ConfirmAsync(
                import.ImportId,
                import.StaffId);


            // Cancel sau
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CancelAsync(import.ImportId));


            Assert.Equal(
                "Only pending import can be cancelled",
                ex.Message);
        }







    }
}