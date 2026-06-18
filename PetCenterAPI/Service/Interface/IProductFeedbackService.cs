using PetCenterAPI.DTOs.Responses.ManageFeedback;

namespace PetCenterAPI.Service.Interface
{
    public interface IProductFeedbackService
    {
        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedbackResponseDTO>> GetFeedbacksByProductIdAsync(Guid productId);
    }
}

