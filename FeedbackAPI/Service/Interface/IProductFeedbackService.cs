using FeedbackAPI.DTOs;

namespace FeedbackAPI.Service.Interface
{
    public interface IProductFeedbackService
    {
        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByProductIdAsync(Guid productId);

        // Lấy tất cả feedback theo OrderId để hiển thị popup View Feedback
        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId);

        // Kiểm tra order đã feedback chưa (trả về true/false)
        Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId);

        // Tạo bulk feedback cho nhiều sản phẩm trong 1 order
        Task<List<ProductFeedbackResponseDto>> CreateBulkFeedbackAsync(
            Guid customerId,
            CreateBulkFeedbackDto dto
        );

        // Cập nhật 1 feedback cụ thể
        Task<ProductFeedbackResponseDto?> UpdateFeedbackAsync(
            Guid customerId,
            UpdateProductFeedbackDto dto
        );
    }
}
