using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
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
    public class AppointmentUnitTest_GetBookingDataAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_GetBookingDataAsync()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _petRepoMock = new Mock<IPetRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _mapperMock = new Mock<IMapper>();
            _scheduleRepoMock = new Mock<IScheduleRepository>();
            _staffRepoMock = new Mock<IStaffRepository>();
            _staffRepoMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => new PetCenterAPI.Models.Staff { StaffId = id, IsActive = true });
            _staffRepoMock = new Mock<IStaffRepository>();
            _staffRepoMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => new PetCenterAPI.Models.Staff { StaffId = id, IsActive = true });

            _service = new PetCenterAPI.Service.AppointmentService(
                _repositoryMock.Object,
                _petRepoMock.Object,
                _serviceRepoMock.Object,
                _mapperMock.Object,
                NullLogger<PetCenterAPI.Service.AppointmentService>.Instance,
                _scheduleRepoMock.Object,
                _staffRepoMock.Object);
        }

        //=========================================================
        // UTCID01
        // Find full details (Pets, Staffs, Services)
        //=========================================================
        [Fact]
        public async Task UTCID01_GetBookingDataAsync_FullDataFound_ShouldReturnMappedResponse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var mockPets = new List<Pet>
            {
                new Pet { PetId = petId, Species = "Dog" }
            };

            var mockStaffs = new List<Staff>
            {
                new Staff { StaffId = staffId, FullName = "Dr. VinhTP" }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, ServiceName = "Surgical Care" }
            };

            var mappedPets = new List<BookingPetDTO>
            {
                new BookingPetDTO { PetId = petId, Species = "Dog" }
            };

            var mappedStaffs = new List<BookingStaffDTO>
            {
                new BookingStaffDTO { StaffId = staffId, FullName = "Dr. VinhTP" }
            };

            var mappedServices = new List<BookingServiceDTO>
            {
                new BookingServiceDTO { ServiceId = serviceId, ServiceName = "Surgical Care" }
            };

            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ReturnsAsync(mockPets);

            _repositoryMock.Setup(x => x.GetActiveVetsAsync())
                .ReturnsAsync(mockStaffs);

            _serviceRepoMock.Setup(x => x.GetAllActiveServicesAsync())
                .ReturnsAsync(mockServices);

            _mapperMock.Setup(x => x.Map<List<BookingPetDTO>>(mockPets))
                .Returns(mappedPets);

            _mapperMock.Setup(x => x.Map<List<BookingStaffDTO>>(mockStaffs))
                .Returns(mappedStaffs);

            _mapperMock.Setup(x => x.Map<List<BookingServiceDTO>>(mockServices))
                .Returns(mappedServices);

            // Act
            var result = await _service.GetBookingDataAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Pets);
            Assert.Single(result.Staffs);
            Assert.Single(result.Services);
            Assert.Equal("Dr. VinhTP", result.Staffs[0].FullName);
        }

        //=========================================================
        // UTCID02
        // Customer has no pets (Pets list is empty)
        //=========================================================
        [Fact]
        public async Task UTCID02_GetBookingDataAsync_CustomerHasNoPets_ShouldReturnEmptyPetsList()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();

            var mockPets = new List<Pet>(); // Khách hàng chưa đăng ký thú cưng nào

            var mockStaffs = new List<Staff>
            {
                new Staff { StaffId = staffId, FullName = "Dr. VinhTP" }
            };

            var mockServices = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service { ServiceId = serviceId, ServiceName = "Grooming" }
            };

            var mappedPets = new List<BookingPetDTO>();
            var mappedStaffs = new List<BookingStaffDTO>
            {
                new BookingStaffDTO { StaffId = staffId, FullName = "Dr. VinhTP" }
            };
            var mappedServices = new List<BookingServiceDTO>
            {
                new BookingServiceDTO { ServiceId = serviceId, ServiceName = "Grooming" }
            };

            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ReturnsAsync(mockPets);

            _repositoryMock.Setup(x => x.GetActiveVetsAsync())
                .ReturnsAsync(mockStaffs);

            _serviceRepoMock.Setup(x => x.GetAllActiveServicesAsync())
                .ReturnsAsync(mockServices);

            _mapperMock.Setup(x => x.Map<List<BookingPetDTO>>(mockPets))
                .Returns(mappedPets);

            _mapperMock.Setup(x => x.Map<List<BookingStaffDTO>>(mockStaffs))
                .Returns(mappedStaffs);

            _mapperMock.Setup(x => x.Map<List<BookingServiceDTO>>(mockServices))
                .Returns(mappedServices);

            // Act
            var result = await _service.GetBookingDataAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Pets); // Danh sách Pet rỗng đúng kỳ vọng
            Assert.Single(result.Staffs);
            Assert.Single(result.Services);
        }

        //=========================================================
        // UTCID03
        // Clinic empty schedules (No active Staffs or Services)
        //=========================================================
        [Fact]
        public async Task UTCID03_GetBookingDataAsync_NoActiveStaffsOrServices_ShouldReturnEmptyLists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();

            var mockPets = new List<Pet>
            {
                new Pet { PetId = petId, Species = "Cat" }
            };

            var mockStaffs = new List<Staff>(); // Hệ thống không có bác sĩ hoạt động
            var mockServices = new List<PetCenterAPI.Models.Service>(); // Không có dịch vụ hoạt động

            var mappedPets = new List<BookingPetDTO>
            {
                new BookingPetDTO { PetId = petId, Species = "Cat" }
            };
            var mappedStaffs = new List<BookingStaffDTO>();
            var mappedServices = new List<BookingServiceDTO>();

            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ReturnsAsync(mockPets);

            _repositoryMock.Setup(x => x.GetActiveVetsAsync())
                .ReturnsAsync(mockStaffs);

            _serviceRepoMock.Setup(x => x.GetAllActiveServicesAsync())
                .ReturnsAsync(mockServices);

            _mapperMock.Setup(x => x.Map<List<BookingPetDTO>>(mockPets))
                .Returns(mappedPets);

            _mapperMock.Setup(x => x.Map<List<BookingStaffDTO>>(mockStaffs))
                .Returns(mappedStaffs);

            _mapperMock.Setup(x => x.Map<List<BookingServiceDTO>>(mockServices))
                .Returns(mappedServices);

            // Act
            var result = await _service.GetBookingDataAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Pets);
            Assert.Empty(result.Staffs); // Danh sách bác sĩ rỗng
            Assert.Empty(result.Services); // Danh sách dịch vụ rỗng
        }

        //=========================================================
        // UTCID04
        // All system database tables are empty
        //=========================================================
        //=========================================================
        // UTCID04
        // All system database tables are empty
        //=========================================================
        [Fact]
        public async Task UTCID04_GetBookingDataAsync_AllTablesEmpty_ShouldReturnAllListsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Khai báo danh sách rỗng từ cơ sở dữ liệu cho cả 3 bảng dữ liệu chính
            var mockPets = new List<Pet>();
            var mockStaffs = new List<Staff>();
            var mockServices = new List<PetCenterAPI.Models.Service>();

            // Định nghĩa danh sách DTO rỗng sau khi đi qua AutoMapper
            var mappedPets = new List<BookingPetDTO>();
            var mappedStaffs = new List<BookingStaffDTO>();
            var mappedServices = new List<BookingServiceDTO>();

            // Setup các hàm gọi Repository trả về danh sách trống tương ứng
            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ReturnsAsync(mockPets);

            _repositoryMock.Setup(x => x.GetActiveVetsAsync())
                .ReturnsAsync(mockStaffs);

            _serviceRepoMock.Setup(x => x.GetAllActiveServicesAsync())
                .ReturnsAsync(mockServices);

            // Setup AutoMapper ánh xạ chính xác các tập dữ liệu rỗng này
            _mapperMock.Setup(x => x.Map<List<BookingPetDTO>>(mockPets))
                .Returns(mappedPets);

            _mapperMock.Setup(x => x.Map<List<BookingStaffDTO>>(mockStaffs))
                .Returns(mappedStaffs);

            _mapperMock.Setup(x => x.Map<List<BookingServiceDTO>>(mockServices))
                .Returns(mappedServices);

            // Act
            var result = await _service.GetBookingDataAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Pets);
            Assert.Empty(result.Staffs);
            Assert.Empty(result.Services);

            // Xác minh hệ thống gọi đủ và đúng 1 lần các Repository liên quan
            _petRepoMock.Verify(x => x.GetPetsByCustomerIdAsync(customerId), Times.Once);
            _repositoryMock.Verify(x => x.GetActiveVetsAsync(), Times.Once);
            _serviceRepoMock.Verify(x => x.GetAllActiveServicesAsync(), Times.Once);
        }
        //=========================================================
        // UTCID05
        // Database connection error while retrieving data
        //=========================================================
        [Fact]
        public async Task UTCID05_GetBookingDataAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Giả lập lỗi kết nối cơ sở dữ liệu xảy ra ở một trong các lệnh gọi Repository
            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ThrowsAsync(new Exception("Database connection failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetBookingDataAsync(customerId));

            Assert.Equal("Database connection failure.", exception.Message);
        }

        //=========================================================
        // UTCID06
        // Mapping error due to incorrect profile configuration
        //=========================================================
        [Fact]
        public async Task UTCID06_GetBookingDataAsync_MapperThrows_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();

            var mockPets = new List<Pet>
            {
                new Pet { PetId = petId, Species = "Dog" }
            };

            var mockStaffs = new List<Staff>();
            var mockServices = new List<PetCenterAPI.Models.Service>();

            _petRepoMock.Setup(x => x.GetPetsByCustomerIdAsync(customerId))
                .ReturnsAsync(mockPets);

            _repositoryMock.Setup(x => x.GetActiveVetsAsync())
                .ReturnsAsync(mockStaffs);

            _serviceRepoMock.Setup(x => x.GetAllActiveServicesAsync())
                .ReturnsAsync(mockServices);

            // Giả lập trường hợp lỗi cấu hình Mapper khi chuyển đổi dữ liệu
            _mapperMock.Setup(x => x.Map<List<BookingPetDTO>>(mockPets))
                .Throws(new Exception("AutoMapper mapping exception."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetBookingDataAsync(customerId));

            Assert.Equal("AutoMapper mapping exception.", exception.Message);
        }
    }
}

    

    