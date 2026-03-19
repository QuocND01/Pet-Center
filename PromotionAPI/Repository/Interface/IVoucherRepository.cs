using PromotionAPI.Models;

namespace PromotionAPI.Repository.Interface
{
    public interface IVoucherRepository
    {
        Task<List<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(Guid id);
        Task<Voucher?> GetByCodeAsync(string code);
        Task AddAsync(Voucher v);
        Task UpdateAsync(Voucher v);
        Task DeleteAsync(Guid id);
        Task<CustomerVoucher?> GetCustomerVoucher(Guid customerId, Guid voucherId);
        Task AddCustomerVoucher(CustomerVoucher cv);
    }
}
