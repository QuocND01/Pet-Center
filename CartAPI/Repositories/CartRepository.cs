using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Repositories;

public class CartRepository : ICartRepository
{
    private readonly PetCenterCartServiceContext _db;

    public CartRepository(PetCenterCartServiceContext db)
    {
        _db = db;
    }

    
    public async Task<Cart?> GetByIdAsync(Guid cartId)
        => await _db.Carts
            .Include(c => c.CartDetails)
            .FirstOrDefaultAsync(c => c.CartId == cartId);

    
    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId)
        => await _db.Carts
            .Include(c => c.CartDetails)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

    
    public async Task<Cart> CreateAsync(Cart cart)
    {
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    
    public async Task<bool> ExistsAsync(Guid cartId)
        => await _db.Carts.AnyAsync(c => c.CartId == cartId);

    
    public async Task<bool> ExistsByCustomerIdAsync(Guid customerId)
        => await _db.Carts.AnyAsync(c => c.CustomerId == customerId);
}