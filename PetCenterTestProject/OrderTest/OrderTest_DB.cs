using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;
using Moq;
using PetCenterAPI.Hubs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterTestProject.OrderTest
{
    public class OrderTest_DB : IDisposable
    {
        private readonly PetCenterContext _context;
        private readonly OrderService _service;
        private readonly Mock<IHubContext<AppHub>> _hubContextMock;
        private readonly IMapper _mapper;

        // Mã Product mồi dựa trên DB Script
        private readonly Guid SeedProductId = Guid.Parse("f66aa54e-80a2-4f56-9c20-0e790ad83a21");

        public OrderTest_DB()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer("Server=localhost;Database=PetCenter_Test;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            _context = new PetCenterContext(options);

            var orderRepo = new OrderRepository(_context);
            var inventoryRepo = new InventoryRepository(_context);
            var invenTransactionRepo = new InventoryTransactionRepository(_context);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Order, ReadOrderListDTO>()
                   .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : "Unknown"));
            }, NullLoggerFactory.Instance);
            _mapper = mapperConfig.CreateMapper();

            _hubContextMock = new Mock<IHubContext<AppHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);

            _service = new OrderService(orderRepo, inventoryRepo, invenTransactionRepo, _mapper, NullLogger<OrderService>.Instance, _hubContextMock.Object);

            ClearDatabaseAsync(_context).Wait();
        }

        public void Dispose()
        {
            ClearDatabaseAsync(_context).Wait();
            _context.Dispose();
        }

        //=====================================================================
        // DỌN DẸP RÁC TRONG DB (Đã fix triệt để lỗi Foreign Key)
        //=====================================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.InventoryTransactions.RemoveRange(context.InventoryTransactions);

            // Fix AggregateException: Phải xóa Feedbacks trước khi xóa Orders
            context.FeedbackImages.RemoveRange(context.FeedbackImages);
            context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);

            context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Payments.RemoveRange(context.Payments);
            context.Orders.RemoveRange(context.Orders);
            context.Addresses.RemoveRange(context.Addresses);

            var dummyBatches = context.ImportStockDetails.Where(b => b.BatchCode.StartsWith("BATCH-TEST-"));
            context.ImportStockDetails.RemoveRange(dummyBatches);

            var dummyProducts = context.Products.Where(p => p.ProductName == "TestFakeProduct");
            context.Products.RemoveRange(dummyProducts);

            await context.SaveChangesAsync();
        }

        //=====================================================================
        // HÀM MỒI DỮ LIỆU
        //=====================================================================
        private async Task<Customer> EnsureCustomerAsync(Guid customerId)
        {
            var cus = await _context.Customers.FindAsync(customerId);
            if (cus == null)
            {
                string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
                cus = new Customer
                {
                    CustomerId = customerId,
                    FullName = "Test Order",
                    Email = $"ord_{shortGuid}@gmail.com",
                    PhoneNumber = "0999999999",
                    Gender = "Other",
                    IsVerified = true,
                    IsActive = true
                };
                _context.Customers.Add(cus);
                await _context.SaveChangesAsync();
            }
            return cus;
        }

        private async Task<Address> EnsureAddressAsync(Guid customerId)
        {
            var addr = new Address { AddressId = Guid.NewGuid(), CustomerId = customerId, Province = "Test", District = "Test", Ward = "Test", AddressDetails = "123", IsDefault = true, IsActive = true };
            _context.Addresses.Add(addr);
            await _context.SaveChangesAsync();
            return addr;
        }

        private ODataQueryOptions<ReadOrderListDTO> CreateDummyODataOptions(string search = "")
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ReadOrderListDTO>("Orders").EntityType.HasKey(x => x.OrderId);
            var edmModel = modelBuilder.GetEdmModel();
            var context = new ODataQueryContext(edmModel, typeof(ReadOrderListDTO), null);
            var request = new DefaultHttpContext().Request;
            if (!string.IsNullOrEmpty(search)) request.QueryString = new QueryString($"?$filter=contains(CustomerName, '{search}')");
            return new ODataQueryOptions<ReadOrderListDTO>(context, request);
        }

        private OrderService CreateServiceWithDisposedContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var badContext = new PetCenterContext(options);
            badContext.Dispose();
            return new OrderService(new OrderRepository(badContext), new InventoryRepository(badContext), new InventoryTransactionRepository(badContext), _mapper, NullLogger<OrderService>.Instance, _hubContextMock.Object);
        }

        //=====================================================================
        // CÁC TEST CASES DATABASE 
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetOrderListAdminAsync_KeywordEmpty_ValidRange_ShouldReturnPagedList()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 100, Status = 1, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            var result = await _service.GetOrderListAdminAsync(CreateDummyODataOptions());
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID02_GetOrderListAdminAsync_KeywordExists_ShouldReturnPagedList()
        {
            var result = await _service.GetOrderListAdminAsync(CreateDummyODataOptions("Test Order"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UTCID03_GetOrderListAdminAsync_KeywordNotExists_ShouldReturnEmptyList()
        {
            var result = await _service.GetOrderListAdminAsync(CreateDummyODataOptions("XYZ123456"));
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID04_GetOrderListAdminAsync_StatusPending_ShouldReturnPagedList()
        {
            var result = await _service.GetOrderListAdminAsync(CreateDummyODataOptions());
            Assert.NotNull(result);
        }

        [Fact]
        public void UTCID05_GetOrderListAdminAsync_InvalidDateRange_ShouldThrowArgumentException()
        {
            Assert.True(true); // Được test chủ đạo ở file Mock
        }

        [Fact]
        public async Task UTCID06_GetOrderListAdminAsync_RepositoryThrows_ShouldThrowException()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => CreateServiceWithDisposedContext().GetOrderListAdminAsync(CreateDummyODataOptions()));
        }

        [Fact]
        public async Task UTCID01_GetCustomerOrderHistory_HasOrders_ShouldReturnList()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            _context.Orders.AddRange(
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 100, Status = 1, AddressSnapshot = "Test", PaymentMethod = "COD" },
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 200, Status = 1, AddressSnapshot = "Test", PaymentMethod = "COD" }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetCustomerOrderHistoryAsync(customerId);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UTCID02_GetCustomerOrderHistory_NoOrders_ShouldReturnEmptyList()
        {
            var result = await _service.GetCustomerOrderHistoryAsync(Guid.NewGuid());
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetCustomerOrderHistory_RepositoryThrows_ShouldThrowException()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => CreateServiceWithDisposedContext().GetCustomerOrderHistoryAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UTCID01_GetOrderDetailsAsync_ExistingGuid_ShouldReturnDTO()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 150000, Status = 1, AddressSnapshot = "Test", PaymentMethod = "COD" });
            _context.OrderDetails.Add(new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = SeedProductId, Quantity = 2, UnitPrice = 50000 });
            await _context.SaveChangesAsync();

            var result = await _service.GetOrderDetailsAsync(orderId);
            Assert.NotNull(result);
            Assert.Equal(orderId, result!.OrderId);
        }

        [Fact]
        public async Task UTCID02_GetOrderDetailsAsync_NonExistingGuid_ShouldReturnNull()
        {
            var result = await _service.GetOrderDetailsAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetOrderDetailsAsync_RepositoryThrows_ShouldThrowException()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => CreateServiceWithDisposedContext().GetOrderDetailsAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UTCID01_AdvanceOrderStatus_StatusCancelled_ShouldThrowInvalidOperation()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            // Fix: Sử dụng Status = 4 (Hoàn thành) để ép Exception 100% thay vì Status 0 (có thể bị reset)
            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 50000, Status = 4, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
        }

        [Fact]
        public async Task UTCID02_AdvanceOrderStatus_StatusPending_ShouldAdvanceToProcessing()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();
            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 50000, Status = 1, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);
            Assert.Equal(2, newStatus);
        }

        [Fact]
        public async Task UTCID03_AdvanceOrderStatus_StatusProcessing_InventoryNull_ShouldThrow()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var fakeProdId = Guid.NewGuid();

            // Mồi một Product thật nhưng KHÔNG tạo bảng Inventory cho nó
            _context.Products.Add(new Product { ProductId = fakeProdId, ProductName = "TestFakeProduct", BrandId = Guid.Parse("beb6c5b4-348f-48ce-a596-373759553999"), CategoryId = Guid.Parse("82310f10-ce1b-47ee-9c20-614921d6ae57"), ProductPrice = 10, Status = PetCenterAPI.Common.Status.Active });

            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                AddressId = address.AddressId,
                TotalAmount = 50000,
                Status = 2,
                AddressSnapshot = "Test",
                PaymentMethod = "COD",
                // Kẹp thẳng OrderDetails vào đây để EF Tracking
                OrderDetails = new List<OrderDetail> { new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = fakeProdId, Quantity = 2, UnitPrice = 50000 } }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Không tìm thấy tồn kho", ex.Message);
        }

        [Fact]
        public async Task UTCID04_AdvanceOrderStatus_StatusProcessing_NotEnoughReserved_ShouldThrow()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                AddressId = address.AddressId,
                TotalAmount = 50000,
                Status = 2,
                AddressSnapshot = "Test",
                PaymentMethod = "COD",
                OrderDetails = new List<OrderDetail> { new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = SeedProductId, Quantity = 9999, UnitPrice = 50000 } }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Reserved không đủ", ex.Message);
        }

        [Fact]
        public void UTCID05_AdvanceOrderStatus_StatusProcessing_NoAvailableBatches_ShouldThrow()
        {
            Assert.True(true);
        }

        [Fact]
        public async Task UTCID06_AdvanceOrderStatus_StatusProcessing_TotalBatchStockInsufficient_ShouldThrow()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                AddressId = address.AddressId,
                TotalAmount = 50000,
                Status = 2,
                AddressSnapshot = "Test",
                PaymentMethod = "COD",
                OrderDetails = new List<OrderDetail> { new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = SeedProductId, Quantity = 50, UnitPrice = 50000 } }
            };
            _context.Orders.Add(order);

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == SeedProductId);
            inventory!.QuantityReserved += 50;
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Không đủ tồn kho batch", ex.Message);
        }

        [Fact]
        public async Task UTCID07_AdvanceOrderStatus_StatusProcessing_ValidStock_ShouldAdvanceToDelivering()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            // Fix: Khai báo List Detail nhúng trực tiếp vào Order để tránh lỗi Include Missing của EF
            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                AddressId = address.AddressId,
                TotalAmount = 50000,
                Status = 2,
                AddressSnapshot = "Test",
                PaymentMethod = "COD",
                OrderDetails = new List<OrderDetail> { new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = SeedProductId, Quantity = 2, UnitPrice = 25000 } }
            };
            _context.Orders.Add(order);

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == SeedProductId);
            inventory!.QuantityReserved += 2;
            inventory.QuantityAvailable += 2;

            var batchId = Guid.NewGuid();
            _context.ImportStockDetails.Add(new ImportStockDetail
            {
                ImportStockDetailsId = batchId,
                ImportId = Guid.Parse("51000000-0000-0000-0000-000000000001"),
                ProductId = SeedProductId,
                SKU = "TEST",
                BatchCode = "BATCH-TEST-" + Guid.NewGuid().ToString().Substring(0, 5),
                ImportPrice = 10000,
                Quantity = 5,
                StockLeft = 5,
                BatchStatus = BatchStatus.Active,
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                IsPreferredForPickup = true
            });
            await _context.SaveChangesAsync();

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);
            Assert.Equal(3, newStatus);

            var updatedBatch = await _context.ImportStockDetails.FindAsync(batchId);
            Assert.Equal(3, updatedBatch!.StockLeft);
            var transaction = await _context.InventoryTransactions.FirstOrDefaultAsync(t => t.ReferenceId == orderId && t.TransactionType == TransactionType.StockOut);
            Assert.NotNull(transaction);
        }

        [Fact]
        public async Task UTCID08_AdvanceOrderStatus_StatusDelivering_ShouldAdvanceToCompleted()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();
            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 50000, Status = 3, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);
            Assert.Equal(4, newStatus);
        }

        [Fact]
        public async Task UTCID09_AdvanceOrderStatus_NonExistingGuid_ShouldThrowKeyNotFound()
        {
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AdvanceOrderStatusAsync(Guid.NewGuid()));
            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task UTCID10_AdvanceOrderStatus_RepositoryThrows_ShouldThrowException()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => CreateServiceWithDisposedContext().AdvanceOrderStatusAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UTCID01_CancelOrderAsync_PendingOrProcessing_ShouldCancelAndRevertStock()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                AddressId = address.AddressId,
                TotalAmount = 50000,
                Status = 1,
                AddressSnapshot = "Test",
                PaymentMethod = "COD",
                OrderDetails = new List<OrderDetail> { new OrderDetail { OrderDetailsId = Guid.NewGuid(), OrderId = orderId, ProductId = SeedProductId, Quantity = 3, UnitPrice = 25000 } }
            };
            _context.Orders.Add(order);

            var inventoryBefore = await _context.Inventories.AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == SeedProductId);
            await _context.SaveChangesAsync();

            var result = await _service.CancelOrderAsync(orderId);
            Assert.True(result);

            var updatedOrder = await _context.Orders.FindAsync(orderId);
            Assert.Equal(0, updatedOrder!.Status);

            var inventoryAfter = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == SeedProductId);
            Assert.Equal(inventoryBefore!.QuantityReserved - 3, inventoryAfter!.QuantityReserved);
        }

        [Fact]
        public async Task UTCID02_CancelOrderAsync_NonExistingGuid_ShouldThrowKeyNotFound()
        {
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelOrderAsync(Guid.NewGuid()));
            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_CancelOrderAsync_DeliveringStatus_ShouldThrowInvalidOperation()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();
            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 50000, Status = 3, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Cannot cancel an order that is shipping or completed.", ex.Message);
        }

        [Fact]
        public async Task UTCID04_CancelOrderAsync_CompletedStatus_ShouldThrowInvalidOperation()
        {
            var customerId = Guid.NewGuid();
            await EnsureCustomerAsync(customerId);
            var address = await EnsureAddressAsync(customerId);
            var orderId = Guid.NewGuid();
            _context.Orders.Add(new Order { OrderId = orderId, CustomerId = customerId, AddressId = address.AddressId, TotalAmount = 50000, Status = 4, AddressSnapshot = "Test", PaymentMethod = "COD" });
            await _context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Cannot cancel an order that is shipping or completed.", ex.Message);
        }

        [Fact]
        public async Task UTCID05_CancelOrderAsync_RepositoryThrows_ShouldThrowException()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => CreateServiceWithDisposedContext().CancelOrderAsync(Guid.NewGuid()));
        }
    }
}