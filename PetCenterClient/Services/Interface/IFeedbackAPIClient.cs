using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IFeedbackAPIClient
    {
        Task<bool> HasFeedbackForOrderAsync(Guid orderId);
        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId);
        Task<bool> CreateBulkFeedbackAsync(CreateBulkFeedbackDto dto);
        Task<bool> UpdateFeedbackAsync(UpdateProductFeedbackDto dto);

        Task<List<ProductFeedbackResponseDto>> GetFeedbacksByProductIdAsync(Guid productId);
    }
}