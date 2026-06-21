using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IProductFeedbackRepository
    {
        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedback>> GetFeedbacksByProductIdAsync(Guid productId);

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        Task<List<ProductFeedback>> GetFeedbacksByOrderIdAsync(Guid orderId);
        Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId);

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedback>> CreateBulkAsync(List<ProductFeedback> feedbacks);
        Task AddMediaRangeAsync(List<FeedbackImage> mediaList);
        Task<List<FeedbackImage>> GetImagesByFeedbackIdAsync(Guid feedbackId);

        // ============================================================
        // FEEDBACK — UPDATE (CUSTOMER SIDE)
        // ============================================================
        Task<ProductFeedback?> GetFeedbackByIdAsync(Guid feedbackId);
        Task<ProductFeedback?> UpdateFeedbackAsync(ProductFeedback feedback);
        Task DeleteMediaByPublicIdsAsync(List<string> publicIds);
    }
}
