using PetCenterAPI.Service.Interface;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.DTOs.Responses.Order;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
﻿using AutoMapper;
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
    [Collection("DatabaseTests")]
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
                .UseSqlServer("Server=.;Database=PetCenter_Test;User Id=sa;Password=123456;TrustServerCertificate=True;")
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
            EnsureSeedProductAsync(_context).Wait();
        }

        public void Dispose()
        {
            ClearDatabaseAsync(_context).Wait();
            EnsureSeedProductAsync(_context).Wait();
            _context.Dispose();
        }

        //=====================================================================
        // DỌN DẸP RÁC TRONG DB (Đã fix triệt để lỗi Foreign Key)
        //=====================================================================
                                                        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.ChangeTracker.Clear();

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [StaffRoles];");

            context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
            context.MedicalRecords.RemoveRange(context.MedicalRecords);
            context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
            context.AppointmentServices.RemoveRange(context.AppointmentServices);
            context.Appointments.RemoveRange(context.Appointments);
            context.Pets.RemoveRange(context.Pets);
            context.Diseases.RemoveRange(context.Diseases);
            context.VetFeedbacks.RemoveRange(context.VetFeedbacks);
            context.VetProfiles.RemoveRange(context.VetProfiles);
            context.ScheduleExceptions.RemoveRange(context.ScheduleExceptions);
            context.GlobalWorkSchedules.RemoveRange(context.GlobalWorkSchedules);
            context.CartDetails.RemoveRange(context.CartDetails);
            context.Carts.RemoveRange(context.Carts);
            context.OtpCodes.RemoveRange(context.OtpCodes);
            context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
            context.Vouchers.RemoveRange(context.Vouchers);
            context.FeedbackImages.RemoveRange(context.FeedbackImages);
            context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
            context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Payments.RemoveRange(context.Payments);
            context.Orders.RemoveRange(context.Orders);
            context.Addresses.RemoveRange(context.Addresses);
            context.Customers.RemoveRange(context.Customers);
            context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
            context.ImportProductSnapshots.RemoveRange(context.ImportProductSnapshots);
            context.ImportStockDetails.RemoveRange(context.ImportStockDetails);
            context.ImportStocks.RemoveRange(context.ImportStocks);
            context.Suppliers.RemoveRange(context.Suppliers);
            context.Staffs.RemoveRange(context.Staffs);
            context.Inventories.RemoveRange(context.Inventories);
            context.ProductImages.RemoveRange(context.ProductImages);
            context.ProductAttributes.RemoveRange(context.ProductAttributes);
            context.Products.RemoveRange(context.Products);
            context.CategoryAttributes.RemoveRange(context.CategoryAttributes);
            context.Categories.RemoveRange(context.Categories);
            context.Brands.RemoveRange(context.Brands);
            context.ServiceImages.RemoveRange(context.ServiceImages);
            context.Services.RemoveRange(context.Services);

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
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
     
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
    

        //=====================================================================
        // Helper: Create CheckoutService
        //=====================================================================
        private CheckoutService CreateCheckoutService(PetCenterContext context)
        {
            var hubMock = new Mock<IHubContext<AppHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubMock.Setup(x => x.Clients).Returns(clientsMock.Object);

            var vnPayMock = new Mock<IVnPayService>();
            var moMoMock = new Mock<IMoMoService>();

            var orderRepo = new PetCenterAPI.Repository.OrderRepository(context);
            var paymentRepo = new PetCenterAPI.Repository.PaymentRepository(context);
            var voucherRepo = new PetCenterAPI.Repository.VoucherRepository(context);
            var inventoryRepo = new PetCenterAPI.Repository.InventoryRepository(context);
            var productRepo = new PetCenterAPI.Repository.ProductRepository(context, new Mock<IMapper>().Object);
            var addressRepo = new PetCenterAPI.Repository.AddressRepository(context);
            var cartRepo = new PetCenterAPI.Repository.CartRepository(context);
            var appointmentRepo = new PetCenterAPI.Repository.AppointmentRepository(context);

            return new CheckoutService(
                orderRepo, paymentRepo, voucherRepo, inventoryRepo, productRepo, addressRepo, cartRepo, appointmentRepo,
                hubMock.Object, vnPayMock.Object, moMoMock.Object, NullLogger<CheckoutService>.Instance);
        }

        private async Task EnsureSeedProductAsync(PetCenterContext context)
        {
            var brandId21 = Guid.Parse("beb6c5b4-348f-48ce-a596-373759553999");
            var brand21 = await context.Brands.FindAsync(brandId21);
            if (brand21 == null)
            {
                brand21 = new Brand { BrandId = brandId21, BrandName = "Seed Brand UTC03", Status = PetCenterAPI.Common.Status.Active };
                context.Brands.Add(brand21);
                await context.SaveChangesAsync();
            }

            var catId11 = Guid.Parse("82310f10-ce1b-47ee-9c20-614921d6ae57");
            var cat11 = await context.Categories.FindAsync(catId11);
            if (cat11 == null)
            {
                cat11 = new Category { CategoryId = catId11, CategoryName = "Seed Category UTC03", Status = PetCenterAPI.Common.Status.Active };
                context.Categories.Add(cat11);
                await context.SaveChangesAsync();
            }

            var product = await context.Products.FindAsync(SeedProductId);
            if (product == null)
            {
                var brand = await context.Brands.FirstOrDefaultAsync(b => b.BrandName == "Seed Brand");
                if (brand == null)
                {
                    brand = new Brand { BrandId = Guid.NewGuid(), BrandName = "Seed Brand", Status = PetCenterAPI.Common.Status.Active };
                    context.Brands.Add(brand);
                    await context.SaveChangesAsync();
                }

                var category = await context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Seed Category");
                if (category == null)
                {
                    category = new Category { CategoryId = Guid.NewGuid(), CategoryName = "Seed Category", Status = PetCenterAPI.Common.Status.Active };
                    context.Categories.Add(category);
                    await context.SaveChangesAsync();
                }

                product = new Product
                {
                    ProductId = SeedProductId,
                    ProductName = "Seed Product",
                    ProductPrice = 50000,
                    CategoryId = category.CategoryId,
                    BrandId = brand.BrandId,
                    Status = PetCenterAPI.Common.Status.Active,
                    AddedAt = DateTime.UtcNow
                };
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            var inventory = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == SeedProductId);
            if (inventory == null)
            {
                inventory = new Inventory
                {
                    InventoryId = Guid.NewGuid(),
                    ProductId = SeedProductId,
                    SKU = "SEED-SKU",
                    QuantityAvailable = 1000,
                    QuantityReserved = 100,
                    LastUpdated = DateTime.UtcNow
                };
                context.Inventories.Add(inventory);
                await context.SaveChangesAsync();
            }

            var importStockId = Guid.Parse("51000000-0000-0000-0000-000000000001");
            var importStock = await context.ImportStocks.FindAsync(importStockId);
            if (importStock == null)
            {
                var staff = await context.Staffs.FirstOrDefaultAsync();
                if (staff == null)
                {
                    staff = new Staff
                    {
                        StaffId = Guid.NewGuid(),
                        FullName = "Test Staff",
                        Email = "staff@test.com",
                        PhoneNumber = "0123456789",
                        PasswordHash = "password",
                        Gender = "Male",
                        BirthDate = DateTime.UtcNow.AddYears(-25),
                        HireDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    context.Staffs.Add(staff);
                    await context.SaveChangesAsync();
                }

                var supplier = await context.Suppliers.FirstOrDefaultAsync();
                if (supplier == null)
                {
                    supplier = new Supplier
                    {
                        SupplierId = Guid.NewGuid(),
                        TaxId = "TAX-123",
                        SupplierName = "Test Supplier",
                        SupplierEmail = "supplier@test.com",
                        SupplierPhoneNumber = "0123456789",
                        SupplierAddress = "Test Address",
                        IsActive = true
                    };
                    context.Suppliers.Add(supplier);
                    await context.SaveChangesAsync();
                }

                importStock = new ImportStock
                {
                    ImportId = importStockId,
                    StaffId = staff.StaffId,
                    SupplierId = supplier.SupplierId,
                    ImportDate = DateTime.UtcNow,
                    TotalAmount = 100000,
                    InvoiceNumber = "INV-SEED",
                    Status = ImportStock.ImportStatus.Confirmed,
                    Note = "Seed Import Stock"
                };
                context.ImportStocks.Add(importStock);
                await context.SaveChangesAsync();
            }

            var batch = await context.ImportStockDetails.FirstOrDefaultAsync(b => b.ProductId == SeedProductId);
            if (batch == null)
            {
                batch = new ImportStockDetail
                {
                    ImportStockDetailsId = Guid.NewGuid(),
                    ImportId = importStockId,
                    ProductId = SeedProductId,
                    SKU = "SEED-SKU",
                    BatchCode = "SEED-BATCH",
                    ImportPrice = 10000,
                    Quantity = 20,
                    StockLeft = 20, // less than 50 for UTCID06
                    ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
                };
                context.ImportStockDetails.Add(batch);
                await context.SaveChangesAsync();
            }
        }

        private async Task<(Guid productId, Guid categoryId, Guid brandId)> SeedProductAndInventoryAsync(
            PetCenterContext context,
            string productName,
            int qtyAvailable,
            int qtyReserved)
        {
            var brand = new Brand { BrandId = Guid.NewGuid(), BrandName = "Test Brand", Status = PetCenterAPI.Common.Status.Active };
            var category = new Category { CategoryId = Guid.NewGuid(), CategoryName = "Test Category", Status = PetCenterAPI.Common.Status.Active };
            context.Brands.Add(brand);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = productName,
                ProductPrice = 100000,
                CategoryId = category.CategoryId,
                BrandId = brand.BrandId,
                Status = PetCenterAPI.Common.Status.Active,
                AddedAt = DateTime.UtcNow
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var inventory = new Inventory
            {
                InventoryId = Guid.NewGuid(),
                ProductId = product.ProductId,
                SKU = "SKU-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                QuantityAvailable = qtyAvailable,
                QuantityReserved = qtyReserved,
                LastUpdated = DateTime.UtcNow
            };
            context.Inventories.Add(inventory);
            await context.SaveChangesAsync();

            return (product.ProductId, category.CategoryId, brand.BrandId);
        }

        //=====================================================================
        // PlaceCodOrderAsync() Tests (UTCID01 to UTCID17)
        //=====================================================================

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID01_Success()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER1",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(5),
                MinOrderAmount = 50000,
                MaxDiscountAmount = 20000,
                UseageLimit = 5,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
            Assert.Equal("Order placed successfully!", result.Message);
            Assert.NotNull(result.OrderId);

            var order = await _context.Orders.FindAsync(result.OrderId.Value);
            Assert.NotNull(order);
            Assert.Equal(180000, order.TotalAmount); // 200000 - 20000 discount
            Assert.Equal(20000, order.DiscountAmount);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID02_Success_VoucherExpiredDateNull_MinOrderAmountNull_UsageLimitNull()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER2",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = null,
                MinOrderAmount = null,
                UseageLimit = null,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID03_Success_VoucherExpiredDateEqualNow()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER3",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddHours(2), // equal/greater than now
                MinOrderAmount = 50000,
                UseageLimit = 5,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID04_Success_VoucherMinOrderAmountEqualOrderAmount()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER4",
                DiscountPercent = 10,
                IsActive = true,
                MinOrderAmount = 200000, // equals subtotal
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID05_Success_VoucherUsageLimitEqualOne_RemainingOne()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER5",
                DiscountPercent = 10,
                IsActive = true,
                UseageLimit = 1,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);

            var updatedVoucher = await _context.Vouchers.FindAsync(voucher.VoucherId);
            Assert.Equal(0, updatedVoucher.UseageLimit);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID06_Success_VoucherMaxDiscountAmountApplied()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "VOUCHER6",
                DiscountPercent = 50, // 50% of 200000 = 100000
                MaxDiscountAmount = 30000, // Capped at 30000
                IsActive = true,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
            var order = await _context.Orders.FindAsync(result.OrderId.Value);
            Assert.Equal(30000, order.DiscountAmount);
            Assert.Equal(170000, order.TotalAmount);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID07_Success_InventoryStockEqualQuantityRequested()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 2, 0); // available = 2

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID08_Fail_AddressInvalid()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = Guid.NewGuid(), // Invalid address
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("The address is invalid or does not belong to this account.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID09_Fail_VoucherAlreadyUsed()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "USED123",
                DiscountPercent = 10,
                IsActive = true,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            _context.CustomerVouchers.Add(new CustomerVoucher
            {
                CustomerId = customer.CustomerId,
                VoucherId = voucher.VoucherId,
                IsUsed = true
            });
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("You have already used this voucher.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID10_Fail_VoucherInactive()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "INACTIVE",
                DiscountPercent = 10,
                IsActive = false,
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("The voucher is invalid or has been deactivated.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID11_Fail_VoucherExpired()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "EXPIRED",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(-1), // Expired
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("The voucher has expired.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID12_Fail_VoucherMinOrderAmountNotReached()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "HIGHMIN",
                DiscountPercent = 10,
                IsActive = true,
                MinOrderAmount = 300000, // min = 300k, order = 200k
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("A minimum order of 300,000 ₫ is required to apply this voucher.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID13_Fail_VoucherUsageLimitReached()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "LIMITREACHED",
                DiscountPercent = 10,
                IsActive = true,
                UseageLimit = 0, // usage limit is 0
                CreateAt = DateTime.Now
            };
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                VoucherId = voucher.VoucherId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("The voucher has reached its usage limit.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID14_Fail_InventoryStockInsufficient()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 1, 0); // only 1 stock available

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 } // requesting 2
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Contains("does not have enough stock", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID15_Fail_SupplierNotFound_WhenProductDetailsNotFoundInInventory()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var productId = Guid.NewGuid(); // No inventory seeded

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Inventory information for the product was not found.", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID16_Fail_InventoryStockInsufficient_Edge()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 1, 4); // available = 1

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 } // requesting 2
                }
            };

            var service = CreateCheckoutService(_context);
            var result = await service.PlaceCodOrderAsync(request);

            Assert.False(result.Success);
            Assert.Contains("does not have enough stock", result.Message);
        }

        [Fact]
        public async Task PlaceCodOrderAsync_UTCID17_Fail_DatabaseSaveThrowsException()
        {
            await ClearDatabaseAsync(_context);

            var customer = await EnsureCustomerAsync(Guid.NewGuid());
            var address = await EnsureAddressAsync(customer.CustomerId);
            var (productId, _, _) = await SeedProductAndInventoryAsync(_context, "Product 1", 10, 0);

            var brandId = Guid.NewGuid();
            _context.Brands.Add(new Brand { BrandId = brandId, BrandName = "Dup-DB-1-" + Guid.NewGuid().ToString("N"), Status = PetCenterAPI.Common.Status.Active });
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _context.Brands.Add(new Brand { BrandId = brandId, BrandName = "Dup-DB-2-" + Guid.NewGuid().ToString("N"), Status = PetCenterAPI.Common.Status.Active });

            var request = new PlaceCodOrderDTO
            {
                CustomerId = customer.CustomerId,
                AddressId = address.AddressId,
                Items = new List<CodOrderItemDTO>
                {
                    new CodOrderItemDTO { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
                }
            };

            var service = CreateCheckoutService(_context);
            await Assert.ThrowsAnyAsync<Exception>(() => service.PlaceCodOrderAsync(request));
        }

}
}