using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IProductFeedbackRepository
    {
        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedback>> GetFeedbacksByProductIdAsync(Guid productId);
    }
}
