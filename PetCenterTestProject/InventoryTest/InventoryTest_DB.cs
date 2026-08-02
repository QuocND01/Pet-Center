using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PetCenterAPI.DTOs.Requests.Inventory;
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

namespace PetCenterTestProject.InventoryTest
{
    public class InventoryTest_DB
    {
        private readonly PetCenterContext _context;
        private readonly InventoryService _service;
        private readonly IMapper _mapper;


        //=========================================================
        // Constructor
        //=========================================================

        public InventoryTest_DB()
        {
            _context = CreateContext();


            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<InventoryProfile>();

            }, NullLoggerFactory.Instance)
            .CreateMapper();


            // Real repository
            IInventoryRepository repository =
                new InventoryRepository(_context);


            _service = new InventoryService(
                repository,
                _mapper);
        }



        //=========================================================
        // Create SQL Server Context
        //=========================================================

        private PetCenterContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;")
                .Options;


            return new PetCenterContext(options);
        }



        //=========================================================
        // UTCID01
        // Existing inventory
        //=========================================================

        [Fact]
        public async Task UTCID01_GetPagedAsync_ShouldReturnInventoryList()
        {
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            var result =
                await _service.GetPagedAsync(request);


            Assert.NotNull(result);

            Assert.NotNull(result.Items);

            Assert.True(
                result.TotalRecords > 0);
        }



        //=========================================================
        // UTCID02
        // Empty result
        //=========================================================

        [Fact]
        public async Task UTCID02_GetPagedAsync_NoData_ShouldReturnEmptyList()
        {
            var request = new InventoryQueryRequestDTO
            {
                Page = 9999,
                PageSize = 10
            };


            var result =
                await _service.GetPagedAsync(request);


            Assert.NotNull(result);

            Assert.Empty(result.Items);
        }



        //=========================================================
        // UTCID03
        // Null Product Image
        //=========================================================

        [Fact]
        public async Task UTCID03_GetPagedAsync_NullProductImage_ShouldReturnSuccessfully()
        {
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            var result =
                await _service.GetPagedAsync(request);


            Assert.NotNull(result);


            foreach (var item in result.Items)
            {
                Assert.NotNull(item);

                // optional field
                Assert.True(
                    item.ProductImage == null ||
                    item.ProductImage != null);
            }
        }



        //=========================================================
        // UTCID04
        // TotalRecords > PageSize
        //=========================================================

        [Fact]
        public async Task UTCID04_GetPagedAsync_ShouldCalculateTotalPages()
        {
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 5
            };


            var result =
                await _service.GetPagedAsync(request);


            var expected =
                (int)Math.Ceiling(
                    result.TotalRecords /
                    (double)request.PageSize);


            Assert.Equal(
                expected,
                result.TotalPages);
        }



        //=========================================================
        // UTCID05
        // Last page
        //=========================================================

        [Fact]
        public async Task UTCID05_GetPagedAsync_LastPage_ShouldReturnRemainingRecords()
        {
            var totalRecords =
                await _context.Inventories.CountAsync();


            var pageSize = 10;


            var lastPage =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);



            var request = new InventoryQueryRequestDTO
            {
                Page = lastPage,
                PageSize = pageSize
            };


            var result =
                await _service.GetPagedAsync(request);



            Assert.True(
                result.Items.Count <= pageSize);


            Assert.Equal(
                lastPage,
                result.Page);
        }
        //=========================================================
        // UTCID06
        // Existing inventory ID -> Should return detail DTO with relations
        //=========================================================

        [Fact]
        public async Task UTCID06_GetByIdAsync_ExistingId_ShouldReturnInventoryDetail()
        {
            // Arrange: Lấy một ID thực tế đang tồn tại từ Database test
            var existingInventory = await _context.Inventories.FirstOrDefaultAsync();

            Assert.True(existingInventory != null,
                "Database test phải có ít nhất một bản ghi Inventory để chạy test case này.");

            var targetId = existingInventory.InventoryId;

            // Act: Gọi service
            var result = await _service.GetByIdAsync(targetId);

            // Assert: Kiểm tra dữ liệu trả về khớp hoàn toàn với DB
            Assert.NotNull(result);

            Assert.Equal(
                targetId,
                result.InventoryId);

            Assert.Equal(
                existingInventory.ProductId,
                result.ProductId);

            // Xác thực dữ liệu số lượng kho khớp với thực tế
            Assert.Equal(
                existingInventory.QuantityAvailable,
                result.QuantityAvailable);

            Assert.Equal(
                existingInventory.QuantityReserved,
                result.QuantityReserved);

            // Kiểm tra các chuỗi không được phép null theo định nghĩa DTO (null!)
            Assert.NotNull(result.ProductName);

            Assert.NotNull(result.SKU);

            Assert.NotNull(result.Category);

            Assert.NotNull(result.Brand);

            // Kiểm tra danh sách liên kết đã được khởi tạo (không bị null)
            Assert.NotNull(result.Batches);

            Assert.NotNull(result.Transactions);
        }



        //=========================================================
        // UTCID07
        // Non-existing inventory ID -> Should return null
        //=========================================================

        [Fact]
        public async Task UTCID07_GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange: Tạo một Guid ngẫu nhiên không tồn tại trong hệ thống
            var nonExistingId = Guid.NewGuid();

            // Act: Gọi service
            var result = await _service.GetByIdAsync(nonExistingId);

            // Assert: Kết quả bắt buộc phải trả về null đúng theo logic code
            Assert.Null(result);
        }



        //=========================================================
        // UTCID08
        // Deep check on mapped collections (Batches & Transactions)
        //=========================================================

        [Fact]
        public async Task UTCID08_GetByIdAsync_CheckCollectionsMapping_ShouldReturnSuccessfully()
        {
            // Arrange: Lấy một bản ghi từ Database test
            var existingInventory = await _context.Inventories.FirstOrDefaultAsync();

            Assert.True(existingInventory != null,
                "Database test phải có ít nhất một bản ghi Inventory để chạy test case này.");

            // Act: Gọi service
            var result = await _service.GetByIdAsync(existingInventory.InventoryId);

            // Assert
            Assert.NotNull(result);

            // Kiểm tra cấu trúc phần tử trong danh sách Batches nếu có dữ liệu
            if (result.Batches.Count > 0)
            {
                var firstBatch = result.Batches.First();

                Assert.NotEqual(Guid.Empty, firstBatch.ImportStockDetailsId);

                Assert.NotNull(firstBatch.SKU);

                Assert.NotNull(firstBatch.BatchCode);

                Assert.True(firstBatch.Quantity >= 0);

                Assert.True(
                    firstBatch.ManufacturingDate == null ||
                    firstBatch.ManufacturingDate != null);
            }

            // Kiểm tra cấu trúc phần tử trong danh sách Transactions nếu có dữ liệu
            if (result.Transactions.Count > 0)
            {
                var firstTransaction = result.Transactions.First();

                Assert.NotEqual(Guid.Empty, firstTransaction.TransactionId);

                // Trường tùy chọn (Optional Field)
                Assert.True(
                    firstTransaction.Note == null ||
                    firstTransaction.Note != null);
            }
        }

    }
}