using CartAPI.DTOs;

namespace CartAPI.Services.Interfaces;

public interface ICartService
{
    /// <summary>Lấy cart của customer (kèm thông tin sản phẩm từ ProductAPI)</summary>
    Task<CartReadDto?> GetCartByCustomerIdAsync(Guid customerId);

    /// <summary>Lấy cart theo CartId</summary>
    Task<CartReadDto?> GetCartByIdAsync(Guid cartId);

    /// <summary>Khởi tạo cart cho customer mới (gọi nội bộ từ CustomerService)</summary>
    Task<CartReadDto> InitCartAsync(CartInitDto dto);

    /// <summary>Thêm hoặc cộng dồn item vào cart, validate stock</summary>
    Task<CartReadDto> AddItemToCartAsync(Guid cartId, CartDetailAddDto dto);

    /// <summary>Cập nhật số lượng item, validate stock</summary>
    Task<CartReadDto> UpdateItemQuantityAsync(Guid cartId, Guid cartDetailsId, CartDetailUpdateDto dto);

    /// <summary>Xóa một item khỏi cart</summary>
    Task RemoveItemAsync(Guid cartId, Guid cartDetailsId);

    /// <summary>Xóa toàn bộ items (clear cart sau khi đặt hàng)</summary>
    Task ClearCartAsync(Guid cartId);
}