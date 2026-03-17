using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;

namespace OrdersAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly PetCenterCartServiceContext _context;

        public CartRepository(PetCenterCartServiceContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByCustomerIdAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Cart> CreateCartAsync(Guid customerId)
        {
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                CustomerId = customerId
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartDetail?> GetCartDetailByIdAsync(Guid cartDetailId)
        {
            return await _context.CartDetails.FindAsync(cartDetailId);
        }

        public async Task<CartDetail?> GetCartDetailByProductAsync(Guid cartId, Guid productId)
        {
            return await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.ProductId == productId);
        }

        public async Task AddCartDetailAsync(CartDetail cartDetail)
        {
            await _context.CartDetails.AddAsync(cartDetail);
        }

        public Task UpdateCartDetailAsync(CartDetail cartDetail)
        {
            _context.CartDetails.Update(cartDetail);
            return Task.CompletedTask;
        }

        public Task DeleteCartDetailAsync(CartDetail cartDetail)
        {
            _context.CartDetails.Remove(cartDetail);
            return Task.CompletedTask;
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            var details = await _context.CartDetails
                .Where(cd => cd.CartId == cartId)
                .ToListAsync();
            _context.CartDetails.RemoveRange(details);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}