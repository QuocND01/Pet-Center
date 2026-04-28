using Microsoft.EntityFrameworkCore;
using PayrollAPI.DTOs;
using PayrollAPI.Models;
using PayrollAPI.Repository.Interface;

namespace PayrollAPI.Repository
{
    public class ViolationRepository : IViolationRepository
    {
        // Thay tên Context theo đúng file Context của project PayrollAPI
        private readonly PetCenterPayrollServiceContext _context;

        public ViolationRepository(PetCenterPayrollServiceContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Violation>> GetAllWithFilterAsync(ViolationQueryParameters query)
        {
            var violations = _context.Violations.AsQueryable();

            if (query.StaffId.HasValue)
                violations = violations.Where(v => v.StaffId == query.StaffId.Value);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                violations = violations.Where(v => v.ViolationType.Contains(query.SearchTerm));

            if (query.Status.HasValue)
                violations = violations.Where(v => v.Status == query.Status.Value);

            if (query.FromDate.HasValue)
                violations = violations.Where(v => v.ViolationDate >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                violations = violations.Where(v => v.ViolationDate <= query.ToDate.Value);

            return await violations
                .OrderByDescending(v => v.ViolationDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Violation?> GetByIdAsync(Guid id) =>
            await _context.Violations.FindAsync(id);

        public async Task AddAsync(Violation violation) => await _context.Violations.AddAsync(violation);

        public void Update(Violation violation) => _context.Violations.Update(violation);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}