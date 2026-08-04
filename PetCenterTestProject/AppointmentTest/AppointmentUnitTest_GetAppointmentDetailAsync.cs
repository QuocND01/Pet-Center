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
    public class AppointmentUnitTest_GetAppointmentDetailAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_GetAppointmentDetailAsync()
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
        // Find existing appointment detail with full child objects
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAppointmentDetailAsync_FullDataExists_ShouldReturnMappedDetails()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                Status = 1,
                Total = 450
            };

            var mappedResponse = new AppointmentResponseDTO
            {
                AppointmentId = appointmentId,
                PetName = "Shiba",
                VetName = "Dr. VinhTP",
                Status = 1,
                Total = 450,
                AppointmentServices = new List<AppointmentServiceResponseDTO>
                {
                    new AppointmentServiceResponseDTO
                    {
                        AppointmentServiceId = Guid.NewGuid(),
                        ServiceName = "Grooming",
                        Price = 450,
                        Duration = 60
                    }
                },
                Snapshot = new AppointmentSnapshotResponseDTO
                {
                    Species = "Dog",
                    Breed = "Inu",
                    Gender = "Male",
                    Weight = 12.5m,
                    VetName = "Dr. VinhTP"
                }
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _mapperMock.Setup(x => x.Map<AppointmentResponseDTO>(mockAppointment))
                .Returns(mappedResponse);

            // Act
            var result = await _service.GetAppointmentDetailAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointmentId, result.AppointmentId);
            Assert.Equal("Shiba", result.PetName);
            Assert.Single(result.AppointmentServices);
            Assert.NotNull(result.Snapshot);
            Assert.Equal("Dog", result.Snapshot.Species);

            _repositoryMock.Verify(x => x.GetAppointmentDetailAsync(appointmentId), Times.Once);
            _mapperMock.Verify(x => x.Map<AppointmentResponseDTO>(mockAppointment), Times.Once);
        }

        //=========================================================
        // UTCID02
        // Appointment exists but child objects are empty/null
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAppointmentDetailAsync_MinimalDataExists_ShouldReturnMappedDetails()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                Status = 0,
                Total = 0
            };

            var mappedResponse = new AppointmentResponseDTO
            {
                AppointmentId = appointmentId,
                Status = 0,
                Total = 0,
                AppointmentServices = new List<AppointmentServiceResponseDTO>(), // Danh sách rỗng
                Snapshot = null // Snapshot bị null
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _mapperMock.Setup(x => x.Map<AppointmentResponseDTO>(mockAppointment))
                .Returns(mappedResponse);

            // Act
            var result = await _service.GetAppointmentDetailAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointmentId, result.AppointmentId);
            Assert.Empty(result.AppointmentServices);
            Assert.Null(result.Snapshot);

            _repositoryMock.Verify(x => x.GetAppointmentDetailAsync(appointmentId), Times.Once);
            _mapperMock.Verify(x => x.Map<AppointmentResponseDTO>(mockAppointment), Times.Once);
        }

        //=========================================================
        // UTCID03
        // Appointment not found (Returns null from repository)
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAppointmentDetailAsync_AppointmentNotFound_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(appointmentId))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAppointmentDetailAsync(appointmentId));

            Assert.Equal("Appointment not found.", exception.Message);

            _repositoryMock.Verify(x => x.GetAppointmentDetailAsync(appointmentId), Times.Once);
            _mapperMock.Verify(x => x.Map<AppointmentResponseDTO>(It.IsAny<Appointment>()), Times.Never);
        }

        //=========================================================
        // UTCID04
        // Repository throws database connection error
        //=========================================================
        [Fact]
        public async Task UTCID04_GetAppointmentDetailAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(appointmentId))
                .ThrowsAsync(new Exception("Database connection failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAppointmentDetailAsync(appointmentId));

            Assert.Equal("Database connection failure.", exception.Message);
        }

        //=========================================================
        // UTCID05
        // Mapping error due to incorrect profile configuration
        //=========================================================
        [Fact]
        public async Task UTCID05_GetAppointmentDetailAsync_MapperThrows_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _mapperMock.Setup(x => x.Map<AppointmentResponseDTO>(mockAppointment))
                .Throws(new Exception("AutoMapper mapping exception."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAppointmentDetailAsync(appointmentId));

            Assert.Equal("AutoMapper mapping exception.", exception.Message);
        }
    }
}
