using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_CompleteAppointmentService
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_CompleteAppointmentService()
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
        // Appointment service not found (Returns null from repository)
        //=========================================================
        [Fact]
        public async Task UTCID01_CompleteAppointmentService_ServiceNotFound_ShouldThrowException()
        {
            // Arrange
            var appointmentServiceId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId))
                .ReturnsAsync((PetCenterAPI.Models.AppointmentService?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CompleteAppointmentService(appointmentServiceId));

            Assert.Equal("Appointment service not found.", exception.Message);

            _repositoryMock.Verify(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID02
        // Valid data -> Update status to 2 and save successfully
        //=========================================================
        [Fact]
        public async Task UTCID02_CompleteAppointmentService_ValidData_ShouldUpdateStatusAndSave()
        {
            // Arrange
            var appointmentServiceId = Guid.NewGuid();

            var mockAppointmentService = new PetCenterAPI.Models.AppointmentService
            {
                AppointmentServiceId = appointmentServiceId,
                Status = 1,
                CompleteAt = null
            };

            _repositoryMock.Setup(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId))
                .ReturnsAsync(mockAppointmentService);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _service.CompleteAppointmentService(appointmentServiceId);

            // Assert
            Assert.Equal(2, mockAppointmentService.Status);
            Assert.NotNull(mockAppointmentService.CompleteAt);

            // Kiểm tra thời gian CompleteAt sát với thời gian hiện tại (trong vòng 1 giây)
            Assert.True((DateTime.UtcNow - mockAppointmentService.CompleteAt.Value).TotalSeconds < 1);

            _repositoryMock.Verify(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        //=========================================================
        // UTCID03
        // Repository throws database connection error on retrieval
        //=========================================================
        [Fact]
        public async Task UTCID03_CompleteAppointmentService_RepositoryThrowsOnGet_ShouldThrowException()
        {
            // Arrange
            var appointmentServiceId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId))
                .ThrowsAsync(new Exception("Database read failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CompleteAppointmentService(appointmentServiceId));

            Assert.Equal("Database read failure.", exception.Message);

            _repositoryMock.Verify(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID04
        // Repository throws database connection error on SaveChangesAsync
        //=========================================================
        [Fact]
        public async Task UTCID04_CompleteAppointmentService_RepositoryThrowsOnSave_ShouldThrowException()
        {
            // Arrange
            var appointmentServiceId = Guid.NewGuid();

            var mockAppointmentService = new PetCenterAPI.Models.AppointmentService
            {
                AppointmentServiceId = appointmentServiceId,
                Status = 1
            };

            _repositoryMock.Setup(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId))
                .ReturnsAsync(mockAppointmentService);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database save failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CompleteAppointmentService(appointmentServiceId));

            Assert.Equal("Database save failure.", exception.Message);

            _repositoryMock.Verify(x => x.GetAppointmentServiceByIdAsync(appointmentServiceId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
