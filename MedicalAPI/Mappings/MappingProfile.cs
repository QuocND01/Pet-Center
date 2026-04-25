// File: Mappings/MappingProfile.cs
using AutoMapper;
using MedicalAPI.DTOs.MedicalRecord;
using MedicalAPI.DTOs.Prescription;
using MedicalAPI.Models;

namespace MedicalAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MedicalRecord, MedicalRecordReadDto>()
            .ForMember(dest => dest.PrescriptionItems,
                       opt => opt.MapFrom(src => src.PrescriptionItems));

        CreateMap<PrescriptionItem, PrescriptionItemReadDto>();
    }
}