using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using System.Text.Json;
namespace PetCenterAPI.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IPetRepository _petRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IMapper _mapper;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IVnPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly IPaymentRepository _paymentRepo;


        public AppointmentService(
            IAppointmentRepository repository,
            IPetRepository petRepo,
            IServiceRepository serviceRepo,
            IMapper mapper,
            IScheduleRepository scheduleRepository,
            IMoMoService? moMoService = null,
            IVnPayService? vnPayService = null,
            IPaymentRepository? paymentRepository = null)
        {   
            _appointmentRepo = repository;
            _petRepo = petRepo;
            _serviceRepo = serviceRepo;
            _mapper = mapper;
            _scheduleRepo = scheduleRepository;
            _moMoService = moMoService!;
            _vnPayService = vnPayService!;
            _paymentRepo = paymentRepository!;
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

            var services = await _appointmentRepo.GetServicesAsync(request.ServiceIds);

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
                await _appointmentRepo.GetStaffExceptionAsync(
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
                    await _appointmentRepo.GetGlobalExceptionAsync(
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
                await _appointmentRepo.GetGlobalScheduleAsync(
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
                await _appointmentRepo.IsTimeConflictAsync(
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
            var pet = await _appointmentRepo.GetPetForSnapshotAsync(request.PetId);

            if (pet == null)
                throw new Exception("Pet not found.");

            var staff = await _appointmentRepo.GetStaffForSnapshotAsync(request.StaffId);

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
            await _appointmentRepo.CreateAppointmentAsync(appointment);

            await _appointmentRepo.SaveChangesAsync();
            return _mapper.Map<AppointmentResponseDTO>(appointment);
        }

        public async Task<BookingDataResponseDTO> GetBookingDataAsync(Guid customerId)
        {
            var pets = await _petRepo.GetPetsByCustomerIdAsync(customerId);

            var staffs = await _appointmentRepo.GetActiveVetsAsync();

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
                await _appointmentRepo
                    .GetAppointmentsByCustomerAsync(customerId);

            return _mapper.Map<
                List<AppointmentListResponseDTO>>
                (appointments);
        }
        public async Task<List<AppointmentListResponseDTO>> GetAllAppointmentsAsync()
        {
            var appointments =
                await _appointmentRepo
                    .GetAllAppointmentsAsync();

            return _mapper.Map<
                List<AppointmentListResponseDTO>>
                (appointments);
        }
        public async Task<AppointmentResponseDTO> GetAppointmentDetailAsync(Guid appointmentId)
        {
            var appointment =
                await _appointmentRepo
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
                await _appointmentRepo
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

            await _appointmentRepo.SaveChangesAsync();
        }
        public async Task ForwardAppointmentStatusAsync(Guid appointmentId, Guid staffId)
        {
            var appointment =
                await _appointmentRepo
                    .GetByIdAsync(appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (appointment.StaffId != staffId)
                throw new Exception("You are not assigned to this appointment.");

            if (appointment.Status is < 1 or > 3)
                throw new Exception("Appointment status cannot be updated.");

            appointment.Status++;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepo.SaveChangesAsync();
        }
        public async Task SubmitReviewAsync(
    Guid customerId,
    SubmitReviewRequestDTO request)
        {
            var appointment =
                await _appointmentRepo
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

            await _appointmentRepo.SaveChangesAsync();
        }
        public async Task CompleteAppointmentService(Guid appointmentServiceId)
        {
            var appointmentService = await _appointmentRepo.GetAppointmentServiceByIdAsync(appointmentServiceId);

            if (appointmentService == null)
            {
                throw new Exception("Appointment service not found.");
            }

            appointmentService.Status = 2;
            appointmentService.CompleteAt = DateTime.UtcNow;

            await _appointmentRepo.SaveChangesAsync();
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
                await _appointmentRepo
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
        public async Task<AppointmentPaymentResponseDTO> CreatePaymentUrlAsync(AppointmentPaymentRequestDTO request)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found.");

            // 1: Reserved
            if (appointment.Status != 1)
                throw new InvalidOperationException("Appointment is not in a valid state for payment.");

            if (appointment.ReservedUntil.HasValue && appointment.ReservedUntil.Value < DateTime.UtcNow)
            {
                appointment.Status = 5; // Expired
                await _appointmentRepo.UpdateAsync(appointment);
                throw new InvalidOperationException("Time to hold the appointment has expired.");
            }

            string transactionRef = $"{appointment.AppointmentId.ToString()[..8]}_{DateTime.UtcNow.Ticks}";
            decimal remainingAmount = appointment.Total - appointment.PaidAmount;
            string paymentUrl = string.Empty;

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                AppointmentId = appointment.AppointmentId,
                PaymentMethod = request.PaymentMethod,
                Amount = remainingAmount,
                Status = 0, // 0: Pending, 1: Success, 2: Failed
                TransactionRef = transactionRef,
                CreatedAt = DateTime.UtcNow
            };

            string orderInfo = $"Thanh toan lich hen #{appointment.AppointmentId}";

            //  AN TOÀN: string.Equals xử lý được trường hợp arg1 bị null
            if (string.Equals(request.PaymentMethod, "VNPAY", StringComparison.OrdinalIgnoreCase))
            {
                paymentUrl = _vnPayService.CreatePaymentUrl(
                    appointment.AppointmentId,
                    remainingAmount,
                    transactionRef,
                    request.ClientIpAddress ?? "127.0.0.1",
                    orderInfo
                );
            }
            else if (request.PaymentMethod.Equals("MOMO", StringComparison.OrdinalIgnoreCase))
            {
                var momoUrl = await _moMoService.CreatePaymentUrlAsync(
                    appointment.AppointmentId,
                    remainingAmount,
                    transactionRef,
                    orderInfo
                );

                if (string.IsNullOrEmpty(momoUrl))
                    throw new Exception("Failed to initialize MoMo payment.");

                paymentUrl = momoUrl;
            }
            else
            {
                throw new BadHttpRequestException("Payment method is invalid.");
            }

            // Lưu trực tiếp vào Database
            await _paymentRepo.AddAsync(payment);

            return new AppointmentPaymentResponseDTO
            {
                PaymentId = payment.PaymentId,
                PaymentMethod = payment.PaymentMethod,
                TransactionRef = transactionRef,
                Amount = remainingAmount,
                PaymentUrl = paymentUrl
            };
        }

        public async Task<PaymentCallbackResponseDTO> ProcessVnPayCallbackAsync(IQueryCollection query)
        {
            if (!_vnPayService.ValidateCallback(query))
            {
                return new PaymentCallbackResponseDTO { IsSuccess = false, Message = "Signature is invalid." };
            }

            var result = _vnPayService.ParseCallback(query); // Giả định ParseCallback trả về Obj có TransactionRef, ResponseCode...
            var payment = await _paymentRepo.GetByTransactionRefAsync(result.TransactionRef);

            if (payment == null)
                return new PaymentCallbackResponseDTO { IsSuccess = false, Message = "Cant find payment." };

            if (payment.Status == 1)
                return new PaymentCallbackResponseDTO { IsSuccess = true, Message = "Payment has been processed before.", AppointmentId = payment.AppointmentId };

            if (result.ResponseCode == "00") // Thành công trên VnPay
            {
                payment.Status = 1; // Success
                payment.PaidAmount = payment.Amount;
                payment.PaidAt = DateTime.UtcNow;
                payment.ResponseCode = result.ResponseCode;
                await _paymentRepo.UpdateAsync(payment);

                // Update Appointment State -> Confirmed (2)
                var appointment = payment.Appointment;
                if (appointment != null)
                {
                    appointment.Status = 2; // Confirmed
                    appointment.PaidAmount += payment.Amount;
                    appointment.UpdatedAt = DateTime.UtcNow;
                    await _appointmentRepo.UpdateAsync(appointment);
                }

                return new PaymentCallbackResponseDTO
                {
                    IsSuccess = true,
                    Message = "Payment succesful!",
                    AppointmentId = payment.AppointmentId,
                    TransactionRef = payment.TransactionRef!
                };
            }

            payment.Status = 2; // Failed
            payment.ResponseCode = result.ResponseCode;
            await _paymentRepo.UpdateAsync(payment);

            return new PaymentCallbackResponseDTO
            {
                IsSuccess = false,
                Message = $"Payment failed with error code: {result.ResponseCode}",
                AppointmentId = payment.AppointmentId,
                TransactionRef = payment.TransactionRef!
            };
        }

        public async Task<PaymentCallbackResponseDTO> ProcessMoMoCallbackAsync(JsonElement body, string rawBody, string signature)
        {
            if (!_moMoService.ValidateCallback(rawBody, signature))
            {
                return new PaymentCallbackResponseDTO { IsSuccess = false, Message = "Signature is invalid." };
            }

            var result = _moMoService.ParseCallback(body, rawBody);
            var payment = await _paymentRepo.GetByTransactionRefAsync(result.TransactionRef);

            if (payment == null)
                return new PaymentCallbackResponseDTO { IsSuccess = false, Message = "Cant find payment." };

            if (payment.Status == 1)
                return new PaymentCallbackResponseDTO { IsSuccess = true, Message = "Payment has been processed before.", AppointmentId = payment.AppointmentId };

            if (result.ResponseCode == "0") // Thành công trên MoMo
            {
                payment.Status = 1;
                payment.PaidAmount = payment.Amount;
                payment.PaidAt = DateTime.UtcNow;
                payment.ResponseCode = result.ResponseCode.ToString();
                await _paymentRepo.UpdateAsync(payment);

                var appointment = payment.Appointment;
                if (appointment != null)
                {
                    appointment.Status = 2; // Confirmed
                    appointment.PaidAmount += payment.Amount;
                    appointment.UpdatedAt = DateTime.UtcNow;
                    await _appointmentRepo.UpdateAsync(appointment);
                }

                return new PaymentCallbackResponseDTO
                {
                    IsSuccess = true,
                    Message = "Payment successful.",
                    AppointmentId = payment.AppointmentId,
                    TransactionRef = payment.TransactionRef!
                };
            }

            payment.Status = 2;
            payment.ResponseCode = result.ResponseCode.ToString();
            await _paymentRepo.UpdateAsync(payment);

            return new PaymentCallbackResponseDTO
            {
                IsSuccess = false,
                Message = $"Payment failed with error code: {result.ResponseCode}",
                AppointmentId = payment.AppointmentId,
                TransactionRef = payment.TransactionRef!
            };
        }
        public async Task<AppointmentResponseDTO> UpdateReservedAppointmentAsync(UpdateAppointmentRequestDTO request, Guid customerId)
        {
            // 1. Get Appointment kèm theo AppointmentServices VÀ AppointmentSnapshot (nếu có)
            var appointment = await _appointmentRepo.GetByIdForUpdateAsync(request.AppointmentId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found!");

            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa lịch hẹn này.");

            if (appointment.Status != 1)
                throw new InvalidOperationException("Chỉ có thể cập nhật lịch hẹn ở trạng thái Giữ chỗ (Reserved).");

            // 2. Validate & Lấy thông tin Pet và Staff cho Snapshot
            var pet = await _appointmentRepo.GetPetForSnapshotAsync(appointment.PetId);
            if (pet == null)
                throw new KeyNotFoundException("Không tìm thấy thông tin thú cưng.");

            var staff = await _appointmentRepo.GetStaffForSnapshotAsync(request.StaffId);
            if (staff == null)
                throw new KeyNotFoundException("Không tìm thấy thông tin bác sĩ/nhân viên.");

            var selectedServices = await _appointmentRepo.GetServicesAsync(request.ServiceIds);
            if (selectedServices == null || !selectedServices.Any())
                throw new ArgumentException("Vui lòng chọn ít nhất 1 dịch vụ.");

            // 3. Tính toán lại tổng tiền và thời lượng
            decimal newTotal = selectedServices.Sum(s => s.Price);
            int totalDurationMinutes = selectedServices.Sum(s => s.Duration);

            // 4. Cập nhật thông tin Lịch hẹn (Parent)
            appointment.StaffId = request.StaffId;
            appointment.AppointmentStart = request.AppointmentStart;
            appointment.AppointmentEnd = request.AppointmentStart.AddMinutes(totalDurationMinutes);
            appointment.Total = newTotal;
            appointment.Note = request.Note;
            appointment.UpdatedAt = DateTime.UtcNow;

            // 5. Cập nhật danh sách AppointmentServices (Bảng con 1-N)
            // .Clear() sẽ chuyển các dịch vụ cũ sang trạng thái Deleted trong Change Tracker
            appointment.AppointmentServices.Clear();

            foreach (var service in selectedServices)
            {
                // ❌ KHÔNG gán AppointmentServiceId = Guid.NewGuid() ở đây
                // EF Core sẽ tự sinh GUID hoặc coi đây là Added entity để chạy INSERT INTO
                appointment.AppointmentServices.Add(new PetCenterAPI.Models.AppointmentService
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    PriceAtBooking = service.Price,
                    Duration = service.Duration,
                    ServiceType = service.ServiceType,
                    Status = 1
                });
            }

            // 6. Cập nhật hoặc Khởi tạo AppointmentSnapshot (Bảng con 1-1)
            if (appointment.AppointmentSnapshot != null)
            {
                appointment.AppointmentSnapshot.Species = pet.Species ?? "Unknown";
                appointment.AppointmentSnapshot.Breed = pet.Breed ?? "Unknown";
                appointment.AppointmentSnapshot.Gender = pet.Gender ?? "Unknown";
                appointment.AppointmentSnapshot.Weight = pet.Weight ?? 0;
                appointment.AppointmentSnapshot.VetName = staff.FullName;
            }
            else
            {
                appointment.AppointmentSnapshot = new AppointmentSnapshot
                {
                    Species = pet.Species ?? "Unknown",
                    Breed = pet.Breed ?? "Unknown",
                    Gender = pet.Gender ?? "Unknown",
                    Weight = pet.Weight ?? 0,
                    VetName = staff.FullName,
                    Rating = 0
                };
            }

            // 7. Lưu thay đổi qua Repository
            try
            {
                await _appointmentRepo.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    Console.WriteLine($"[CONCURRENCY DEBUG] Entity: {entry.Entity.GetType().Name} | State: {entry.State}");
                    foreach (var prop in entry.Properties)
                    {
                        Console.WriteLine($"   - Property: {prop.Metadata.Name} | CurrentValue: {prop.CurrentValue} | OriginalValue: {prop.OriginalValue}");
                    }
                }
                throw;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Console.WriteLine($"[DB UPDATE ERROR]: {dbEx.InnerException?.Message ?? dbEx.Message}");
                throw;
            }

            return _mapper.Map<AppointmentResponseDTO>(appointment);
        }
    }
}