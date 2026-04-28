using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Repository.Interface
{
    public interface IViolationRepository
    {
        Task<IEnumerable<Violation>> GetAllWithFilterAsync(ViolationQueryParameters query);
        Task<Violation?> GetByIdAsync(Guid id);
        Task AddAsync(Violation violation);
        void Update(Violation violation);
        Task<bool> SaveChangesAsync();
    }
}
