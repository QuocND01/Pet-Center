using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.ManageFeedback;

namespace PetCenterClient.Services.Interface
{
    public interface IFeedbackApiService
    {
        Task<bool> HasFeedbackForOrderAsync(Guid orderId);
        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId);

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        Task<List<ProductFeedbackViewModel>> GetFeedbacksByProductIdAsync(Guid productId);

        Task<bool> CreateBulkFeedbackAsync(MultipartFormDataContent formData);
        Task<bool> UpdateFeedbackAsync(MultipartFormDataContent formData);
    }
}