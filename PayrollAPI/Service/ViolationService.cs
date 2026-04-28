using AutoMapper;
using PayrollAPI.DTOs;
using PayrollAPI.Models;
using PayrollAPI.Repository.Interface;
using PayrollAPI.Service.Interface;

namespace PayrollAPI.Service
{
    public class ViolationService : IViolationService
    {
        private readonly IViolationRepository _repo;
        private readonly IMapper _mapper;

        public ViolationService(IViolationRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ViolationResponseDTO>> GetViolationsAsync(ViolationQueryParameters query)
        {
            var list = await _repo.GetAllWithFilterAsync(query);
            return _mapper.Map<IEnumerable<ViolationResponseDTO>>(list);
        }

        public async Task<ViolationResponseDTO?> GetViolationDetailsAsync(Guid id)
        {
            var violation = await _repo.GetByIdAsync(id);
            return _mapper.Map<ViolationResponseDTO>(violation);
        }

        public async Task<ViolationResponseDTO> CreateViolationAsync(ViolationRequestDTO dto)
        {
            var violation = _mapper.Map<Violation>(dto);
            violation.ViolationId = Guid.NewGuid();
            violation.Status = 1; // Mặc định: 1 = Đã ghi nhận
            violation.CreatedAt = DateTime.Now;

            await _repo.AddAsync(violation);
            await _repo.SaveChangesAsync();

            return _mapper.Map<ViolationResponseDTO>(violation);
        }

        public async Task<bool> ChangeViolationStatusAsync(Guid id, int newStatus)
        {
            var violation = await _repo.GetByIdAsync(id);
            if (violation == null) return false;

            violation.Status = newStatus;
            violation.UpdatedAt = DateTime.Now;

            _repo.Update(violation);
            return await _repo.SaveChangesAsync();
        }
    }
}