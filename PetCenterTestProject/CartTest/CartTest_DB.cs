using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Cart;
using PetCenterAPI.DTOs.Responses.Cart;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;

namespace PetCenterTestProject.CartTest
{
    [Collection("DatabaseTests")]
    public class CartTest_DB
    {
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=.;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;",
                    builder => builder.EnableRetryOnFailure())
                .Options;

            return new PetCenterContext(options);
        }

        private CartRepository CreateRepository(PetCenterContext context)
        {
            return new CartRepository(context);
        }

        private CartService CreateService(PetCenterContext context)
        {
            return new CartService(CreateRepository(context));
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
            context.MedicalRecords.RemoveRange(context.MedicalRecords);
            context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
            context.AppointmentServices.RemoveRange(context.AppointmentServices);
            context.Appointments.RemoveRange(context.Appointments);
            context.FeedbackImages.RemoveRange(context.FeedbackImages);
            context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
            context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Payments.RemoveRange(context.Payments);
            context.Orders.RemoveRange(context.Orders);
            context.Pets.RemoveRange(context.Pets);
            context.CartDetails.RemoveRange(context.CartDetails);
            context.Carts.RemoveRange(context.Carts);
            context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
            context.OtpCodes.RemoveRange(context.OtpCodes);
            context.Addresses.RemoveRange(context.Addresses);
            context.Customers.RemoveRange(context.Customers);
            context.ImportStockDetails.RemoveRange(context.ImportStockDetails);
            context.ImportStocks.RemoveRange(context.ImportStocks);
            context.Inventories.RemoveRange(context.Inventories);
            context.Products.RemoveRange(context.Products);
            context.Brands.RemoveRange(context.Brands);
            context.Categories.RemoveRange(context.Categories);
            context.VetFeedbacks.RemoveRange(context.VetFeedbacks);
            context.VetProfiles.RemoveRange(context.VetProfiles);
            context.Set<Dictionary<string, object>>("StaffRole").RemoveRange(context.Set<Dictionary<string, object>>("StaffRole"));
            context.Staffs.RemoveRange(context.Staffs);
            context.Roles.RemoveRange(context.Roles);
            await context.SaveChangesAsync();
        }

        private async Task EnsureBrandExistsAsync(PetCenterContext context, Guid brandId)
        {
            if (!await context.Brands.AnyAsync(b => b.BrandId == brandId))
            {
                context.Brands.Add(new Brand
                {
                    BrandId = brandId,
                    BrandName = "Test Brand",
                    Status = Status.Active
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureCategoryExistsAsync(PetCenterContext context, Guid categoryId)
        {
            if (!await context.Categories.AnyAsync(c => c.CategoryId == categoryId))
            {
                context.Categories.Add(new Category
                {
                    CategoryId = categoryId,
                    CategoryName = "Test Category",
                    Status = Status.Active
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureProductExistsAsync(PetCenterContext context, Guid productId, Status status = Status.Active, int stockQuantity = 10)
        {
            var brandId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var categoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            await EnsureBrandExistsAsync(context, brandId);
            await EnsureCategoryExistsAsync(context, categoryId);

            var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                product = new Product
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    ProductPrice = 100,
                    BrandId = brandId,
                    CategoryId = categoryId,
                    Status = status
                };
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }
            else
            {
                product.Status = status;
                await context.SaveChangesAsync();
            }

            var inventory = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
            {
                context.Inventories.Add(new Inventory
                {
                    InventoryId = Guid.NewGuid(),
                    ProductId = productId,
                    SKU = "TEST-SKU-" + productId.ToString().Substring(0, 8),
                    QuantityAvailable = stockQuantity,
                    LastUpdated = DateTime.UtcNow
                });
            }
            else
            {
                inventory.QuantityAvailable = stockQuantity;
            }
            await context.SaveChangesAsync();
        }

        private async Task EnsureCustomerExistsAsync(PetCenterContext context, Guid customerId)
        {
            if (!await context.Customers.AnyAsync(c => c.CustomerId == customerId))
            {
                context.Customers.Add(new Customer
                {
                    CustomerId = customerId,
                    FullName = "Test Customer",
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureCartExistsAsync(PetCenterContext context, Guid cartId, Guid customerId)
        {
            await EnsureCustomerExistsAsync(context, customerId);
            if (!await context.Carts.AnyAsync(c => c.CartId == cartId))
            {
                context.Carts.Add(new Cart
                {
                    CartId = cartId,
                    CustomerId = customerId
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureCartDetailExistsAsync(PetCenterContext context, Guid cartDetailId, Guid cartId, Guid productId, int quantity)
        {
            await EnsureProductExistsAsync(context, productId);
            if (!await context.CartDetails.AnyAsync(d => d.CartDetailsId == cartDetailId))
            {
                context.CartDetails.Add(new CartDetail
                {
                    CartDetailsId = cartDetailId,
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity
                });
                await context.SaveChangesAsync();
            }
        }

        // =========================================================================
        // SECTION 1: GetCartAsync() Integration Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetCartAsync_ReturnsMappedCartWithItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId1, 2);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId2, 5);

            var service = CreateService(context);
            var result = await service.GetCartAsync(customerId);

            Assert.NotNull(result);
            Assert.Equal(cartId, result.CartId);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(2, result.CartDetails.Count);

            var firstItem = result.CartDetails.First(d => d.ProductId == productId1);
            Assert.Equal(2, firstItem.Quantity);

            var secondItem = result.CartDetails.First(d => d.ProductId == productId2);
            Assert.Equal(5, secondItem.Quantity);
        }

        [Fact]
        public async Task UTCID02_GetCartAsync_ReturnsCartWithEmptyItemsList_WhenNoCartExists()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            await EnsureCustomerExistsAsync(context, customerId);

            var service = CreateService(context);
            var result = await service.GetCartAsync(customerId);

            Assert.NotNull(result);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Empty(result.CartDetails);
        }

        [Fact]
        public async Task UTCID03_GetCartAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();
            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ThrowsAsync(new Exception("Database error"));

            var service = new CartService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetCartAsync(customerId));
        }

        // =========================================================================
        // SECTION 2: AddToCartAsync() Integration Tests (UTCID01 - UTCID15)
        // =========================================================================

        [Fact]
        public async Task UTCID01_AddToCartAsync_Success_NewProduct()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 3 };

            await EnsureCustomerExistsAsync(context, customerId);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);

            var cartDetail = await context.CartDetails.FirstOrDefaultAsync(d => d.ProductId == productId);
            Assert.NotNull(cartDetail);
            Assert.Equal(3, cartDetail.Quantity);
        }

        [Fact]
        public async Task UTCID02_AddToCartAsync_Success_ExistingProduct()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId, 3);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10); // Update stock

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);

            var cartDetail = await context.CartDetails.FirstOrDefaultAsync(d => d.CartId == cartId && d.ProductId == productId);
            Assert.NotNull(cartDetail);
            Assert.Equal(5, cartDetail.Quantity);
        }

        [Fact]
        public async Task UTCID03_AddToCartAsync_Success_NewProduct_QuantityOne()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 1 };

            await EnsureCustomerExistsAsync(context, customerId);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);
        }

        [Fact]
        public async Task UTCID04_AddToCartAsync_Success_ExistingProduct_QuantityEqualsStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 7 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId, 3);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10); // Stock = 10

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);

            var cartDetail = await context.CartDetails.FirstOrDefaultAsync(d => d.CartId == cartId && d.ProductId == productId);
            Assert.NotNull(cartDetail);
            Assert.Equal(10, cartDetail.Quantity);
        }

        [Fact]
        public async Task UTCID05_AddToCartAsync_Fail_ProductIdEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new AddToCartRequestDTO { ProductId = Guid.Empty, Quantity = 1 };

            var service = CreateService(context);
            var result = await service.AddToCartAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Product is required.", result.Message);
        }

        [Fact]
        public async Task UTCID06_AddToCartAsync_Fail_QuantityZero()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 0 };

            var service = CreateService(context);
            var result = await service.AddToCartAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID07_AddToCartAsync_Fail_QuantityNegative()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = -5 };

            var service = CreateService(context);
            var result = await service.AddToCartAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID08_AddToCartAsync_Fail_ProductNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 2 };

            var service = CreateService(context);
            var result = await service.AddToCartAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Product not found.", result.Message);
        }

        [Fact]
        public async Task UTCID09_AddToCartAsync_Fail_ProductInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };

            await EnsureProductExistsAsync(context, productId, Status.Inactive, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID10_AddToCartAsync_Fail_ProductOutOfStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };

            await EnsureCustomerExistsAsync(context, customerId);
            await EnsureProductExistsAsync(context, productId, Status.Active, 0);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.False(result.Success);
            Assert.Equal("This product is out of stock.", result.Message);
        }

        [Fact]
        public async Task UTCID11_AddToCartAsync_Fail_NewProduct_QuantityExceedsStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 15 };

            await EnsureCustomerExistsAsync(context, customerId);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID12_AddToCartAsync_Fail_NewProduct_QuantityEqualsStockPlusOne()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 11 };

            await EnsureCustomerExistsAsync(context, customerId);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID13_AddToCartAsync_Fail_ExistingProduct_TotalQuantityExceedsStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 8 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId, 3);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10); // Update stock to 10

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock. You already have 3 in your cart.", result.Message);
        }

        [Fact]
        public async Task UTCID14_AddToCartAsync_Fail_ExistingProduct_TotalQuantityEqualsStockPlusOne()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 8 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId, 3);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.AddToCartAsync(customerId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock. You already have 3 in your cart.", result.Message);
        }

        [Fact]
        public async Task UTCID15_AddToCartAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 2 };

            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetProductAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Database save failed"));

            var service = new CartService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.AddToCartAsync(customerId, request));
        }

        // =========================================================================
        // SECTION 3: UpdateDetailAsync() Integration Tests (UTCID01 - UTCID12)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateDetailAsync_Success_QuantityLessThanStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 5 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);

            var cartDetail = await context.CartDetails.FindAsync(cartDetailId);
            Assert.NotNull(cartDetail);
            Assert.Equal(5, cartDetail.Quantity);
        }

        [Fact]
        public async Task UTCID02_UpdateDetailAsync_Success_QuantityOne()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 1 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);
        }

        [Fact]
        public async Task UTCID03_UpdateDetailAsync_Success_QuantityEqualsStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 10 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);
        }

        [Fact]
        public async Task UTCID04_UpdateDetailAsync_Fail_QuantityZero()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new UpdateCartDetailRequestDTO { Quantity = 0 };

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateDetailAsync_Fail_QuantityNegative()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new UpdateCartDetailRequestDTO { Quantity = -2 };

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID06_UpdateDetailAsync_Fail_CartItemNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Cart item not found.", result.Message);
        }

        [Fact]
        public async Task UTCID07_UpdateDetailAsync_Fail_NotCartOwner()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            await EnsureCartExistsAsync(context, cartId, customerId1);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId2, cartDetailId, request);

            Assert.False(result.Success);
            Assert.Equal("You are not allowed to modify this cart item.", result.Message);
        }

        [Fact]
        public async Task UTCID08_UpdateDetailAsync_Fail_ProductNotFound()
        {
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart };

            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            repoMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync((Product?)null);

            var service = new CartService(repoMock.Object);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID09_UpdateDetailAsync_Fail_ProductInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Inactive, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID10_UpdateDetailAsync_Fail_QuantityExceedsStock()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 15 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID11_UpdateDetailAsync_Fail_QuantityEqualsStockPlusOne()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 11 };

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);
            await EnsureProductExistsAsync(context, productId, Status.Active, 10);

            var service = CreateService(context);
            var result = await service.UpdateDetailAsync(customerId, cartDetailId, request);

            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID12_UpdateDetailAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new CartService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.UpdateDetailAsync(customerId, cartDetailId, request));
        }

        // =========================================================================
        // SECTION 4: DeleteDetailAsync() Integration Tests (UTCID01 - UTCID04)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteDetailAsync_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);

            var service = CreateService(context);
            var result = await service.DeleteDetailAsync(customerId, cartDetailId);

            Assert.True(result.Success);
            Assert.Equal("Item removed from cart.", result.Message);

            var detail = await context.CartDetails.FindAsync(cartDetailId);
            Assert.Null(detail);
        }

        [Fact]
        public async Task UTCID02_DeleteDetailAsync_Fail_CartItemNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var result = await service.DeleteDetailAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Cart item not found.", result.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteDetailAsync_Fail_NotCartOwner()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            await EnsureCartExistsAsync(context, cartId, customerId1);
            await EnsureCartDetailExistsAsync(context, cartDetailId, cartId, productId, 2);

            var service = CreateService(context);
            var result = await service.DeleteDetailAsync(customerId2, cartDetailId);

            Assert.False(result.Success);
            Assert.Equal("You are not allowed to modify this cart item.", result.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteDetailAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();

            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new CartService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.DeleteDetailAsync(customerId, cartDetailId));
        }

        // =========================================================================
        // SECTION 5: ClearCartAsync() Integration Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_ClearCartAsync_Success_CartWithItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            await EnsureCartExistsAsync(context, cartId, customerId);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId1, 2);
            await EnsureCartDetailExistsAsync(context, Guid.NewGuid(), cartId, productId2, 4);

            var service = CreateService(context);
            var result = await service.ClearCartAsync(customerId);

            Assert.True(result.Success);
            Assert.Equal("Cart cleared successfully.", result.Message);

            var details = await context.CartDetails.Where(d => d.CartId == cartId).ToListAsync();
            Assert.Empty(details);
        }

        [Fact]
        public async Task UTCID02_ClearCartAsync_Success_CartAlreadyEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();

            await EnsureCartExistsAsync(context, cartId, customerId);

            var service = CreateService(context);
            var result = await service.ClearCartAsync(customerId);

            Assert.True(result.Success);
            Assert.Equal("Your cart is already empty.", result.Message);
        }

        [Fact]
        public async Task UTCID03_ClearCartAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();

            var repoMock = new Mock<ICartRepository>();
            repoMock.Setup(repo => repo.GetCartWithDetailsAsync(customerId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new CartService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.ClearCartAsync(customerId));
        }
    }
}
