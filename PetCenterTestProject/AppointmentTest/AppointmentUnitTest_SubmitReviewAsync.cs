using Microsoft.Extensions.Logging.Abstractions;
﻿using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests; // Thay đổi theo namespace chứa SubmitReviewRequestDTO của bạn
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    public class AppointmentUnitTest_SubmitReviewAsync
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<IStaffRepository> _staffRepoMock;
        private readonly PetCenterAPI.Service.AppointmentService _service;

        public AppointmentUnitTest_SubmitReviewAsync()
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
        public async Task UTCID01_SubmitReviewAsync_AppointmentNotFound_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 5,
                Feedback = "Great service!"
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync((Appointment?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("Appointment not found.", exception.Message);
        }

        //=========================================================
        // UTCID02
        // Appointment belongs to another customer
        //=========================================================
        [Fact]
        public async Task UTCID02_SubmitReviewAsync_WrongCustomer_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var dbCustomerId = Guid.NewGuid(); // ID khách hàng khác trong DB

            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 5,
                Feedback = "Good clinic"
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = dbCustomerId
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("You are not allowed to review this appointment.", exception.Message);
        }

        //=========================================================
        // UTCID03
        // Appointment is not completed (Status != 4)
        //=========================================================
        [Fact]
        public async Task UTCID03_SubmitReviewAsync_AppointmentNotCompleted_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 5,
                Feedback = "Nice staff"
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = customerId,
                Status = 1 // Trạng thái chưa hoàn tất (ví dụ: Chờ khám)
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("Only completed appointments can be reviewed.", exception.Message);
        }

        //=========================================================
        // UTCID04
        // Appointment snapshot not found (Snapshot is null)
        //=========================================================
        [Fact]
        public async Task UTCID04_SubmitReviewAsync_SnapshotNotFound_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 4,
                Feedback = "Good doctor"
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = customerId,
                Status =  3,
                AppointmentSnapshot = null // Lỗi hệ thống: Snapshot bị null
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("Appointment snapshot not found.", exception.Message);
        }

        //=========================================================
        // UTCID05
        // Rating below minimum boundary (Rating == 0)
        //=========================================================
        [Fact]
        public async Task UTCID05_SubmitReviewAsync_RatingBelowMinimum_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 0, // Nhỏ hơn biên dưới quy định
                Feedback = "Bad score"
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = customerId,
                Status = 4,
                AppointmentSnapshot = new AppointmentSnapshot()
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("Rating must be between 1 and 5.", exception.Message);
        }

        //=========================================================
        // UTCID06
        // Rating above maximum boundary (Rating == 6)
        //=========================================================
        [Fact]
        public async Task UTCID06_SubmitReviewAsync_RatingAboveMaximum_ShouldThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 6, // Lớn hơn biên trên quy định
                Feedback = "Too high score"
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = customerId,
                Status = 4,
                AppointmentSnapshot = new AppointmentSnapshot()
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                .ReturnsAsync(mockAppointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.SubmitReviewAsync(customerId, request));

            Assert.Equal("Rating must be between 1 and 5.", exception.Message);
        }

        //=========================================================
        // UTCID07
        // Data is fully valid -> Submit review successful
        //=========================================================
        [Fact]
        public async Task UTCID07_SubmitReviewAsync_FullyValidData_ShouldSaveReviewSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new SubmitReviewRequestDTO
            {
                AppointmentId = Guid.NewGuid(),
                Rating = 5,
                Feedback = "Absolutely wonderful care!"
            };

            var mockSnapshot = new AppointmentSnapshot
            {
                Rating = 0,
                Feedback = string.Empty
            };

            var mockAppointment = new Appointment
            {
                AppointmentId = request.AppointmentId,
                CustomerId = customerId,
                Status =  3,
                AppointmentSnapshot = mockSnapshot
            };

            _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId)).ReturnsAsync(mockAppointment);
            await _service.SubmitReviewAsync(customerId, request);
            // Assert
            Assert.Equal(5, mockSnapshot.Rating);
            Assert.Equal("Absolutely wonderful care!", mockSnapshot.Feedback);
            _repositoryMock.Verify(x => x.GetAppointmentDetailAsync(request.AppointmentId), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
            //=========================================================
            // UTCID08
            // Repository throws database connection error on SaveChangesAsync
            //=========================================================
            [Fact]
            public async Task UTCID08_SubmitReviewAsync_RepositoryThrows_ShouldThrowException()
            {
                // Arrange
                var customerId = Guid.NewGuid();
                var appointmentId = Guid.NewGuid();

                var request = new SubmitReviewRequestDTO
                {
                    AppointmentId = appointmentId,
                    Rating = 5,
                    Feedback = "Excellent service!"
                };

                var mockAppointment = new Appointment
                {
                    AppointmentId = appointmentId,
                    CustomerId = customerId,
                    Status =  3, // Trạng thái đã hoàn thành hợp lệ
                    AppointmentSnapshot = new AppointmentSnapshot
                    {
                        AppointmentSnapshotId = Guid.NewGuid(),
                        AppointmentId = appointmentId,
                        Rating = 0,
                        Feedback = null
                    }
                };

                // Setup hàm lấy chi tiết lịch hẹn trả về đối tượng hợp lệ
                _repositoryMock.Setup(x => x.GetAppointmentDetailAsync(request.AppointmentId))
                    .ReturnsAsync(mockAppointment);

                // Giả lập lỗi hệ thống cơ sở dữ liệu khi gọi lệnh SaveChangesAsync
                _repositoryMock.Setup(x => x.SaveChangesAsync())
                    .ThrowsAsync(new Exception("Database connection failure."));

                // Act & Assert
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => _service.SubmitReviewAsync(customerId, request));

                Assert.Equal("Database connection failure.", exception.Message);

                // Kiểm tra xem dữ liệu tạm thời đã được gán vào thực thể trước khi crash chưa
                Assert.Equal(5, mockAppointment.AppointmentSnapshot.Rating);
                Assert.Equal("Excellent service!", mockAppointment.AppointmentSnapshot.Feedback);

                // Xác minh hệ thống gọi đủ lệnh đọc và lệnh lưu dữ liệu
                _repositoryMock.Verify(x => x.GetAppointmentDetailAsync(request.AppointmentId), Times.Once);
                _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            }
        }
    }
