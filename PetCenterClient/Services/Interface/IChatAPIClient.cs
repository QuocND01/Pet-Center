using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IChatAPIClient
    {
        Task<List<ChatCustomerViewModel>> GetMyCustomersAsync();
        Task<List<ChatMessageViewModel>> GetChatHistoryAsync(Guid partnerId);
    }
}