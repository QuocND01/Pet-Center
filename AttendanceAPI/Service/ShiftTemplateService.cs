using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using AttendanceAPI.Service.Interface;
using AutoMapper;

namespace AttendanceAPI.Service
{
    public class ShiftTemplateService : IShiftTemplateService
    {
        private readonly IShiftTemplateRepository _repo;
        private readonly IMapper _mapper;

        public ShiftTemplateService(IShiftTemplateRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShiftTemplateResponseDTO>> GetTemplatesAsync(ShiftTemplateQueryParameters query)
        {
            var templates = await _repo.GetAllWithFilterAsync(query);
            return _mapper.Map<IEnumerable<ShiftTemplateResponseDTO>>(templates);
        }

        public async Task<ShiftTemplateResponseDTO?> GetTemplateDetailsAsync(Guid id)
        {
            var template = await _repo.GetByIdAsync(id);
            return _mapper.Map<ShiftTemplateResponseDTO>(template);
        }

        public async Task<ShiftTemplateResponseDTO> CreateTemplateAsync(ShiftTemplateRequestDTO dto)
        {
            var template = _mapper.Map<ShiftTemplate>(dto);
            template.TemplateId = Guid.NewGuid();
            template.IsActive = true;
            template.CreatedAt = DateTime.Now;

            await _repo.AddAsync(template);
            await _repo.SaveChangesAsync();

            return _mapper.Map<ShiftTemplateResponseDTO>(template);
        }

        public async Task<bool> UpdateTemplateAsync(Guid id, ShiftTemplateRequestDTO dto)
        {
            var template = await _repo.GetByIdAsync(id);
            if (template == null) return false;

            // Ghi đè dữ liệu mới từ DTO sang Entity có sẵn
            _mapper.Map(dto, template);
            template.UpdatedAt = DateTime.Now;

            _repo.Update(template);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            var template = await _repo.GetByIdAsync(id);
            if (template == null) return false;

            // Soft Delete (chỉ ẩn đi chứ không xóa khỏi CSDL)
            template.IsActive = false;
            template.UpdatedAt = DateTime.Now;

            _repo.Update(template);
            return await _repo.SaveChangesAsync();
        }
    }
}