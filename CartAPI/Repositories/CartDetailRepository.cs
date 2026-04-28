using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Repositories;

public class CartDetailRepository : ICartDetailRepository
{
    private readonly PetCenterCartServiceContext _db;

    public CartDetailRepository(PetCenterCartServiceContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<CartDetail?> GetByCartAndProductAsync(Guid cartId, Guid productId)
        => await _db.CartDetails
            .FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.ProductId == productId);

    /// <inheritdoc/>
    public async Task<CartDetail?> GetByIdAsync(Guid cartDetailsId)
        => await _db.CartDetails.FindAsync(cartDetailsId);

    /// <inheritdoc/>
    public async Task<CartDetail> AddAsync(CartDetail cartDetail)
    {
        _db.CartDetails.Add(cartDetail);
        await _db.SaveChangesAsync();
        return cartDetail;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(CartDetail cartDetail)
    {
        _db.CartDetails.Update(cartDetail);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid cartDetailsId)
    {
        var item = await _db.CartDetails.FindAsync(cartDetailsId);
        if (item is not null)
        {
            _db.CartDetails.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAllByCartIdAsync(Guid cartId)
    {
        var items = await _db.CartDetails
            .Where(cd => cd.CartId == cartId)
            .ToListAsync();

        _db.CartDetails.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}