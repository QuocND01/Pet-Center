using Microsoft.AspNetCore.Http;
using PetCenterAPI.DTOs;

namespace PetCenterAPI.Service.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Guid orderId, decimal amount, string transactionRef, string clientIp, string orderInfo);
        bool ValidateCallback(IQueryCollection query);
        VnPayCallbackResult ParseCallback(IQueryCollection query);
    }
}