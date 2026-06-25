using PetCenterAPI.DTOs.Responses.ChatBot;

namespace PetCenterAPI.Repository.Interface
{
    public interface IChatbotRepository
    {
        // ============================================================
        // PENDING ORDERS
        // ============================================================
        Task<List<ChatbotPendingOrderResponseDTO>> GetPendingOrdersAsync(Guid customerId);

        // ============================================================
        // LATEST ORDER STATUS
        // ============================================================
        Task<ChatbotLatestOrderResponseDTO?> GetLatestOrderStatusAsync(Guid customerId);
    }
}
