using FeedbackAPI.Models;

namespace FeedbackAPI.Repository.Interface
{
    public interface IProductFeedbackRepository
    {
        // Lấy tất cả feedback theo ProductId
        Task<List<ProductFeedback>> GetFeedbacksByProductIdAsync(Guid productId);

        // Lấy tất cả feedback theo OrderId
        Task<List<ProductFeedback>> GetFeedbacksByOrderIdAsync(Guid orderId);

        // Lấy feedback theo FeedbackId
        Task<ProductFeedback?> GetFeedbackByIdAsync(Guid feedbackId);

        // Kiểm tra order đã được feedback chưa
        Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId);

        // Tạo nhiều feedback cùng lúc (1 order nhiều sản phẩm)
        Task<List<ProductFeedback>> CreateBulkFeedbackAsync(List<ProductFeedback> feedbacks);

        // Cập nhật feedback
        Task<ProductFeedback?> UpdateFeedbackAsync(ProductFeedback feedback);

        // Feedback hình ảnh và video
        Task AddMediaRangeAsync(List<FeedbackMedia> mediaList);
        Task<List<FeedbackMedia>> GetMediaByFeedbackIdAsync(Guid feedbackId);
        Task DeleteMediaByPublicIdsAsync(List<string> publicIds);
    }
}
