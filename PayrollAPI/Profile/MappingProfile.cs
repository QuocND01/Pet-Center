using AutoMapper;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Violation, ViolationResponseDTO>();
            CreateMap<ViolationRequestDTO, Violation>();
        }
    }
}