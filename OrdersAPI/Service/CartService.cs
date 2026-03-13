using OrdersAPI.DTOs;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartResponseDTO?> GetCartByCustomerIdAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null) return null;

            return new CartResponseDTO
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CartDetails = cart.CartDetails.Select(cd => new CartDetailResponseDTO
                {
                    CartDetailId = cd.CartDetailId,
                    CartId = cd.CartId,
                    ProductId = cd.ProductId,
                    Quantity = cd.Quantity
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> AddToCartAsync(AddToCartRequestDTO dto)
        {
            if (dto.Quantity <= 0)
                return (false, "Quantity must be greater than 0.");

            var cart = await _cartRepository.GetCartByCustomerIdAsync(dto.CustomerId);
            if (cart == null)
                cart = await _cartRepository.CreateCartAsync(dto.CustomerId);

            var existingDetail = await _cartRepository.GetCartDetailByProductAsync(cart.CartId, dto.ProductId);

            if (existingDetail != null)
            {
                existingDetail.Quantity += dto.Quantity;
                await _cartRepository.UpdateCartDetailAsync(existingDetail);
            }
            else
            {
                var newDetail = new CartDetail
                {
                    CartDetailId = Guid.NewGuid(),
                    CartId = cart.CartId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                await _cartRepository.AddCartDetailAsync(newDetail);
            }

            await _cartRepository.SaveChangesAsync();
            return (true, "Product added to cart successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateCartDetailAsync(Guid cartDetailId, int quantity)
        {
            if (quantity <= 0)
                return (false, "Quantity must be greater than 0.");

            var detail = await _cartRepository.GetCartDetailByIdAsync(cartDetailId);
            if (detail == null)
                return (false, "Cart item not found.");

            detail.Quantity = quantity;
            await _cartRepository.UpdateCartDetailAsync(detail);
            await _cartRepository.SaveChangesAsync();
            return (true, "Cart item updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteCartDetailAsync(Guid cartDetailId)
        {
            var detail = await _cartRepository.GetCartDetailByIdAsync(cartDetailId);
            if (detail == null)
                return (false, "Cart item not found.");

            await _cartRepository.DeleteCartDetailAsync(detail);
            await _cartRepository.SaveChangesAsync();
            return (true, "Item removed from cart successfully.");
        }

        public async Task<(bool Success, string Message)> ClearCartAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
                return (false, "Cart not found.");

            await _cartRepository.ClearCartAsync(cart.CartId);
            await _cartRepository.SaveChangesAsync();
            return (true, "Cart cleared successfully.");
        }
    }
}