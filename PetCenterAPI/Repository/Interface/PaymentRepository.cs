using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public class PaymentRepository
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

        public async Task<Payment?> GetByTransactionRefAsync(string transactionRef)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
