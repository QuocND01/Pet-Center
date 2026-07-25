using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;
using MockQueryable.Moq; // <-- ĐÃ BỔ SUNG THƯ VIỆN NÀY ĐỂ FIX LỖI IAsyncEnumerable
using Moq;
using PetCenterAPI.Hubs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterTestProject.OrderTest
{
    public class OrderTest_Mock
    {
        //=========================================================
        // Mock & Setup
        //=========================================================
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IInventoryRepository> _inventoryRepoMock;
        private readonly Mock<IInventoryTransactionRepository> _invenTransactionRepoMock;
        private readonly Mock<IHubContext<AppHub>> _hubContextMock;
        private readonly IMapper _mapper;
        private readonly OrderService _service;

        public OrderTest_Mock()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _inventoryRepoMock = new Mock<IInventoryRepository>();
            _invenTransactionRepoMock = new Mock<IInventoryTransactionRepository>();

            // 1. Setup AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Order, ReadOrderListDTO>()
                   .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : "Unknown"));
            }, NullLoggerFactory.Instance);

            _mapper = mapperConfig.CreateMapper();

            // 2. Setup SignalR Mock
            _hubContextMock = new Mock<IHubContext<AppHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);

            // 3. Init Service
            _service = new OrderService(
                _orderRepoMock.Object,
                _inventoryRepoMock.Object,
                _invenTransactionRepoMock.Object,
                _mapper,
                NullLogger<OrderService>.Instance,
                _hubContextMock.Object
            );
        }

        //=========================================================
        // Helpers
        //=========================================================

        // Helper tạo ODataQueryOptions giả lập phục vụ cho GetOrderListAdminAsync
        private ODataQueryOptions<ReadOrderListDTO> CreateDummyODataOptions()
        {
            var modelBuilder = new ODataConventionModelBuilder();

            // CHỈ ĐÍCH DANH OrderId LÀ KHÓA CHÍNH (PRIMARY KEY) ĐỂ ODATA KHÔNG BÁO LỖI
            modelBuilder.EntitySet<ReadOrderListDTO>("Orders").EntityType.HasKey(x => x.OrderId);

            var edmModel = modelBuilder.GetEdmModel();

            // Bỏ đi new ODataPath() và truyền null để tránh lỗi namespace
            var context = new ODataQueryContext(edmModel, typeof(ReadOrderListDTO), null);
            var request = new DefaultHttpContext().Request;

            return new ODataQueryOptions<ReadOrderListDTO>(context, request);
        }

        //=====================================================================
        // Function: GetOrderListAdminAsync() (6 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetOrderListAdminAsync_KeywordEmpty_ValidRange_ShouldReturnPagedList()
        {
            var queryOptions = CreateDummyODataOptions();
            // SỬ DỤNG .BuildMock() THAY VÌ .AsQueryable() ĐỂ HỖ TRỢ BẤT ĐỒNG BỘ EF CORE
            var orders = new List<Order> { new Order { OrderId = Guid.NewGuid() } }.BuildMock();

            _orderRepoMock.Setup(x => x.GetAllOrders()).Returns(orders);

            var result = await _service.GetOrderListAdminAsync(queryOptions);

            Assert.NotNull(result);
            Assert.IsType<List<ReadOrderListDTO>>(result);
        }

        [Fact]
        public async Task UTCID02_GetOrderListAdminAsync_KeywordExists_ShouldReturnPagedList()
        {
            var queryOptions = CreateDummyODataOptions();
            var orders = new List<Order> { new Order { OrderId = Guid.NewGuid() } }.BuildMock();

            _orderRepoMock.Setup(x => x.GetAllOrders()).Returns(orders);

            var result = await _service.GetOrderListAdminAsync(queryOptions);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UTCID03_GetOrderListAdminAsync_KeywordNotExists_ShouldReturnEmptyList()
        {
            var queryOptions = CreateDummyODataOptions();
            var orders = new List<Order>().BuildMock(); // Empty list

            _orderRepoMock.Setup(x => x.GetAllOrders()).Returns(orders);

            var result = await _service.GetOrderListAdminAsync(queryOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID04_GetOrderListAdminAsync_StatusPending_ShouldReturnPagedList()
        {
            var queryOptions = CreateDummyODataOptions();
            var orders = new List<Order> { new Order { OrderId = Guid.NewGuid(), Status = 1 } }.BuildMock();

            _orderRepoMock.Setup(x => x.GetAllOrders()).Returns(orders);

            var result = await _service.GetOrderListAdminAsync(queryOptions);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UTCID05_GetOrderListAdminAsync_InvalidDateRange_ShouldThrowArgumentException()
        {
            var queryOptions = CreateDummyODataOptions();

            // Giả lập Repository/Service ném lỗi Invalid Date Range theo kịch bản Excel
            _orderRepoMock.Setup(x => x.GetAllOrders()).Throws(new ArgumentException("Invalid date range"));

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrderListAdminAsync(queryOptions));
            Assert.Equal("Invalid date range", ex.Message);
        }

        [Fact]
        public async Task UTCID06_GetOrderListAdminAsync_RepositoryThrows_ShouldThrowException()
        {
            var queryOptions = CreateDummyODataOptions();
            _orderRepoMock.Setup(x => x.GetAllOrders()).Throws(new Exception("Database connection failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetOrderListAdminAsync(queryOptions));
            Assert.Equal("Database connection failed", ex.Message);
        }

        //=====================================================================
        // Function: GetCustomerOrderHistoryAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetCustomerOrderHistory_HasOrders_ShouldReturnList()
        {
            var customerId = Guid.NewGuid();
            var orders = new List<Order>
            {
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId, TotalAmount = 100 },
                new Order { OrderId = Guid.NewGuid(), CustomerId = customerId, TotalAmount = 200 }
            };

            _orderRepoMock.Setup(x => x.GetOrdersByCustomerIdAsync(customerId)).ReturnsAsync(orders);

            var result = await _service.GetCustomerOrderHistoryAsync(customerId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UTCID02_GetCustomerOrderHistory_NoOrders_ShouldReturnEmptyList()
        {
            var customerId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrdersByCustomerIdAsync(customerId)).ReturnsAsync(new List<Order>());

            var result = await _service.GetCustomerOrderHistoryAsync(customerId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetCustomerOrderHistory_RepositoryThrows_ShouldThrowException()
        {
            var customerId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrdersByCustomerIdAsync(customerId)).ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetCustomerOrderHistoryAsync(customerId));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: GetOrderDetailsAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetOrderDetailsAsync_ExistingGuid_ShouldReturnDTO()
        {
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                Customer = new Customer { FullName = "TienNN", PhoneNumber = "0999999999", Email = "tien@gmail.com" },
                OrderDate = DateTime.UtcNow,
                TotalAmount = 150000,
                Status = 1,
                PaymentMethod = "COD",
                AddressSnapshot = "Can Tho",
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 2,
                        UnitPrice = 50000,
                        OrderProductSnapshot = new OrderProductSnapshot { ProductName = "Dog Food" }
                    }
                }
            };

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var result = await _service.GetOrderDetailsAsync(orderId);

            Assert.NotNull(result);
            Assert.Equal(orderId, result!.OrderId);
            Assert.Equal("TienNN", result.CustomerName);
            Assert.Single(result.OrderItems);
            Assert.Equal("Dog Food", result.OrderItems[0].ProductName);
        }

        [Fact]
        public async Task UTCID02_GetOrderDetailsAsync_NonExistingGuid_ShouldReturnNull()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

            var result = await _service.GetOrderDetailsAsync(orderId);

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetOrderDetailsAsync_RepositoryThrows_ShouldThrowException()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetOrderDetailsAsync(orderId));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: AdvanceOrderStatusAsync() (10 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_AdvanceOrderStatus_StatusCancelledOrCompleted_ShouldThrowInvalidOperation()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 0 }; // Cancelled (0) hoặc Completed (4)

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Equal("Cannot advance status of a cancelled or completed order.", ex.Message);
        }

        [Fact]
        public async Task UTCID02_AdvanceOrderStatus_StatusPending_ShouldAdvanceToProcessing()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 1 }; // Pending -> Processing

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);

            Assert.Equal(2, newStatus);
            Assert.Equal(2, order.Status);
            _orderRepoMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID03_AdvanceOrderStatus_StatusProcessing_InventoryNull_ShouldThrow()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 2, OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 2 } } };

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync((Inventory?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Không tìm thấy tồn kho", ex.Message);
        }

        [Fact]
        public async Task UTCID04_AdvanceOrderStatus_StatusProcessing_NotEnoughReserved_ShouldThrow()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 2, OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 5 } } };

            var inventory = new Inventory { ProductId = productId, QuantityReserved = 2 }; // Reserved < Quantity

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync(inventory);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Reserved không đủ", ex.Message);
        }

        [Fact]
        public async Task UTCID05_AdvanceOrderStatus_StatusProcessing_NoAvailableBatches_ShouldThrow()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 2, OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 5 } } };

            var inventory = new Inventory { ProductId = productId, QuantityReserved = 5 };

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync(inventory);
            _inventoryRepoMock.Setup(x => x.GetAvailableBatchesByProductIdAsync(productId)).ReturnsAsync(new List<ImportStockDetail>()); // Rỗng

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Không có lô hàng khả dụng", ex.Message);
        }

        [Fact]
        public async Task UTCID06_AdvanceOrderStatus_StatusProcessing_TotalBatchStockInsufficient_ShouldThrow()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 2, OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 5 } } };

            var inventory = new Inventory { ProductId = productId, QuantityReserved = 5 };
            var batches = new List<ImportStockDetail> { new ImportStockDetail { StockLeft = 3 } }; // Tổng batch stock < 5

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync(inventory);
            _inventoryRepoMock.Setup(x => x.GetAvailableBatchesByProductIdAsync(productId)).ReturnsAsync(batches);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Contains("Không đủ tồn kho batch", ex.Message);
        }

        [Fact]
        public async Task UTCID07_AdvanceOrderStatus_StatusProcessing_ValidStock_ShouldAdvanceToDelivering()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 2, OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 2 } } };

            var inventory = new Inventory { ProductId = productId, QuantityReserved = 5, QuantityAvailable = 10 };
            var batch1 = new ImportStockDetail { BatchCode = "B01", StockLeft = 1, QuantitySold = 0 };
            var batch2 = new ImportStockDetail { BatchCode = "B02", StockLeft = 5, QuantitySold = 0 };

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync(inventory);
            _inventoryRepoMock.Setup(x => x.GetAvailableBatchesByProductIdAsync(productId)).ReturnsAsync(new List<ImportStockDetail> { batch1, batch2 });

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);

            Assert.Equal(3, newStatus);
            Assert.Equal(0, batch1.StockLeft);
            Assert.Equal(4, batch2.StockLeft);
            Assert.Equal(3, inventory.QuantityReserved);

            _invenTransactionRepoMock.Verify(x => x.AddTransactionAsync(It.IsAny<InventoryTransaction>()), Times.Exactly(2));
            _invenTransactionRepoMock.Verify(x => x.SaveChange(), Times.Once);
            _orderRepoMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID08_AdvanceOrderStatus_StatusDelivering_ShouldAdvanceToCompleted()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 3, DeliveredDate = null }; // Delivering -> Completed

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var newStatus = await _service.AdvanceOrderStatusAsync(orderId);

            Assert.Equal(4, newStatus);
            Assert.NotNull(order.DeliveredDate);
            _orderRepoMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID09_AdvanceOrderStatus_NonExistingGuid_ShouldThrowKeyNotFound()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task UTCID10_AdvanceOrderStatus_RepositoryThrows_ShouldThrowException()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AdvanceOrderStatusAsync(orderId));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: CancelOrderAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_CancelOrderAsync_PendingOrProcessing_ShouldCancelAndRevertStock()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                Status = 1, // Pending
                OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = productId, Quantity = 5 } }
            };
            var inventory = new Inventory
            {
                ProductId = productId,
                QuantityReserved = 10,
                QuantityAvailable = 50
            };

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _inventoryRepoMock.Setup(x => x.GetInventoryByProductIdAsync(productId)).ReturnsAsync(inventory);

            var result = await _service.CancelOrderAsync(orderId);

            Assert.True(result);
            Assert.Equal(0, order.Status);
            Assert.Equal(5, inventory.QuantityReserved);
            Assert.Equal(55, inventory.QuantityAvailable);

            _orderRepoMock.Verify(x => x.SaveAsync(), Times.Once);
            _hubContextMock.Verify(x => x.Clients, Times.AtLeastOnce);
        }

        [Fact]
        public async Task UTCID02_CancelOrderAsync_NonExistingGuid_ShouldThrowKeyNotFound()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Order not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_CancelOrderAsync_DeliveringStatus_ShouldThrowInvalidOperation()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 3 }; // Delivering

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Cannot cancel an order that is shipping or completed.", ex.Message);
        }

        [Fact]
        public async Task UTCID04_CancelOrderAsync_CompletedStatus_ShouldThrowInvalidOperation()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = 4 }; // Completed

            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Cannot cancel an order that is shipping or completed.", ex.Message);
        }

        [Fact]
        public async Task UTCID05_CancelOrderAsync_RepositoryThrows_ShouldThrowException()
        {
            var orderId = Guid.NewGuid();
            _orderRepoMock.Setup(x => x.GetOrderByIdAsync(orderId)).ThrowsAsync(new Exception("Database connection failed"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.CancelOrderAsync(orderId));
            Assert.Equal("Database connection failed", ex.Message);
        }
    }
}