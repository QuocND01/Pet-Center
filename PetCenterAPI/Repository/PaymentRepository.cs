using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PetCenterContext _context;

        public PaymentRepository(PetCenterContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public Task AddPaymentAsync(Payment payment) => AddAsync(payment);

        public async Task<Payment?> GetByTransactionRefAsync(string transactionRef)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);
        }

        public Task<Payment?> GetPaymentByTransactionRefAsync(string transactionRef) => GetByTransactionRefAsync(transactionRef);

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public Task UpdatePaymentAsync(Payment payment) => UpdateAsync(payment);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
