using PetCenterAPI.DTOs.Responses.ChatBot;

namespace PetCenterAPI.Service.Interface
{
    public interface IChatbotService
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
