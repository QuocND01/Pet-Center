using System.Text.Json;
using PetCenterAPI.DTOs;

namespace PetCenterAPI.Service.Interface
{
    public interface IMoMoService
    {
        Task<string?> CreatePaymentUrlAsync(Guid orderId, decimal amount, string transactionRef, string orderInfo);
        bool ValidateCallback(string rawBody, string signature);
        MoMoCallbackResult ParseCallback(JsonElement body, string rawBody);
    }
}