using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_GetAllAppointmentsAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_GetAllAppointmentsAsync()
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
        // Find existing list of all system appointments
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllAppointmentsAsync_ExistingAppointments_ShouldReturnMappedList()
        {
            // Arrange
            var appointmentId1 = Guid.NewGuid();
            var appointmentId2 = Guid.NewGuid();

            var mockAppointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = appointmentId1,
                    Status = 1,
                    Total = 500
                },
                new Appointment
                {
                    AppointmentId = appointmentId2,
                    Status = 3,
                    Total = 750
                }
            };

            var mappedResponse = new List<AppointmentListResponseDTO>
            {
                new AppointmentListResponseDTO
                {
                    AppointmentId = appointmentId1,
                    PetName = "Lucky",
                    VetName = "Dr. VinhTP",
                    AppointmentStart = DateTime.Now.AddDays(1),
                    AppointmentEnd = DateTime.Now.AddDays(1).AddHours(1),
                    Status = 1,
                    Total = 500
                },
                new AppointmentListResponseDTO
                {
                    AppointmentId = appointmentId2,
                    PetName = "LuLu",
                    VetName = "Dr. VinhTP",
                    AppointmentStart = DateTime.Now.AddDays(2),
                    AppointmentEnd = DateTime.Now.AddDays(2).AddMinutes(45),
                    Status = 3,
                    Total = 750
                }
            };

            _repositoryMock.Setup(x => x.GetAllAppointmentsAsync())
                .ReturnsAsync(mockAppointments);

            _mapperMock.Setup(x => x.Map<List<AppointmentListResponseDTO>>(mockAppointments))
                .Returns(mappedResponse);

            // Act
            var result = await _service.GetAllAppointmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(appointmentId1, result[0].AppointmentId);
            Assert.Equal("Lucky", result[0].PetName);
            Assert.Equal(500, result[0].Total);

            _repositoryMock.Verify(x => x.GetAllAppointmentsAsync(), Times.Once);
            _mapperMock.Verify(x => x.Map<List<AppointmentListResponseDTO>>(mockAppointments), Times.Once);
        }

        //=========================================================
        // UTCID02
        // System has zero appointments (Empty list)
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllAppointmentsAsync_NoAppointments_ShouldReturnEmptyList()
        {
            // Arrange
            var mockAppointments = new List<Appointment>();
            var mappedResponse = new List<AppointmentListResponseDTO>();

            _repositoryMock.Setup(x => x.GetAllAppointmentsAsync())
                .ReturnsAsync(mockAppointments);

            _mapperMock.Setup(x => x.Map<List<AppointmentListResponseDTO>>(mockAppointments))
                .Returns(mappedResponse);

            // Act
            var result = await _service.GetAllAppointmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _repositoryMock.Verify(x => x.GetAllAppointmentsAsync(), Times.Once);
            _mapperMock.Verify(x => x.Map<List<AppointmentListResponseDTO>>(mockAppointments), Times.Once);
        }

        //=========================================================
        // UTCID03
        // Repository throws database connection error
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAllAppointmentsAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetAllAppointmentsAsync())
                .ThrowsAsync(new Exception("Database connection failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAllAppointmentsAsync());

            Assert.Equal("Database connection failure.", exception.Message);
        }

        //=========================================================
        // UTCID04
        // Mapping error due to incorrect profile configuration
        //=========================================================
        [Fact]
        public async Task UTCID04_GetAllAppointmentsAsync_MapperThrows_ShouldThrowException()
        {
            // Arrange
            var mockAppointments = new List<Appointment>
            {
                new Appointment { AppointmentId = Guid.NewGuid() }
            };

            _repositoryMock.Setup(x => x.GetAllAppointmentsAsync())
                .ReturnsAsync(mockAppointments);

            _mapperMock.Setup(x => x.Map<List<AppointmentListResponseDTO>>(mockAppointments))
                .Throws(new Exception("AutoMapper mapping exception."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAllAppointmentsAsync());

            Assert.Equal("AutoMapper mapping exception.", exception.Message);
        }
    }
}
