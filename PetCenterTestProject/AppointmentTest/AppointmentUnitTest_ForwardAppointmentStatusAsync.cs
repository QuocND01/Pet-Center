using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_ForwardAppointmentStatusAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_ForwardAppointmentStatusAsync()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _petRepoMock = new Mock<IPetRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _mapperMock = new Mock<IMapper>();
            _scheduleRepoMock = new Mock<IScheduleRepository>();
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
        // Appointment not found (Returns null from repository)
        //=========================================================
        [Fact]
        public async Task UTCID01_ForwardAppointmentStatusAsync_AppointmentNotFound_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, staffId));

            Assert.Equal("Appointment not found.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID02
        // Staff is not assigned to this appointment
        //=========================================================
        [Fact]
        public async Task UTCID02_ForwardAppointmentStatusAsync_StaffNotAssigned_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var assignedStaffId = Guid.NewGuid();
            var requestStaffId = Guid.NewGuid(); // Bác sĩ khác gửi yêu cầu

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = assignedStaffId,
                Status = 1
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, requestStaffId));

            Assert.Equal("You are not assigned to this appointment.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID03
        // Appointment status is lower boundary check (Status == 0)
        //=========================================================
        [Fact]
        public async Task UTCID03_ForwardAppointmentStatusAsync_StatusIsZero_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = staffId,
                Status = 0 // Trạng thái đã hủy
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, staffId));

            Assert.Equal("Cannot forward a cancelled appointment.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID04
        // Appointment status is upper boundary check (Status == 4)
        //=========================================================
        [Fact]
        public async Task UTCID04_ForwardAppointmentStatusAsync_StatusIsFour_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = staffId,
                Status = 4 // Trạng thái đã hoàn thành
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, staffId));

            Assert.Equal("Invalid appointment status.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID05
        // Valid status progression check at lower bound (Status 1 -> 2)
        //=========================================================
        [Fact]
        public async Task UTCID05_ForwardAppointmentStatusAsync_FromStatusOne_ShouldIncrementStatus()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = staffId,
                Status = 1,
                UpdatedAt = null
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act
            await _service.ForwardAppointmentStatusAsync(appointmentId, staffId);

            // Assert
            Assert.Equal(2, mockAppointment.Status); // Tăng trạng thái lên 2
            Assert.NotNull(mockAppointment.UpdatedAt); // Được ghi nhận thời gian cập nhật

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        //=========================================================
        // UTCID06
        // Valid status progression check at upper bound (Status 3 -> 4)
        //=========================================================
        [Fact]
        public async Task UTCID06_ForwardAppointmentStatusAsync_FromStatusThree_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = staffId,
                Status = 3
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, staffId));

            Assert.Equal("Appointment is already completed.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID07
        // Repository throws database connection error on SaveChanges
        //=========================================================
        [Fact]
        public async Task UTCID07_ForwardAppointmentStatusAsync_RepositoryThrowsOnSave_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                StaffId = staffId,
                Status = 2
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database context update error."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ForwardAppointmentStatusAsync(appointmentId, staffId));

            Assert.Equal("Database context update error.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}