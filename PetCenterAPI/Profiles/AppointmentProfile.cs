using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;

namespace PetCenterAPI.Profiles
{
    public class AppointmentProfile : AutoMapper.Profile
    {
        public AppointmentProfile()
        {
            // Request -> Entity
            CreateMap<BookAppointmentRequestDTO, Appointment>()
                .ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentEnd, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentServices, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentSnapshot, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Pet, opt => opt.Ignore())
                .ForMember(dest => dest.Staff, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecords, opt => opt.Ignore());

            // Appointment -> Response
            CreateMap<Appointment, AppointmentResponseDTO>()
                .ForMember(
                    dest => dest.PetName,
                    opt => opt.MapFrom(src => src.Pet.PetName))
                .ForMember(
                    dest => dest.VetName,
                    opt => opt.MapFrom(src => src.Staff.FullName))
                .ForMember(
                    dest => dest.AppointmentServices,
                    opt => opt.MapFrom(src => src.AppointmentServices))
                .ForMember(
                    dest => dest.Snapshot,
                    opt => opt.MapFrom(src => src.AppointmentSnapshot));

            // AppointmentService -> DTO
            CreateMap<AppointmentService, AppointmentServiceResponseDTO>()
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.PriceAtBooking));

            // Snapshot -> DTO
            CreateMap<AppointmentSnapshot, AppointmentSnapshotResponseDTO>();

            // Staff -> BookingStaffDTO
            CreateMap<Staff, BookingStaffDTO>()
            // Map các trường thuộc Staff sang DTO (Tự động trùng tên)
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))

                // Map các trường từ VetProfile lồng bên trong Staff
                .ForMember(dest => dest.ExperienceYears, opt => opt.MapFrom(src => src.VetProfile != null ? src.VetProfile.ExperienceYears : null))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.VetProfile != null ? src.VetProfile.Description : null))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.VetProfile != null ? src.VetProfile.LicenseNumber : null));
            //
            CreateMap<Pet, BookingPetDTO>();
            CreateMap<Models.Service, BookingServiceDTO>()
            .ForMember(dest => dest.ServiceImages,
                       opt => opt.MapFrom(src => src.ServiceImages.Select(img => img.ImageUrl).ToList()));
            CreateMap<Appointment,
          AppointmentListResponseDTO>()
                .ForMember(
                    dest => dest.PetName,
                    opt => opt.MapFrom(src => src.Pet.PetName))
                .ForMember(
                    dest => dest.VetName,
                    opt => opt.MapFrom(src => src.Staff.FullName));
            

           

        }
    }
}
