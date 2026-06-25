using PetCenterAPI.DTOs.Responses.ChatBot;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class ChatbotService : IChatbotService
    {
        private readonly IChatbotRepository _chatbotRepository;

        public ChatbotService(IChatbotRepository chatbotRepository)
        {
            _chatbotRepository = chatbotRepository;
        }

        // ============================================================
        // PENDING ORDERS
        // ============================================================
        public async Task<List<ChatbotPendingOrderResponseDTO>> GetPendingOrdersAsync(Guid customerId)
            => await _chatbotRepository.GetPendingOrdersAsync(customerId);

        // ============================================================
        // LATEST ORDER STATUS
        // ============================================================
        public async Task<ChatbotLatestOrderResponseDTO?> GetLatestOrderStatusAsync(Guid customerId)
            => await _chatbotRepository.GetLatestOrderStatusAsync(customerId);
    }
}
