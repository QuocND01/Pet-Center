// File: Services/VetProfileService.cs
using AutoMapper;
using StaffAPI.DTOs.Staff;
using StaffAPI.Models;
using StaffAPI.Repositories.Interfaces;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Services;

public class VetProfileService : IVetProfileService
{
    private readonly IVetProfileRepository _vetRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IMapper _mapper;

    public VetProfileService(
        IVetProfileRepository vetRepo,
        IStaffRepository staffRepo,
        IMapper mapper)
    {
        _vetRepo = vetRepo;
        _staffRepo = staffRepo;
        _mapper = mapper;
    }

    
    public IQueryable<VetProfile> GetAllQueryable()
        => _vetRepo.GetAll();

    
    public async Task<VetProfileReadDto> GetByIdAsync(Guid id)
    {
        var vp = await _vetRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"VetProfile {id} not found.");
        return _mapper.Map<VetProfileReadDto>(vp);
    }

    
    public async Task<VetProfileReadDto> GetByStaffIdAsync(Guid staffId)
    {
        var vp = await _vetRepo.GetByStaffIdAsync(staffId)
            ?? throw new KeyNotFoundException($"VetProfile for Staff {staffId} not found.");
        return _mapper.Map<VetProfileReadDto>(vp);
    }

    
    public async Task<VetProfileReadDto> CreateAsync(Guid staffId, VetProfileCreateDto dto)
    {
        // Staff must exist
        var staffExists = await _staffRepo.ExistsAsync(staffId);
        if (!staffExists)
            throw new KeyNotFoundException($"Staff {staffId} not found.");

        // No duplicate profile
        var existing = await _vetRepo.GetByStaffIdAsync(staffId);
        if (existing != null)
            throw new InvalidOperationException($"Staff {staffId} already has a VetProfile.");

        // LicenseNumber unique
        if (!string.IsNullOrWhiteSpace(dto.LicenseNumber))
        {
            var dup = await _vetRepo.GetByLicenseNumberAsync(dto.LicenseNumber);
            if (dup != null)
                throw new InvalidOperationException($"LicenseNumber '{dto.LicenseNumber}' already exists.");
        }

        var vp = new VetProfile
        {
            VetProfileId = Guid.NewGuid(),
            StaffId = staffId,
            Description = dto.Description,
            LicenseNumber = dto.LicenseNumber,
            ExperienceYears = 0,
            IsActive = true
        };

        var created = await _vetRepo.CreateAsync(vp);
        return _mapper.Map<VetProfileReadDto>(created);
    }

    
    public async Task<VetProfileReadDto> UpdateAsync(Guid id, VetProfileUpdateDto dto)
    {
        var vp = await _vetRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"VetProfile {id} not found.");

        // Only Description and IsActive are updatable
        // ExperienceYears → accumulated automatically
        // LicenseNumber   → immutable
        // Rating          → updated by appointment service
        if (dto.Description != null) vp.Description = dto.Description;
        if (dto.IsActive != null) vp.IsActive = dto.IsActive.Value;

        var updated = await _vetRepo.UpdateAsync(vp);
        return _mapper.Map<VetProfileReadDto>(updated);
    }

    
    public async Task SoftDeleteAsync(Guid id)
    {
        var vp = await _vetRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"VetProfile {id} not found.");
        await _vetRepo.SoftDeleteAsync(vp);
    }
}