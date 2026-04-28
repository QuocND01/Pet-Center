using CartAPI.Models;

namespace CartAPI.Repositories.Interfaces;

public interface ICartDetailRepository
{
    /// <summary>Lấy item trong cart theo CartId và ProductId</summary>
    Task<CartDetail?> GetByCartAndProductAsync(Guid cartId, Guid productId);

    /// <summary>Lấy item theo CartDetailsId</summary>
    Task<CartDetail?> GetByIdAsync(Guid cartDetailsId);

    /// <summary>Thêm item vào cart</summary>
    Task<CartDetail> AddAsync(CartDetail cartDetail);

    /// <summary>Cập nhật item (quantity)</summary>
    Task UpdateAsync(CartDetail cartDetail);

    /// <summary>Xóa item khỏi cart</summary>
    Task DeleteAsync(Guid cartDetailsId);

    /// <summary>Xóa toàn bộ items trong cart</summary>
    Task DeleteAllByCartIdAsync(Guid cartId);
}