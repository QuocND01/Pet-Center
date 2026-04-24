// File: Mappings/MappingProfile.cs
using AutoMapper;
using StaffAPI.DTOs.Staff;
using StaffAPI.Models;

namespace StaffAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Staff, StaffReadDto>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.Roles))
            .ForMember(d => d.VetProfile, o => o.MapFrom(s => s.VetProfile));

        CreateMap<Role, RoleReadDto>();
        CreateMap<VetProfile, VetProfileReadDto>();
    }
}