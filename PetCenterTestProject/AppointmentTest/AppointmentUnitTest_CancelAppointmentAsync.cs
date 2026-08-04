using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_CancelAppointmentAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_CancelAppointmentAsync()
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
        // Appointment not found (Returns null from repository)
        //=========================================================
        [Fact]
        public async Task UTCID01_CancelAppointmentAsync_AppointmentNotFound_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, customerId));

            Assert.Equal("Appointment not found.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID02
        // Customer is not the owner of the appointment
        //=========================================================
        [Fact]
        public async Task UTCID02_CancelAppointmentAsync_NotAllowedCustomer_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var requestCustomerId = Guid.NewGuid();
            var actualOwnerId = Guid.NewGuid(); // ID khác với ID yêu cầu hủy

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = actualOwnerId,
                Status = 1
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, requestCustomerId));

            Assert.Equal("You are not allowed to cancel this appointment.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID03
        // Appointment already cancelled (Status == 0)
        //=========================================================
        [Fact]
        public async Task UTCID03_CancelAppointmentAsync_AlreadyCancelled_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = customerId,
                Status = 0 // Đã hủy từ trước
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, customerId));

            Assert.Equal("Appointment is already cancelled.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID04
        // Appointment is in progress (Status == 3)
        //=========================================================
        [Fact]
        public async Task UTCID04_CancelAppointmentAsync_InProgress_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = customerId,
                Status = 3 // Đang tiến hành khám
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, customerId));

            Assert.Equal("Appointment has already been completed.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID05
        // Appointment already completed (Status == 4)
        //=========================================================
        [Fact]
        public async Task UTCID05_CancelAppointmentAsync_AlreadyCompleted_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = customerId,
                Status = 4 // Đã hoàn thành khám
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, customerId));

            Assert.Equal("Cannot cancel appointment with current status.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        //=========================================================
        // UTCID06
        // Fully valid data -> Successfully cancel the appointment
        //=========================================================
        [Fact]
        public async Task UTCID06_CancelAppointmentAsync_ValidCancellation_ShouldUpdateStatusAndSave()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = customerId,
                Status = 1, // Trạng thái chờ khám thông thường
                UpdatedAt = null
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _service.CancelAppointmentAsync(appointmentId, customerId);

            // Assert
            Assert.Equal(0, mockAppointment.Status); // Kiểm tra status đã về 0
            Assert.NotNull(mockAppointment.UpdatedAt); // Kiểm tra thời gian cập nhật đã được ghi nhận

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        //=========================================================
        // UTCID07
        // Repository throws database connection error on SaveChanges
        //=========================================================
        [Fact]
        public async Task UTCID07_CancelAppointmentAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockAppointment = new Appointment
            {
                AppointmentId = appointmentId,
                CustomerId = customerId,
                Status = 1
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(appointmentId))
                .ReturnsAsync(mockAppointment);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database connection failure."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.CancelAppointmentAsync(appointmentId, customerId));

            Assert.Equal("Database connection failure.", exception.Message);

            _repositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}