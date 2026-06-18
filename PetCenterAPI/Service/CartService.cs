using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Cart;
using PetCenterAPI.DTOs.Responses.Cart;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        // ============================================================
        // VIEW CART
        // ============================================================
        public async Task<CartResponseDTO> GetCartAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetOrCreateCartAsync(customerId);

            return new CartResponseDTO
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CartDetails = cart.CartDetails
                    .Select(d => new CartDetailResponseDTO
                    {
                        CartDetailId = d.CartDetailsId,
                        CartId = d.CartId,
                        ProductId = d.ProductId,
                        Quantity = d.Quantity ?? 0
                    })
                    .ToList()
            };
        }

        // ============================================================
        // ADD TO CART
        // ============================================================
        public async Task<(bool Success, string Message)> AddToCartAsync(Guid customerId, AddToCartRequestDTO request)
        {
            if (request.ProductId == Guid.Empty)
                return (false, "Product is required.");

            if (request.Quantity < 1)
                return (false, "Quantity must be at least 1.");

            var product = await _cartRepository.GetProductAsync(request.ProductId);
            if (product == null)
                return (false, "Product not found.");

            if (product.Status != Status.Active)
                return (false, "This product is currently not available.");

            var stock = await _cartRepository.GetAvailableStockAsync(request.ProductId);
            if (stock <= 0)
                return (false, "This product is out of stock.");

            var cart = await _cartRepository.GetOrCreateCartAsync(customerId);
            var existing = await _cartRepository.GetDetailByProductAsync(cart.CartId, request.ProductId);

            var currentQty = existing?.Quantity ?? 0;
            var newQty = currentQty + request.Quantity;

            if (newQty > stock)
                return (false, currentQty > 0
                    ? $"Only {stock} item(s) in stock. You already have {currentQty} in your cart."
                    : $"Only {stock} item(s) in stock.");

            if (existing != null)
            {
                existing.Quantity = newQty;
            }
            else
            {
                await _cartRepository.AddDetailAsync(new CartDetail
                {
                    CartDetailsId = Guid.NewGuid(),
                    CartId = cart.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            await _cartRepository.SaveChangesAsync();
            return (true, "Added to cart successfully.");
        }

        // ============================================================
        // UPDATE QUANTITY
        // ============================================================
        public async Task<(bool Success, string Message)> UpdateDetailAsync(
            Guid customerId, Guid cartDetailId, UpdateCartDetailRequestDTO request)
        {
            if (request.Quantity < 1)
                return (false, "Quantity must be at least 1.");

            var detail = await _cartRepository.GetDetailByIdAsync(cartDetailId);
            if (detail == null)
                return (false, "Cart item not found.");

            if (detail.Cart.CustomerId != customerId)
                return (false, "You are not allowed to modify this cart item.");

            var product = await _cartRepository.GetProductAsync(detail.ProductId);
            if (product == null || product.Status != Status.Active)
                return (false, "This product is currently not available.");

            var stock = await _cartRepository.GetAvailableStockAsync(detail.ProductId);
            if (request.Quantity > stock)
                return (false, $"Only {stock} item(s) in stock.");

            detail.Quantity = request.Quantity;
            await _cartRepository.SaveChangesAsync();
            return (true, "Cart updated successfully.");
        }

        // ============================================================
        // REMOVE ONE ITEM
        // ============================================================
        public async Task<(bool Success, string Message)> DeleteDetailAsync(Guid customerId, Guid cartDetailId)
        {
            var detail = await _cartRepository.GetDetailByIdAsync(cartDetailId);
            if (detail == null)
                return (false, "Cart item not found.");

            if (detail.Cart.CustomerId != customerId)
                return (false, "You are not allowed to modify this cart item.");

            _cartRepository.RemoveDetail(detail);
            await _cartRepository.SaveChangesAsync();
            return (true, "Item removed from cart.");
        }

        // ============================================================
        // CLEAR CART
        // ============================================================
        public async Task<(bool Success, string Message)> ClearCartAsync(Guid customerId)
        {
            var cart = await _cartRepository.GetCartWithDetailsAsync(customerId);
            if (cart == null || cart.CartDetails.Count == 0)
                return (true, "Your cart is already empty.");

            _cartRepository.RemoveDetails(cart.CartDetails);
            await _cartRepository.SaveChangesAsync();
            return (true, "Cart cleared successfully.");
        }
    }
}
