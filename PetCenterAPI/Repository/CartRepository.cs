using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly PetCenterContext _context;

        public CartRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // CART — READ
        // ============================================================
        public async Task<Cart?> GetCartWithDetailsAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Cart> GetOrCreateCartAsync(Guid customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart != null) return cart;

            cart = new Cart
            {
                CartId = Guid.NewGuid(),
                CustomerId = customerId
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        // ============================================================
        // CART DETAILS
        // ============================================================
        public async Task<CartDetail?> GetDetailByIdAsync(Guid cartDetailId)
        {
            return await _context.CartDetails
                .Include(d => d.Cart)
                .FirstOrDefaultAsync(d => d.CartDetailsId == cartDetailId);
        }

        public async Task<CartDetail?> GetDetailByProductAsync(Guid cartId, Guid productId)
        {
            return await _context.CartDetails
                .FirstOrDefaultAsync(d => d.CartId == cartId && d.ProductId == productId);
        }

        public async Task AddDetailAsync(CartDetail detail)
        {
            await _context.CartDetails.AddAsync(detail);
        }

        public void RemoveDetail(CartDetail detail)
        {
            _context.CartDetails.Remove(detail);
        }

        public void RemoveDetails(IEnumerable<CartDetail> details)
        {
            _context.CartDetails.RemoveRange(details);
        }

        // ============================================================
        // PRODUCT / STOCK
        // ============================================================
        public async Task<Product?> GetProductAsync(Guid productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<int> GetAvailableStockAsync(Guid productId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);
            return inventory?.QuantityAvailable ?? 0;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
