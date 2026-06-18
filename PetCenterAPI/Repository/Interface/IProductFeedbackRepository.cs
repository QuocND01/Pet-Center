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
    }
}
