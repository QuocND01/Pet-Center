using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByTransactionRefAsync(string transactionRef);
        Task UpdateAsync(Payment payment);
    }
}
