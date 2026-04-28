using CartAPI.Models;

namespace CartAPI.Repositories.Interfaces;

public interface ICartRepository
{
    /// <summary>Lấy cart theo CartId, include CartDetails</summary>
    Task<Cart?> GetByIdAsync(Guid cartId);

    /// <summary>Lấy cart theo CustomerId</summary>
    Task<Cart?> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Tạo cart mới (internal - gọi từ CustomerService)</summary>
    Task<Cart> CreateAsync(Cart cart);

    /// <summary>Kiểm tra cart tồn tại</summary>
    Task<bool> ExistsAsync(Guid cartId);

    /// <summary>Kiểm tra customer đã có cart chưa</summary>
    Task<bool> ExistsByCustomerIdAsync(Guid customerId);
}