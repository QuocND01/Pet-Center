using AutoMapper;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
namespace PetCenterAPI.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IPetRepository _petRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository repository, IPetRepository petRepo, IServiceRepository serviceRepo,
            IMapper mapper)
        {   
            _repository = repository;
            _petRepo = petRepo;
            _serviceRepo = serviceRepo;
            _mapper = mapper;
        }
        public async Task<AppointmentResponseDTO> BookAppointmentAsync(
            BookAppointmentRequestDTO request)
        {
            #region Validate

            if (request.ServiceIds == null || !request.ServiceIds.Any())
                throw new Exception("Please select at least one service.");

            if (request.AppointmentStart <= DateTime.Now)
                throw new Exception("Appointment time must be in the future.");

            #endregion

            #region Get Services

            var services = await _repository.GetServicesAsync(request.ServiceIds);

            if (services.Count != request.ServiceIds.Count)
                throw new Exception("One or more services do not exist.");

            #endregion

            #region Calculate Total

            decimal totalPrice = services.Sum(x => x.Price);

            int totalDuration = services.Sum(x => x.Duration);

            DateTime appointmentEnd =
                request.AppointmentStart.AddMinutes(totalDuration);

            #endregion

            #region Check Staff Exception

            DateOnly appointmentDate =
                DateOnly.FromDateTime(request.AppointmentStart);

            var staffException =
                await _repository.GetStaffExceptionAsync(
                    request.StaffId,
                    appointmentDate);

            if (staffException != null)
            {
                if (!staffException.IsWorking)
                    throw new Exception("Doctor is unavailable.");

                if (request.AppointmentStart.TimeOfDay <
                    staffException.StartTime!.Value.ToTimeSpan())
                    throw new Exception("Appointment is outside working hours.");

                if (appointmentEnd.TimeOfDay >
                    staffException.EndTime!.Value.ToTimeSpan())
                    throw new Exception("Appointment is outside working hours.");
            }

            #endregion

            #region Check Global Exception

            if (staffException == null)
            {
                var globalException =
                    await _repository.GetGlobalExceptionAsync(
                        appointmentDate);

                if (globalException != null)
                {
                    if (!globalException.IsWorking)
                        throw new Exception(globalException.Reason);

                    if (request.AppointmentStart.TimeOfDay <
                        globalException.StartTime!.Value.ToTimeSpan())
                        throw new Exception("Appointment is outside working hours.");

                    if (appointmentEnd.TimeOfDay >
                        globalException.EndTime!.Value.ToTimeSpan())
                        throw new Exception("Appointment is outside working hours.");
                }
            }

            #endregion

            #region Check Global Schedule

            var schedule =
                await _repository.GetGlobalScheduleAsync(
                    request.AppointmentStart.DayOfWeek);

            if (schedule == null)
                throw new Exception("Working schedule not found.");

            if (!schedule.IsWorking)
                throw new Exception("Clinic is closed.");

            if (request.AppointmentStart.TimeOfDay <
                schedule.StartTime!.Value.ToTimeSpan())
                throw new Exception("Appointment is outside working hours.");

            if (appointmentEnd.TimeOfDay >
                schedule.EndTime!.Value.ToTimeSpan())
                throw new Exception("Appointment is outside working hours.");

            #endregion

            #region Check Time Conflict

            bool conflict =
                await _repository.IsTimeConflictAsync(
                    request.StaffId,
                    request.AppointmentStart,
                    appointmentEnd);

            if (conflict)
                throw new Exception("Doctor already has another appointment.");

            #endregion

            #region Create Appointment

            var appointment = _mapper.Map<Appointment>(request);

            appointment.AppointmentId = Guid.NewGuid();

            appointment.AppointmentEnd = appointmentEnd;

            appointment.Total = totalPrice;

            appointment.Status = 1;

            appointment.CreatedAt = DateTime.Now;

            #endregion
            foreach (var service in services)
            {
                appointment.AppointmentServices.Add(new Models.AppointmentService
                {
                    AppointmentServiceId = Guid.NewGuid(),

                    AppointmentId = appointment.AppointmentId,

                    ServiceId = service.ServiceId,

                    ServiceName = service.ServiceName,

                    PriceAtBooking = service.Price,

                    Duration = service.Duration,

                    ServiceType = service.ServiceType
                });
            }
            var pet = await _repository.GetPetForSnapshotAsync(request.PetId);

            if (pet == null)
                throw new Exception("Pet not found.");

            var staff = await _repository.GetStaffForSnapshotAsync(request.StaffId);

            if (staff == null)
                throw new Exception("Doctor not found.");
            appointment.AppointmentSnapshot =
    new AppointmentSnapshot
    {
        AppointmentSnapshotId = Guid.NewGuid(),

        AppointmentId = appointment.AppointmentId,

        Species = pet.Species ?? "Unknown",

        Breed = pet.Breed ?? "Unknown",

        Gender = pet.Gender ?? "Unknown",

        Weight = pet.Weight ?? 0,

        VetName = staff.FullName,

        ExperienceYears = staff.VetProfile.ExperienceYears,

        Rating = 0

    };
            await _repository.CreateAppointmentAsync(appointment);

            await _repository.SaveChangesAsync();
            return _mapper.Map<AppointmentResponseDTO>(appointment);
        }

        public async Task<BookingDataResponseDTO> GetBookingDataAsync(Guid customerId)
        {
            var pets = await _petRepo.GetPetsByCustomerIdAsync(customerId);

            var staffs = await _repository.GetActiveVetsAsync();

            var services = await _serviceRepo.GetAllActiveServicesAsync();

            return new BookingDataResponseDTO
            {
                Pets = _mapper.Map<List<BookingPetDTO>>(pets),

                Staffs = _mapper.Map<List<BookingStaffDTO>>(staffs),

                Services = _mapper.Map<List<BookingServiceDTO>>(services)
            };
        }
    }
}