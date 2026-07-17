using AutoMapper;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
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
        private readonly IScheduleRepository _scheduleRepo;

        public AppointmentService(
            IAppointmentRepository repository, IPetRepository petRepo, IServiceRepository serviceRepo,
            IMapper mapper, IScheduleRepository scheduleRepository)
        {   
            _repository = repository;
            _petRepo = petRepo;
            _serviceRepo = serviceRepo;
            _mapper = mapper;
            _scheduleRepo = scheduleRepository;
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
        public async Task<List<AppointmentListResponseDTO>> GetMyAppointmentsAsync(Guid customerId)
        {
            var appointments =
                await _repository
                    .GetAppointmentsByCustomerAsync(customerId);

            return _mapper.Map<
                List<AppointmentListResponseDTO>>
                (appointments);
        }
        public async Task<AppointmentResponseDTO> GetAppointmentDetailAsync(Guid appointmentId)
        {
            var appointment =
                await _repository
                    .GetAppointmentDetailAsync(appointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found.");
            }

            return _mapper.Map<
                AppointmentResponseDTO>
                (appointment);
        }
        public async Task CancelAppointmentAsync(
    Guid appointmentId,
    Guid customerId)
        {
            var appointment =
                await _repository
                    .GetByIdAsync(appointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found.");
            }

            if (appointment.CustomerId != customerId)
            {
                throw new Exception(
                    "You are not allowed to cancel this appointment.");
            }

            if (appointment.Status == 0)
            {
                throw new Exception(
                    "Appointment already cancelled.");
            }

            if (appointment.Status == 3)
            {
                throw new Exception(
                    "Appointment is in progress.");
            }

            if (appointment.Status == 4)
            {
                throw new Exception(
                    "Appointment already completed.");
            }

            appointment.Status = 0;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync();
        }
        public async Task SubmitReviewAsync(
    Guid customerId,
    SubmitReviewRequestDTO request)
        {
            var appointment =
                await _repository
                    .GetAppointmentDetailAsync(
                        request.AppointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found.");
            }

            if (appointment.CustomerId != customerId)
            {
                throw new Exception(
                    "You are not allowed to review this appointment.");
            }

            if (appointment.Status != 4)
            {
                throw new Exception(
                    "Only completed appointments can be reviewed.");
            }

            if (appointment.AppointmentSnapshot == null)
            {
                throw new Exception(
                    "Appointment snapshot not found.");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new Exception(
                    "Rating must be between 1 and 5.");
            }

            appointment.AppointmentSnapshot.Rating =
                request.Rating;

            appointment.AppointmentSnapshot.Feedback =
                request.Feedback;

            await _repository.SaveChangesAsync();
        }
        public async Task<List<AvailableSlotResponseDTO>>
    GetAvailableSlotsAsync(
        GetAvailableSlotsRequestDTO request)
        {
            var services =
                await _serviceRepo.GetServicesByIdsAsync(
                    request.ServiceIds);

            var duration =
                services.Sum(x => x.Duration);

            var appointments =
                await _repository
                    .GetDoctorAppointmentsByDateAsync(
                        request.StaffId,
                        request.Date);

            var workTime =
                await GetWorkTimeAsync(
                    request.StaffId,
                    request.Date);

            return GenerateAvailableSlots(
                workTime.Start,
                workTime.End,
                duration,
                appointments);
        }
        //Priveate method to get work time
        private List<AvailableSlotResponseDTO> GenerateAvailableSlots(
            DateTime workStart,
            DateTime workEnd,
            int durationMinutes,
            List<Appointment> appointments)
        {
            var result = new List<AvailableSlotResponseDTO>();

            appointments = appointments
                .OrderBy(x => x.AppointmentStart)
                .ToList();

            var current = workStart;

            while (current.AddMinutes(durationMinutes) <= workEnd)
            {
                var slotEnd =
                    current.AddMinutes(durationMinutes);

                bool overlap =
                    appointments.Any(a =>
                        current < a.AppointmentEnd &&
                        slotEnd > a.AppointmentStart);

                if (!overlap)
                {
                    var previousAppointment = appointments
                        .Where(x => x.AppointmentEnd <= current)
                        .OrderByDescending(x => x.AppointmentEnd)
                        .FirstOrDefault();

                    var nextAppointment = appointments
                        .Where(x => x.AppointmentStart >= slotEnd)
                        .OrderBy(x => x.AppointmentStart)
                        .FirstOrDefault();

                    int gapBefore = previousAppointment == null
                        ? 0
                        : (int)(current - previousAppointment.AppointmentEnd)
                            .TotalMinutes;

                    int gapAfter = nextAppointment == null
                        ? 0
                        : (int)(nextAppointment.AppointmentStart - slotEnd)
                            .TotalMinutes;
                    int score = 0;
                    if (gapBefore <= 15 || gapAfter <= 15)
                    {
                         score = 0;
                    }
                    else
                    {
                        score =
                            Math.Min(gapBefore, gapAfter) * 1000
                            + (gapBefore + gapAfter);
                    }
                    

                    // Phạt slot đầu ngày hoặc cuối ngày
                    if (previousAppointment == null ||
                        nextAppointment == null)
                    {
                        score += 1000;
                    }

                    result.Add(new AvailableSlotResponseDTO
                    {
                        StartTime = current,
                        EndTime = slotEnd,
                        GapBeforeMinutes = gapBefore,
                        GapAfterMinutes = gapAfter,
                        Score = score
                    });
                }

                current = current.AddMinutes(15);
            }

            var recommendedSlots = result
                .OrderBy(x => x.Score)
                .ThenBy(x => x.StartTime)
                .Take(3)
                .ToList();

            for (int i = 0; i < recommendedSlots.Count; i++)
            {
                recommendedSlots[i].IsRecommended = true;
                recommendedSlots[i].RecommendationRank = i + 1;
            }

            return result
                .OrderBy(x => x.StartTime)
                .ToList();
        }
        private async Task<(DateTime Start, DateTime End)> GetWorkTimeAsync(
    Guid staffId,
    DateOnly date)
        {
            var exception =
                await _scheduleRepo.GetScheduleExceptionAsync(
                    staffId,
                    date);

            if (exception != null)
            {
                if (!exception.IsWorking)
                    throw new Exception("Doctor is unavailable.");

                return (
                    date.ToDateTime(exception.StartTime!.Value),
                    date.ToDateTime(exception.EndTime!.Value)
                );
            }

            var dayOfWeek = (byte)(
                date.DayOfWeek == DayOfWeek.Sunday
                    ? 7
                    : (int)date.DayOfWeek);

            var global =
                await _scheduleRepo.GetGlobalWorkScheduleAsync(
                    dayOfWeek);

            if (global == null || !global.IsWorking)
                throw new Exception("Doctor is unavailable.");

            return (
                date.ToDateTime(global.StartTime!.Value),
                date.ToDateTime(global.EndTime!.Value)
            );
        }
    }
}