using AutoMapper;
using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using System;

namespace AttendanceAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==========================================
            // 1. MAPPING CHO STAFF SHIFT (CA LÀM VIỆC)
            // ==========================================
            CreateMap<StaffShift, StaffShiftResponseDTO>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => (src.OverrideStartTime ?? src.Template.StartTime).ToTimeSpan()))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => (src.OverrideEndTime ?? src.Template.EndTime).ToTimeSpan()))
                .ForMember(dest => dest.ShiftDate, opt => opt.MapFrom(src => src.ShiftDate.ToDateTime(TimeOnly.MinValue)));

            CreateMap<StaffShiftRequestDTO, StaffShift>()
                .ForMember(dest => dest.ShiftDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ShiftDate)))
                .ForMember(dest => dest.OverrideStartTime, opt => opt.MapFrom(src => src.OverrideStartTime.HasValue ? TimeOnly.FromTimeSpan(src.OverrideStartTime.Value) : (TimeOnly?)null))
                .ForMember(dest => dest.OverrideEndTime, opt => opt.MapFrom(src => src.OverrideEndTime.HasValue ? TimeOnly.FromTimeSpan(src.OverrideEndTime.Value) : (TimeOnly?)null));


            // ==========================================
            // 2. MAPPING CHO ATTENDANCE (CHẤM CÔNG)
            // ==========================================
            CreateMap<Attendance, AttendanceResponseDTO>();

            CreateMap<AttendanceRequestDTO, Attendance>();

            // ==========================================
            // 3. MAPPING CHO SHIFT TEMPLATE
            // ==========================================
            CreateMap<ShiftTemplate, ShiftTemplateResponseDTO>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));

            CreateMap<ShiftTemplateRequestDTO, ShiftTemplate>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)));
        }
    }
}