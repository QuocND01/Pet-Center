using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.ManageFeedback;

namespace PetCenterClient.Services.Interface
{
    public interface IFeedbackApiService
    {
        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        Task<bool> HasFeedbackForOrderAsync(Guid orderId);
        Task<List<ProductFeedbackViewModel>> GetFeedbacksByOrderIdAsync(Guid orderId);

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedbackViewModel>> GetFeedbacksByProductIdAsync(Guid productId);

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        Task<(bool Success, string Message)> CreateBulkFeedbackAsync(MultipartFormDataContent formData);
        Task<bool> UpdateFeedbackAsync(MultipartFormDataContent formData);
    }
}