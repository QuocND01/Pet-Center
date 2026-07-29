using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetByTransactionRefAsync(string transactionRef);
        Task<Payment?> GetPaymentByTransactionRefAsync(string transactionRef);
        Task UpdateAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task SaveChangesAsync();
    }
}
