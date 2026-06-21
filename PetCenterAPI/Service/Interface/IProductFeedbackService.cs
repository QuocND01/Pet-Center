using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;

namespace PetCenterAPI.Service.Interface
{
    public interface IProductFeedbackService
    {
        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedbackResponseDTO>> GetFeedbacksByProductIdAsync(Guid productId);

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        Task<List<ProductFeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(Guid orderId);
        Task<bool> HasFeedbackForOrderAsync(Guid orderId, Guid customerId);

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedbackResponseDTO>> CreateBulkFeedbackAsync(
            Guid customerId,
            CreateBulkFeedbackRequestDTO request);

        // ============================================================
        // FEEDBACK — UPDATE (CUSTOMER SIDE)
        // ============================================================
        Task<ProductFeedbackResponseDTO?> UpdateFeedbackAsync(
            Guid customerId,
            UpdateFeedbackRequestDTO request);
    }
}

