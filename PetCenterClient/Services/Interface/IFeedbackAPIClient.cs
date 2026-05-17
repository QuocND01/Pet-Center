using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IFeedbackAPIClient
    {
        Task<bool> HasFeedbackForOrderAsync(Guid orderId);
        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId);

        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByProductIdAsync(Guid productId);

        Task<bool> CreateBulkFeedbackAsync(MultipartFormDataContent formData);
        Task<bool> UpdateFeedbackAsync(MultipartFormDataContent formData);
    }
}