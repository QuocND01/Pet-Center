using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_BookAppointmentAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_BookAppointmentAsync()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _petRepoMock = new Mock<IPetRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _mapperMock = new Mock<IMapper>();
            _scheduleRepoMock = new Mock<IScheduleRepository>();

            _service = new PetCenterAPI.Service.AppointmentService(
                _repositoryMock.Object,
                _petRepoMock.Object,
                _serviceRepoMock.Object,
                _mapperMock.Object,
                _scheduleRepoMock.Object);
        }

        //=========================================================
        // UTCID01
        // ServiceId is null or emty
        //=========================================================
        [Fact]
        public async Task UTCID01_BookAppointmentAsync_ServiceIdsNullOrEmpty_ShouldThrowException()
        {
            // Arrange
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                AppointmentStart = DateTime.Now.AddDays(1),
                Note = "No services selected",
                ServiceIds = new List<Guid>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Please select at least one service.", exception.Message);
        }

        //=========================================================
        // UTCID02
        // AppointmentStart <= current time
        //=========================================================
        [Fact]
        public async Task UTCID02_BookAppointmentAsync_AppointmentStartInPast_ShouldThrowException()
        {
            // Arrange
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                AppointmentStart = DateTime.Now.AddMinutes(-5),
                Note = "Past appointment",
                ServiceIds = new List<Guid> { Guid.NewGuid() }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Appointment time must be in the future.", exception.Message);
        }

        //=========================================================
        // UTCID03
        // Number of service not matching
        //=========================================================
        [Fact]
        public async Task UTCID03_BookAppointmentAsync_NumberOfServicesNotMatching_ShouldThrowException()
        {
            // Arrange
            var serviceId1 = Guid.NewGuid();
            var serviceId2 = Guid.NewGuid();

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                AppointmentStart = DateTime.Now.AddDays(1),
                ServiceIds = new List<Guid> { serviceId1, serviceId2 }
            };

            var mockServicesFromDb = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId1, Price = 100, Duration = 30 }
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServicesFromDb);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("One or more services do not exist.", exception.Message);
        }

        //=========================================================
        // UTCID04
        // Vet is not working (schedule exception)
        //=========================================================
        [Fact]
        public async Task UTCID04_BookAppointmentAsync_VetNotWorkingException_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 150, Duration = 45 }
            };

            var staffException = new ScheduleException
            {
                ExceptionId = Guid.NewGuid(),
                StaffId = staffId,
                ExceptionDate = DateOnly.FromDateTime(appointmentStart),
                IsWorking = false
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync(staffException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Doctor is unavailable.", exception.Message);
        }
        //=========================================================
        // UTCID05
        // Appoinment stay out of vet working hours
        //=========================================================
        [Fact]
        public async Task UTCID05_BookAppointmentAsync_OutsideVetWorkingHours_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(7); // 7:00 AM ngày mai

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 } // Kết thúc lúc 7:30 AM
            };

            var staffException = new ScheduleException
            {
                ExceptionId = Guid.NewGuid(),
                StaffId = staffId,
                ExceptionDate = DateOnly.FromDateTime(appointmentStart),
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"), // Bác sĩ làm việc đột xuất bắt đầu từ 8:00 AM
                EndTime = TimeOnly.Parse("17:00")
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync(staffException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Appointment is outside working hours.", exception.Message);
        }

        //=========================================================
        // UTCID06
        // Clinic closed unexpectedly across the entire system (IsWorking = false)
        //=========================================================
        [Fact]
        public async Task UTCID06_BookAppointmentAsync_ClinicClosedUnexpectedly_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(14); // 2:00 PM ngày mai

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalException = new ScheduleException
            {
                ExceptionId = Guid.NewGuid(),
                StaffId = null,
                ExceptionDate = DateOnly.FromDateTime(appointmentStart),
                IsWorking = false,
                Reason = "Power Outage Crisis" // Hệ thống nghỉ đột xuất kèm lý do cụ thể
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync(globalException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Power Outage Crisis", exception.Message);
        }

        //=========================================================
        // UTCID07
        // Ad-hoc appointment scheduled outside the system's standard operating hours
        //=========================================================
        [Fact]
        public async Task UTCID07_BookAppointmentAsync_OutsideGlobalExceptionHours_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(19); // 7:00 PM ngày mai

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 60 } // Kết thúc lúc 8:00 PM
            };

            var globalException = new ScheduleException
            {
                ExceptionId = Guid.NewGuid(),
                ExceptionDate = DateOnly.FromDateTime(appointmentStart),
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("17:00") // Toàn hệ thống hôm đó chỉ mở cửa đột xuất đến 5:00 PM
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync(globalException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Appointment is outside working hours.", exception.Message);
        }

        //=========================================================
        // UTCID08
        // Schedule working is not found
        //=========================================================
        [Fact]
        public async Task UTCID08_BookAppointmentAsync_GlobalScheduleNotFound_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            // Không tìm thấy lịch làm việc hệ thống định kỳ (GlobalSchedule == null)
            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync((GlobalWorkSchedule?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Working schedule not found.", exception.Message);
        }
        //=========================================================
        // UTCID09
        // Clinic schedule is exception schedule(isworking = false)
        //=========================================================
        [Fact]
        public async Task UTCID09_BookAppointmentAsync_GlobalScheduleClosed_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = false // Ngày đóng cửa định kỳ của Clinic
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Clinic is closed.", exception.Message);
        }

        //=========================================================
        // UTCID10
        // The appointment time falls outside the clinic's regular operating hours.
        //=========================================================
        [Fact]
        public async Task UTCID10_BookAppointmentAsync_OutsideRegularClinicHours_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(22); // 10:00 PM ngày mai

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("20:00") // Lịch định kỳ kết thúc lúc 8:00 PM
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Appointment is outside working hours.", exception.Message);
        }

        //=========================================================
        // UTCID11
        // Doctor has a scheduling conflict
        //=========================================================
        [Fact]
        public async Task UTCID11_BookAppointmentAsync_DoctorConflict_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = Guid.NewGuid(),
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("21:00")
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            // Bác sĩ bị trùng lịch hẹn với khách hàng khác trong khoảng thời gian này
            _repositoryMock.Setup(x => x.IsTimeConflictAsync(staffId, appointmentStart, appointmentStart.AddMinutes(30)))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Doctor already has another appointment.", exception.Message);
        }

        //=========================================================
        // UTCID12
        // The pet (PetId) does not exist in the system.
        //=========================================================
        [Fact]
        public async Task UTCID12_BookAppointmentAsync_PetNotFound_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = petId,
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("21:00")
            };

            // FIX: Khởi tạo thực thể Appointment để tránh NullReferenceException tại AppointmentServices
            var appointmentEntity = new Appointment
            {
                AppointmentServices = new List<PetCenterAPI.Models.AppointmentService>()
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            _repositoryMock.Setup(x => x.IsTimeConflictAsync(staffId, appointmentStart, appointmentStart.AddMinutes(30)))
                .ReturnsAsync(false);

            // FIX: Setup Mapper để trả về thực thể vừa khởi tạo ở trên
            _mapperMock.Setup(x => x.Map<Appointment>(request))
                .Returns(appointmentEntity);

            _repositoryMock.Setup(x => x.GetPetForSnapshotAsync(petId))
                .ReturnsAsync((Pet?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Pet not found.", exception.Message);
        }

        //=========================================================
        // UTCID13
        // The vet (VetId) does not exist in the system.
        //=========================================================
        [Fact]
        public async Task UTCID13_BookAppointmentAsync_DoctorNotFound_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = petId,
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("21:00")
            };

            var pet = new Pet
            {
                PetId = petId,
                Species = "Dog"
            };

            // FIX: Khởi tạo thực thể Appointment để tránh NullReferenceException tại AppointmentServices
            var appointmentEntity = new Appointment
            {
                AppointmentServices = new List<PetCenterAPI.Models.AppointmentService>()
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            _repositoryMock.Setup(x => x.IsTimeConflictAsync(staffId, appointmentStart, appointmentStart.AddMinutes(30)))
                .ReturnsAsync(false);

            // FIX: Setup Mapper để trả về thực thể vừa khởi tạo ở trên
            _mapperMock.Setup(x => x.Map<Appointment>(request))
                .Returns(appointmentEntity);

            _repositoryMock.Setup(x => x.GetPetForSnapshotAsync(petId))
                .ReturnsAsync(pet);

            _repositoryMock.Setup(x => x.GetStaffForSnapshotAsync(staffId))
                .ReturnsAsync((Staff?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Doctor not found.", exception.Message);
        }


        //=========================================================
        // UTCID14
        // Data is fully valid -> Return booking successful
        //=========================================================
        [Fact]
        public async Task UTCID14_BookAppointmentAsync_FullyValidData_ShouldReturnResponse()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = petId,
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, ServiceName = "Grooming", Price = 200, Duration = 45, ServiceType = 2 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("21:00")
            };

            var pet = new Pet { PetId = petId, Species = "Cat", Breed = "Persian", Gender = "Female", Weight = 4 };
            var staff = new Staff { StaffId = staffId, FullName = "Dr. VinhTP" };

            var appointmentEntity = new Appointment
            {
                AppointmentServices = new List<PetCenterAPI.Models.AppointmentService>()
            };

            var responseDto = new AppointmentResponseDTO
            {
                AppointmentId = Guid.NewGuid()
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            _repositoryMock.Setup(x => x.IsTimeConflictAsync(staffId, appointmentStart, appointmentStart.AddMinutes(45)))
                .ReturnsAsync(false);

            _repositoryMock.Setup(x => x.GetPetForSnapshotAsync(petId))
                .ReturnsAsync(pet);

            _repositoryMock.Setup(x => x.GetStaffForSnapshotAsync(staffId))
                .ReturnsAsync(staff);

            _mapperMock.Setup(x => x.Map<Appointment>(request))
                .Returns(appointmentEntity);

            _mapperMock.Setup(x => x.Map<AppointmentResponseDTO>(appointmentEntity))
                .Returns(responseDto);

            // Act
            var result = await _service.BookAppointmentAsync(request);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(x => x.CreateAppointmentAsync(It.IsAny<Appointment>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        //=========================================================
        // UTCID15
        // Repository throws exception
        //=========================================================
        [Fact]
        public async Task UTCID15_BookAppointmentAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var appointmentStart = DateTime.Today.AddDays(1).AddHours(10);

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                PetId = petId,
                StaffId = staffId,
                AppointmentStart = appointmentStart,
                ServiceIds = new List<Guid> { serviceId }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, Price = 100, Duration = 30 }
            };

            var globalSchedule = new GlobalWorkSchedule
            {
                IsWorking = true,
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("21:00")
            };

            var pet = new Pet { PetId = petId, Species = "Dog" };
            var staff = new Staff { StaffId = staffId, FullName = "Dr. VinhTP" };

            var appointmentEntity = new Appointment
            {
                AppointmentServices = new List<PetCenterAPI.Models.AppointmentService>()
            };

            _repositoryMock.Setup(x => x.GetServicesAsync(request.ServiceIds))
                .ReturnsAsync(mockServices);

            _repositoryMock.Setup(x => x.GetStaffExceptionAsync(staffId, DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalExceptionAsync(DateOnly.FromDateTime(appointmentStart)))
                .ReturnsAsync((ScheduleException?)null);

            _repositoryMock.Setup(x => x.GetGlobalScheduleAsync(appointmentStart.DayOfWeek))
                .ReturnsAsync(globalSchedule);

            _repositoryMock.Setup(x => x.IsTimeConflictAsync(staffId, appointmentStart, appointmentStart.AddMinutes(30)))
                .ReturnsAsync(false);

            _repositoryMock.Setup(x => x.GetPetForSnapshotAsync(petId))
                .ReturnsAsync(pet);

            _repositoryMock.Setup(x => x.GetStaffForSnapshotAsync(staffId))
                .ReturnsAsync(staff);

            _mapperMock.Setup(x => x.Map<Appointment>(request))
                .Returns(appointmentEntity);

            // Giả lập lỗi hệ thống cơ sở dữ liệu khi lưu thay đổi
            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database Save failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.BookAppointmentAsync(request));

            Assert.Equal("Database Save failure", exception.Message);
        }

    }
}
