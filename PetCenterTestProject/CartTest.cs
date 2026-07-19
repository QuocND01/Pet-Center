using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Cart;
using PetCenterAPI.DTOs.Responses.Cart;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;

namespace PetCenterTestProject
{
    public class CartTest
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly CartService _service;

        public CartTest()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _service = new CartService(_cartRepositoryMock.Object);
        }

        // =========================================================================
        // SECTION 1: GetCartAsync() Unit Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetCartAsync_ReturnsMappedCartWithItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            var cart = new Cart
            {
                CartId = cartId,
                CustomerId = customerId,
                CartDetails = new List<CartDetail>
                {
                    new CartDetail { CartDetailsId = Guid.NewGuid(), CartId = cartId, ProductId = productId1, Quantity = 2 },
                    new CartDetail { CartDetailsId = Guid.NewGuid(), CartId = cartId, ProductId = productId2, Quantity = 5 }
                }
            };

            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);

            // Act
            var result = await _service.GetCartAsync(customerId);

            // Assert
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
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = cartId,
                CustomerId = customerId,
                CartDetails = new List<CartDetail>()
            };

            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);

            // Act
            var result = await _service.GetCartAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cartId, result.CartId);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Empty(result.CartDetails);
        }

        [Fact]
        public async Task UTCID03_GetCartAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetCartAsync(customerId));
        }

        // =========================================================================
        // SECTION 2: AddToCartAsync() Unit Tests (UTCID01 - UTCID15)
        // =========================================================================

        [Fact]
        public async Task UTCID01_AddToCartAsync_Success_NewProduct()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 3 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId, CartDetails = new List<CartDetail>() };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);

            _cartRepositoryMock.Verify(repo => repo.AddDetailAsync(It.Is<CartDetail>(d =>
                d.CartId == cartId &&
                d.ProductId == productId &&
                d.Quantity == 3
            )), Times.Once);
            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_AddToCartAsync_Success_ExistingProduct()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };
            var existingDetail = new CartDetail { CartId = cartId, ProductId = productId, Quantity = 3 };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync(existingDetail);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);
            Assert.Equal(5, existingDetail.Quantity); // 3 + 2

            _cartRepositoryMock.Verify(repo => repo.AddDetailAsync(It.IsAny<CartDetail>()), Times.Never);
            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID03_AddToCartAsync_Success_NewProduct_QuantityOne()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 1 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);
        }

        [Fact]
        public async Task UTCID04_AddToCartAsync_Success_ExistingProduct_QuantityEqualsStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 7 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };
            var existingDetail = new CartDetail { CartId = cartId, ProductId = productId, Quantity = 3 };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Total stock = 10
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync(existingDetail);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Added to cart successfully.", result.Message);
            Assert.Equal(10, existingDetail.Quantity); // New quantity 3 + 7 = 10 (equals stock)
        }

        [Fact]
        public async Task UTCID05_AddToCartAsync_Fail_ProductIdEmpty()
        {
            // Arrange
            var request = new AddToCartRequestDTO { ProductId = Guid.Empty, Quantity = 1 };

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product is required.", result.Message);
        }

        [Fact]
        public async Task UTCID06_AddToCartAsync_Fail_QuantityZero()
        {
            // Arrange
            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 0 };

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID07_AddToCartAsync_Fail_QuantityNegative()
        {
            // Arrange
            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = -5 };

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID08_AddToCartAsync_Fail_ProductNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product not found.", result.Message);
        }

        [Fact]
        public async Task UTCID09_AddToCartAsync_Fail_ProductInactive()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };
            var product = new Product { ProductId = productId, Status = Status.Inactive }; // Inactive

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID10_AddToCartAsync_Fail_ProductOutOfStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 2 };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(0); // Out of stock

            // Act
            var result = await _service.AddToCartAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This product is out of stock.", result.Message);
        }

        [Fact]
        public async Task UTCID11_AddToCartAsync_Fail_NewProduct_QuantityExceedsStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 15 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Stock 10
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID12_AddToCartAsync_Fail_NewProduct_QuantityEqualsStockPlusOne()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 11 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Stock 10
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID13_AddToCartAsync_Fail_ExistingProduct_TotalQuantityExceedsStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 8 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };
            var existingDetail = new CartDetail { CartId = cartId, ProductId = productId, Quantity = 3 }; // Total would be 11

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Stock 10
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync(existingDetail);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock. You already have 3 in your cart.", result.Message);
        }

        [Fact]
        public async Task UTCID14_AddToCartAsync_Fail_ExistingProduct_TotalQuantityEqualsStockPlusOne()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = productId, Quantity = 8 };

            var product = new Product { ProductId = productId, Status = Status.Active };
            var cart = new Cart { CartId = cartId, CustomerId = customerId };
            var existingDetail = new CartDetail { CartId = cartId, ProductId = productId, Quantity = 3 }; // Total would be 11 (stock 10 + 1)

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);
            _cartRepositoryMock.Setup(repo => repo.GetOrCreateCartAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.GetDetailByProductAsync(cartId, productId)).ReturnsAsync(existingDetail);

            // Act
            var result = await _service.AddToCartAsync(customerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock. You already have 3 in your cart.", result.Message);
        }

        [Fact]
        public async Task UTCID15_AddToCartAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 2 };

            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddToCartAsync(customerId, request));
        }

        // =========================================================================
        // SECTION 3: UpdateDetailAsync() Unit Tests (UTCID01 - UTCID12)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateDetailAsync_Success_QuantityLessThanStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 5 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart, Quantity = 2 };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Stock 10

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);
            Assert.Equal(5, detail.Quantity);

            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_UpdateDetailAsync_Success_QuantityOne()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 1 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart, Quantity = 4 };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);
            Assert.Equal(1, detail.Quantity);
        }

        [Fact]
        public async Task UTCID03_UpdateDetailAsync_Success_QuantityEqualsStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 10 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart, Quantity = 2 };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10); // Stock 10

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cart updated successfully.", result.Message);
            Assert.Equal(10, detail.Quantity);
        }

        [Fact]
        public async Task UTCID04_UpdateDetailAsync_Fail_QuantityZero()
        {
            // Arrange
            var request = new UpdateCartDetailRequestDTO { Quantity = 0 };

            // Act
            var result = await _service.UpdateDetailAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateDetailAsync_Fail_QuantityNegative()
        {
            // Arrange
            var request = new UpdateCartDetailRequestDTO { Quantity = -2 };

            // Act
            var result = await _service.UpdateDetailAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Quantity must be at least 1.", result.Message);
        }

        [Fact]
        public async Task UTCID06_UpdateDetailAsync_Fail_CartItemNotFound()
        {
            // Arrange
            var cartDetailId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.UpdateDetailAsync(Guid.NewGuid(), cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cart item not found.", result.Message);
        }

        [Fact]
        public async Task UTCID07_UpdateDetailAsync_Fail_NotCartOwner()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var differentCustomerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var cart = new Cart { CustomerId = differentCustomerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, Cart = cart };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not allowed to modify this cart item.", result.Message);
        }

        [Fact]
        public async Task UTCID08_UpdateDetailAsync_Fail_ProductNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID09_UpdateDetailAsync_Fail_ProductInactive()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart };
            var product = new Product { ProductId = productId, Status = Status.Inactive }; // Inactive

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This product is currently not available.", result.Message);
        }

        [Fact]
        public async Task UTCID10_UpdateDetailAsync_Fail_QuantityExceedsStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 15 }; // Exceeds 10

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID11_UpdateDetailAsync_Fail_QuantityEqualsStockPlusOne()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 11 }; // 10 + 1

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, ProductId = productId, Cart = cart };
            var product = new Product { ProductId = productId, Status = Status.Active };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.GetProductAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.GetAvailableStockAsync(productId)).ReturnsAsync(10);

            // Act
            var result = await _service.UpdateDetailAsync(customerId, cartDetailId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only 10 item(s) in stock.", result.Message);
        }

        [Fact]
        public async Task UTCID12_UpdateDetailAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();
            var request = new UpdateCartDetailRequestDTO { Quantity = 3 };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ThrowsAsync(new Exception("Database save error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateDetailAsync(customerId, cartDetailId, request));
        }

        // =========================================================================
        // SECTION 4: DeleteDetailAsync() Unit Tests (UTCID01 - UTCID04)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteDetailAsync_Success()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, Cart = cart };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);

            // Act
            var result = await _service.DeleteDetailAsync(customerId, cartDetailId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Item removed from cart.", result.Message);

            _cartRepositoryMock.Verify(repo => repo.RemoveDetail(detail), Times.Once);
            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_DeleteDetailAsync_Fail_CartItemNotFound()
        {
            // Arrange
            var cartDetailId = Guid.NewGuid();
            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync((CartDetail?)null);

            // Act
            var result = await _service.DeleteDetailAsync(Guid.NewGuid(), cartDetailId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cart item not found.", result.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteDetailAsync_Fail_NotCartOwner()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var differentCustomerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();

            var cart = new Cart { CustomerId = differentCustomerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, Cart = cart };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);

            // Act
            var result = await _service.DeleteDetailAsync(customerId, cartDetailId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not allowed to modify this cart item.", result.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteDetailAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cartDetailId = Guid.NewGuid();

            var cart = new Cart { CustomerId = customerId };
            var detail = new CartDetail { CartDetailsId = cartDetailId, Cart = cart };

            _cartRepositoryMock.Setup(repo => repo.GetDetailByIdAsync(cartDetailId)).ReturnsAsync(detail);
            _cartRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteDetailAsync(customerId, cartDetailId));
        }

        // =========================================================================
        // SECTION 5: ClearCartAsync() Unit Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_ClearCartAsync_Success_CartWithItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cart = new Cart
            {
                CustomerId = customerId,
                CartDetails = new List<CartDetail>
                {
                    new CartDetail { ProductId = Guid.NewGuid(), Quantity = 2 },
                    new CartDetail { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            _cartRepositoryMock.Setup(repo => repo.GetCartWithDetailsAsync(customerId)).ReturnsAsync(cart);

            // Act
            var result = await _service.ClearCartAsync(customerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cart cleared successfully.", result.Message);

            _cartRepositoryMock.Verify(repo => repo.RemoveDetails(cart.CartDetails), Times.Once);
            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_ClearCartAsync_Success_CartAlreadyEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Setup case where GetCartWithDetailsAsync returns null or has empty details
            _cartRepositoryMock.Setup(repo => repo.GetCartWithDetailsAsync(customerId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _service.ClearCartAsync(customerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Your cart is already empty.", result.Message);

            _cartRepositoryMock.Verify(repo => repo.RemoveDetails(It.IsAny<IEnumerable<CartDetail>>()), Times.Never);
            _cartRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UTCID03_ClearCartAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var cart = new Cart
            {
                CustomerId = customerId,
                CartDetails = new List<CartDetail>
                {
                    new CartDetail { ProductId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            _cartRepositoryMock.Setup(repo => repo.GetCartWithDetailsAsync(customerId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception("Database save error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ClearCartAsync(customerId));
        }
    }
}
