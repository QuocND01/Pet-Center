using PetCenterAPI.Service.Interface;
using PetCenterAPI.DTOs.Responses.Order;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.Hubs;
using Microsoft.EntityFrameworkCore;
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.DTOs.Requests.ManageVoucher;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;

namespace PetCenterTestProject.VoucherTest
{
    public class VoucherTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<IVoucherRepository> _voucherRepositoryMock;
        private readonly VoucherService _service;

        //=========================================================
        // Constructor
        //=========================================================
        public VoucherTest_Mock()
        {
            _voucherRepositoryMock = new Mock<IVoucherRepository>();

            _service = new VoucherService(_voucherRepositoryMock.Object);
        }

        //=========================================================
        // Helper: Build a Voucher entity, override fields as needed
        //=========================================================
        private Voucher BuildVoucher(
            Guid? voucherId = null,
            string code = "SALE10",
            int? discountPercent = 10,
            bool? isActive = true,
            DateTime? expiredDate = null,
            decimal? minOrderAmount = 100000,
            decimal? maxDiscountAmount = 50000,
            int? useageLimit = 100,
            DateTime? createAt = null,
            string? description = "10% off")
        {
            return new Voucher
            {
                VoucherId = voucherId ?? Guid.NewGuid(),
                Code = code,
                DiscountPercent = discountPercent,
                IsActive = isActive,
                ExpiredDate = expiredDate ?? DateTime.UtcNow.AddDays(30), // mặc định chưa hết hạn
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                UseageLimit = useageLimit,
                CreateAt = createAt ?? DateTime.UtcNow,
                Description = description
            };
        }

        //=========================================================
        // Helper: Build a valid CreateVoucherRequestDTO, override fields as needed
        //=========================================================
        private CreateVoucherRequestDTO BuildCreateVoucherRequest(
            string code = "SALE10",
            int discountPercent = 10,
            string? description = "10% off",
            decimal minOrderAmount = 100000,
            decimal maxDiscountAmount = 50000,
            int? useageLimit = 100,
            DateTime? expiredDate = null)
        {
            return new CreateVoucherRequestDTO
            {
                Code = code,
                DiscountPercent = discountPercent,
                Description = description,
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                UseageLimit = useageLimit,
                ExpiredDate = expiredDate ?? DateTime.UtcNow.AddDays(30) // mặc định chưa hết hạn
            };
        }

        //=========================================================
        // Helper: Chạy DataAnnotations validation thủ công trên DTO
        //=========================================================
        private List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        //=========================================================
        // Helper: Setup CodeExistsAsync mock
        //=========================================================
        private void SetupCodeExists(bool exists)
        {
            _voucherRepositoryMock
                .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
                .ReturnsAsync(exists);
        }

        //=========================================================
        // Helper: Setup CreateAsync mock trả về entity đã gán VoucherId,
        // mô phỏng đúng hành vi repository thật
        //=========================================================
        private void SetupCreateAsyncSuccess()
        {
            _voucherRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Voucher>()))
                .ReturnsAsync((Voucher v) =>
                {
                    v.VoucherId = Guid.NewGuid();
                    v.CreateAt = DateTime.UtcNow;
                    v.IsActive = true;
                    return v;
                });
        }

        //=========================================================
        // UTCID01 - Single voucher, UsedCount = 0, IsActive = true, Not Expired
        // Expected: DTO list returned with mapped UsedCount = 0
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllAsync_SingleVoucherUsedCountZeroActive_ReturnMappedList()
        {
            // Arrange
            var voucher = BuildVoucher(
                isActive: true,
                expiredDate: DateTime.UtcNow.AddDays(10));

            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher> { voucher });
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(0);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(voucher.VoucherId, result[0].VoucherId);
            Assert.Equal(voucher.Code, result[0].Code);
            Assert.Equal(0, result[0].UsedCount);
            Assert.True(result[0].IsActive);
        }

        //=========================================================
        // UTCID02 - Single voucher, UsedCount > 0, IsActive = false, Not Expired
        // Expected: DTO list returned with mapped UsedCount
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllAsync_SingleVoucherUsedCountPositiveInactive_ReturnMappedList()
        {
            // Arrange
            var voucher = BuildVoucher(
                isActive: false,
                expiredDate: DateTime.UtcNow.AddDays(10));

            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher> { voucher });
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(5);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(5, result[0].UsedCount);
            Assert.False(result[0].IsActive);
        }

        //=========================================================
        // UTCID03 - Single voucher, UsedCount > 0, Voucher Expired
        // Expected: DTO list returned with mapped UsedCount, ExpiredDate preserved
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAllAsync_SingleVoucherExpired_ReturnMappedList()
        {
            // Arrange
            var expiredDate = DateTime.UtcNow.AddDays(-5); // đã hết hạn
            var voucher = BuildVoucher(expiredDate: expiredDate);

            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher> { voucher });
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(3);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].UsedCount);
            Assert.Equal(expiredDate, result[0].ExpiredDate);
        }

        //=========================================================
        // UTCID04 - Multiple vouchers returned, UsedCount > 0 each, Not Expired
        // Expected: DTO list returned, mỗi voucher lấy đúng UsedCount riêng
        //=========================================================
        [Fact]
        public async Task UTCID04_GetAllAsync_MultipleVouchers_ReturnMappedListWithCorrectUsedCountEach()
        {
            // Arrange
            var voucherA = BuildVoucher(code: "SALE10", expiredDate: DateTime.UtcNow.AddDays(10));
            var voucherB = BuildVoucher(code: "SALE20", expiredDate: DateTime.UtcNow.AddDays(20));

            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher> { voucherA, voucherB });
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucherA.VoucherId))
                .ReturnsAsync(2);
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucherB.VoucherId))
                .ReturnsAsync(7);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            var mappedA = result.First(r => r.VoucherId == voucherA.VoucherId);
            var mappedB = result.First(r => r.VoucherId == voucherB.VoucherId);
            Assert.Equal(2, mappedA.UsedCount);
            Assert.Equal(7, mappedB.UsedCount);
            _voucherRepositoryMock.Verify(x => x.GetUsedCountAsync(voucherA.VoucherId), Times.Once);
            _voucherRepositoryMock.Verify(x => x.GetUsedCountAsync(voucherB.VoucherId), Times.Once);
        }

        //=========================================================
        // UTCID05 - Single voucher, UsedCount > 0, Not Expired
        // Expected: DTO list returned with mapped UsedCount
        //=========================================================
        [Fact]
        public async Task UTCID05_GetAllAsync_SingleVoucherUsedCountPositiveNotExpired_ReturnMappedList()
        {
            // Arrange
            var voucher = BuildVoucher(
                code: "SALE30",
                expiredDate: DateTime.UtcNow.AddDays(15));

            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher> { voucher });
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(1);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].UsedCount);
            Assert.Equal(voucher.Description, result[0].Description);
        }

        //=========================================================
        // UTCID06 - Repository GetAllAsync throws exception
        // Expected: Exception thrown (Service không try-catch)
        //=========================================================
        [Fact]
        public async Task UTCID06_GetAllAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync());

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
            _voucherRepositoryMock.Verify(
                x => x.GetUsedCountAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        // UTCID07 - Repository GetAllAsync returns empty list
        // Expected: Empty DTO list returned, GetUsedCountAsync never called
        //=========================================================
        [Fact]
        public async Task UTCID07_GetAllAsync_RepositoryReturnsEmptyList_ReturnEmptyList()
        {
            // Arrange
            _voucherRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Voucher>());

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Empty(result);
            _voucherRepositoryMock.Verify(
                x => x.GetUsedCountAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        //=========================================================
        // ToggleStatusAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Voucher not found
        // Expected: (false, "Voucher not found.")
        //=========================================================
        [Fact]
        public async Task UTCID01_ToggleStatusAsync_VoucherNotFound_ReturnNotFound()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucherId))
                .ReturnsAsync((Voucher?)null);

            // Act
            var result = await _service.ToggleStatusAsync(voucherId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Voucher not found.", result.Message);
        }

        //=========================================================
        // UTCID02 - Voucher exists, Activate, Expired
        // Expected: (false, "Cannot activate an expired voucher. Please update the expiry date first.")
        //=========================================================
        [Fact]
        public async Task UTCID02_ToggleStatusAsync_ActivateExpiredVoucher_ReturnCannotActivateExpired()
        {
            // Arrange
            var voucher = BuildVoucher(expiredDate: DateTime.UtcNow.AddDays(-1)); // đã hết hạn

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);

            // Act
            var result = await _service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "Cannot activate an expired voucher. Please update the expiry date first.",
                result.Message);
            _voucherRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID03 - Voucher exists, Activate, Not Expired (Future),
        //           Has UsageLimit, Reached usage limit
        // Expected: (false, "Cannot activate voucher that has reached its usage limit.")
        //=========================================================
        [Fact]
        public async Task UTCID03_ToggleStatusAsync_ActivateUsageLimitReached_ReturnCannotActivateLimitReached()
        {
            // Arrange
            var voucher = BuildVoucher(
                expiredDate: DateTime.UtcNow.AddDays(10), // chưa hết hạn
                useageLimit: 5);

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(5); // đã đạt giới hạn

            // Act
            var result = await _service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "Cannot activate voucher that has reached its usage limit.",
                result.Message);
            _voucherRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID04 - Voucher exists, Activate, Future expiry,
        //           Has UsageLimit, Below usage limit, Update succeeds
        // Expected: (true, "Voucher activated.")
        //=========================================================
        [Fact]
        public async Task UTCID04_ToggleStatusAsync_ActivateBelowUsageLimit_ReturnActivatedSuccess()
        {
            // Arrange
            var voucher = BuildVoucher(
                isActive: false,
                expiredDate: DateTime.UtcNow.AddDays(10),
                useageLimit: 5);

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);
            _voucherRepositoryMock
                .Setup(x => x.GetUsedCountAsync(voucher.VoucherId))
                .ReturnsAsync(3); // dưới giới hạn
            _voucherRepositoryMock
                .Setup(x => x.UpdateAsync(voucher))
                .ReturnsAsync(voucher);

            // Act
            var result = await _service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher activated.", result.Message);
            Assert.True(voucher.IsActive);
            _voucherRepositoryMock.Verify(x => x.UpdateAsync(voucher), Times.Once);
        }

        //=========================================================
        // UTCID05 - Voucher exists, Activate, Null expiry,
        //           No UsageLimit, Update succeeds
        // Expected: (true, "Voucher activated.")
        //=========================================================
        [Fact]
        public async Task UTCID05_ToggleStatusAsync_ActivateNoExpiryNoUsageLimit_ReturnActivatedSuccess()
        {
            // Arrange
            var voucher = BuildVoucher(
                isActive: false,
                expiredDate: null,
                useageLimit: null);

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);
            _voucherRepositoryMock
                .Setup(x => x.UpdateAsync(voucher))
                .ReturnsAsync(voucher);

            // Act
            var result = await _service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher activated.", result.Message);
            Assert.True(voucher.IsActive);
            _voucherRepositoryMock.Verify(
                x => x.GetUsedCountAsync(It.IsAny<Guid>()), Times.Never);
            _voucherRepositoryMock.Verify(x => x.UpdateAsync(voucher), Times.Once);
        }

        //=========================================================
        // UTCID06 - Voucher exists, Deactivate, Update succeeds
        // Expected: (true, "Voucher deactivated.")
        //=========================================================
        [Fact]
        public async Task UTCID06_ToggleStatusAsync_Deactivate_ReturnDeactivatedSuccess()
        {
            // Arrange
            var voucher = BuildVoucher(isActive: true);

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);
            _voucherRepositoryMock
                .Setup(x => x.UpdateAsync(voucher))
                .ReturnsAsync(voucher);

            // Act
            var result = await _service.ToggleStatusAsync(voucher.VoucherId, false);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher deactivated.", result.Message);
            Assert.False(voucher.IsActive);
            _voucherRepositoryMock.Verify(
                x => x.GetUsedCountAsync(It.IsAny<Guid>()), Times.Never);
            _voucherRepositoryMock.Verify(x => x.UpdateAsync(voucher), Times.Once);
        }

        //=========================================================
        // UTCID07 - Voucher exists, UpdateAsync throws
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID07_ToggleStatusAsync_UpdateAsyncThrows_ThrowException()
        {
            // Arrange
            var voucher = BuildVoucher(isActive: true);

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucher.VoucherId))
                .ReturnsAsync(voucher);
            _voucherRepositoryMock
                .Setup(x => x.UpdateAsync(voucher))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ToggleStatusAsync(voucher.VoucherId, false));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID08 - GetByIdAsync throws
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID08_ToggleStatusAsync_GetByIdAsyncThrows_ThrowException()
        {
            // Arrange
            var voucherId = Guid.NewGuid();

            _voucherRepositoryMock
                .Setup(x => x.GetByIdAsync(voucherId))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ToggleStatusAsync(voucherId, true));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // CreateVoucherRequestDTO Validation (DataAnnotations)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - All fields empty/default
        //=========================================================
        [Fact]
        public void UTCID01_CreateVoucherRequestDTO_AllFieldsEmpty_ReturnRequiredAndRangeErrors()
        {
            // Arrange
            var request = new CreateVoucherRequestDTO
            {
                Code = "",
                DiscountPercent = 0,
                Description = null,
                MinOrderAmount = 0,
                MaxDiscountAmount = 0,
                UseageLimit = null,
                ExpiredDate = null
            };

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Code is required.", messages);
            Assert.Contains("Discount must be between 1% and 80%.", messages);
            Assert.Contains("Min order amount must be between 1₫ and 10,000,000₫.", messages);
            Assert.Contains("Max discount must be between 1₫ and 10,000,000₫.", messages);
        }

        //=========================================================
        // UTCID02 - Code length < 2
        // Expected: "Code must be 2–20 characters."
        //=========================================================
        [Fact]
        public void UTCID02_CreateVoucherRequestDTO_CodeUnder2Chars_ReturnLengthError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(code: "A");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Code must be 2–20 characters.", messages);
        }

        //=========================================================
        // UTCID03 - Code length > 20
        // Expected: "Code must be 2–20 characters."
        //=========================================================
        [Fact]
        public void UTCID03_CreateVoucherRequestDTO_CodeOver20Chars_ReturnLengthError()
        {
            // Arrange
            var longCode = new string('A', 21);
            var request = BuildCreateVoucherRequest(code: longCode);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Code must be 2–20 characters.", messages);
        }

        //=========================================================
        // UTCID05 - Code invalid format (lowercase/special)
        //=========================================================
        [Fact]
        public void UTCID05_CreateVoucherRequestDTO_CodeInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(code: "sale-10");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Code must be uppercase letters and numbers only.", messages);
        }

        //=========================================================
        // UTCID06 - DiscountPercent < 1
        // Expected: "Discount must be between 1% and 80%."
        //=========================================================
        [Fact]
        public void UTCID06_CreateVoucherRequestDTO_DiscountPercentUnder1_ReturnRangeError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(discountPercent: 0);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Discount must be between 1% and 80%.", messages);
        }

        //=========================================================
        // UTCID14 - Description length > 100
        // Expected: "Description must not exceed 100 characters."
        //=========================================================
        [Fact]
        public void UTCID14_CreateVoucherRequestDTO_DescriptionOver100Chars_ReturnLengthError()
        {
            // Arrange
            var longDescription = new string('A', 101);
            var request = BuildCreateVoucherRequest(description: longDescription);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Description must not exceed 100 characters.", messages);
        }

        //=========================================================
        // UTCID16 - MinOrderAmount < 0
        //=========================================================
        [Fact]
        public void UTCID16_CreateVoucherRequestDTO_MinOrderAmountNegative_ReturnRangeError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(minOrderAmount: -100);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Min order amount must be between 1₫ and 10,000,000₫.", messages);
        }

        //=========================================================
        // UTCID17 - MinOrderAmount = 0
        //=========================================================
        [Fact]
        public void UTCID17_CreateVoucherRequestDTO_MinOrderAmountZero_ReturnRangeError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(minOrderAmount: 0);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Min order amount must be between 1₫ and 10,000,000₫.", messages);
        }

        //=========================================================
        // UTCID18 - MaxDiscountAmount < 1
        // Expected: "Max discount must be between 1₫ and 10,000,000₫."
        //=========================================================
        [Fact]
        public void UTCID18_CreateVoucherRequestDTO_MaxDiscountAmountUnder1_ReturnRangeError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(maxDiscountAmount: 0);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Max discount must be between 1₫ and 10,000,000₫.", messages);
        }

        //=========================================================
        // UTCID21 - MaxDiscountAmount > 10,000,000
        // Expected: "Max discount must be between 1₫ and 10,000,000₫."
        //=========================================================
        [Fact]
        public void UTCID21_CreateVoucherRequestDTO_MaxDiscountAmountOver10Million_ReturnRangeError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(maxDiscountAmount: 10_000_001);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Max discount must be between 1₫ and 10,000,000₫.", messages);
        }

        //=========================================================
        //=========================================================
        // CreateAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - Code length = 20 (boundary hợp lệ), CodeExists = false,
        //           CreateAsync succeeds, ExpiredDate in future
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID04_CreateAsync_CodeExactly20Chars_ReturnSuccess()
        {
            // Arrange
            var code20 = new string('A', 20);
            Assert.Equal(20, code20.Length);
            var request = BuildCreateVoucherRequest(code: code20);

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(code20, result.Data!.Code);
        }

        //=========================================================
        // UTCID07 - DiscountPercent = 1 (boundary hợp lệ), CodeExists = false,
        //           CreateAsync succeeds, ExpiredDate in future
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID07_CreateAsync_DiscountPercentExactly1_ReturnSuccess()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(discountPercent: 1);

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID08 - DiscountPercent = 80 (boundary hợp lệ),
        //           EffectiveRate = 80% (Max/Min ratio, cũng là boundary hợp lệ),
        //           CodeExists = false, CreateAsync succeeds
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID08_CreateAsync_DiscountPercent80AndEffectiveRate80_ReturnSuccess()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(
                discountPercent: 80,
                minOrderAmount: 100000,
                maxDiscountAmount: 80000); // 80000/100000*100 = 80% đúng biên

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID09 - EffectiveRate (Max/Min ratio) > 80%
        //=========================================================
        [Fact]
        public async Task UTCID09_CreateAsync_EffectiveRateOver80Percent_ReturnEffectiveRateError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(
                discountPercent: 50, // hợp lệ, không liên quan tới lỗi này
                minOrderAmount: 100000,
                maxDiscountAmount: 90000); // 90000/100000*100 = 90% > 80%, Max < Min nên không vi phạm rule khác

            SetupCodeExists(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Effective discount rate (90.0%) exceeds 80%.", result.Message);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID10 - Trường hợp hợp lệ tổng quát (happy path)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID10_CreateAsync_ValidRequest_ReturnSuccess()
        {
            // Arrange
            var request = BuildCreateVoucherRequest();

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
            Assert.NotNull(result.Data);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Once);
        }

        //=========================================================
        // UTCID11 - CodeExists = true
        // Expected: (false, "Voucher code 'XXX' already exists.")
        //=========================================================
        [Fact]
        public async Task UTCID11_CreateAsync_CodeAlreadyExists_ReturnCodeExistsError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(code: "DUPLICATE10");

            SetupCodeExists(true);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Voucher code '{request.Code.ToUpper()}' already exists.", result.Message);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID12 - CodeExists = false, Repository CreateAsync throws
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID12_CreateAsync_CreateAsyncThrows_ThrowException()
        {
            // Arrange
            var request = BuildCreateVoucherRequest();

            SetupCodeExists(false);
            _voucherRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Voucher>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID13 - CodeExists = false, ExpiredDate in past
        // Expected: (false, "Expiry date must be in the future.")
        //=========================================================
        [Fact]
        public async Task UTCID13_CreateAsync_ExpiredDateInPast_ReturnExpiryError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(expiredDate: DateTime.UtcNow.AddDays(-1));

            SetupCodeExists(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Expiry date must be in the future.", result.Message);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID15 - Description length = 100 (boundary hợp lệ),
        //           CodeExists = false, CreateAsync succeeds
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID15_CreateAsync_DescriptionExactly100Chars_ReturnSuccess()
        {
            // Arrange
            var description100 = new string('A', 100);
            var request = BuildCreateVoucherRequest(description: description100);

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID19 - MaxDiscountAmount = 1 (boundary hợp lệ),
        //           CodeExists = false, CreateAsync succeeds
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID19_CreateAsync_MaxDiscountAmountExactly1_ReturnSuccess()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(
                minOrderAmount: 100000,
                maxDiscountAmount: 1);

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID20 - MaxDiscountAmount = 50,000,000 (boundary hợp lệ),
        //           CodeExists = false, CreateAsync succeeds
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID20_CreateAsync_MaxDiscountAmountWithinLimit_ReturnSuccess()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(
                minOrderAmount: 10_000_000,
                maxDiscountAmount: 5_000_000); // ratio 50%, Max < Min

            SetupCodeExists(false);
            SetupCreateAsyncSuccess();

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID22 - UsageLimit < 1
        // [SỬA message theo quyết định: dùng đúng message Service hiện có]
        // Expected: (false, "Usage limit must be between 1 and 500.")
        //=========================================================
        [Fact]
        public async Task UTCID22_CreateAsync_UsageLimitBelow1_ReturnUsageLimitError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(useageLimit: 0);

            SetupCodeExists(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Usage limit must be between 1 and 500.", result.Message);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Never);
        }

        //=========================================================
        // UTCID23 - MaxDiscountAmount >= MinOrderAmount
        // Expected: (false, "Max discount amount must be less than min order amount.")
        //=========================================================
        [Fact]
        public async Task UTCID23_CreateAsync_MaxDiscountGreaterOrEqualMinOrder_ReturnMaxDiscountError()
        {
            // Arrange
            var request = BuildCreateVoucherRequest(
                minOrderAmount: 100000,
                maxDiscountAmount: 100000); // bằng nhau, vi phạm Max < Min

            SetupCodeExists(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Max discount amount must be less than min order amount.", result.Message);
            _voucherRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Voucher>()), Times.Never);
        }
    

        //=====================================================================
        // Helper: Create CheckoutService in Memory (for Mock tests)
        //=====================================================================
        private (PetCenterContext context, CheckoutService service) CreateInMemoryCheckoutService()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase("DbVoucherMock_" + Guid.NewGuid().ToString())
                .Options;

            var context = new PetCenterContext(options);

            var hubMock = new Mock<IHubContext<AppHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubMock.Setup(x => x.Clients).Returns(clientsMock.Object);

            var vnPayMock = new Mock<IVnPayService>();
            var moMoMock = new Mock<IMoMoService>();

            var orderRepo = new PetCenterAPI.Repository.OrderRepository(context);
            var paymentRepo = new PetCenterAPI.Repository.PaymentRepository(context);
            var voucherRepo = new PetCenterAPI.Repository.VoucherRepository(context);
            var inventoryRepo = new PetCenterAPI.Repository.InventoryRepository(context);
            var productRepo = new PetCenterAPI.Repository.ProductRepository(context, new Mock<AutoMapper.IMapper>().Object);
            var addressRepo = new PetCenterAPI.Repository.AddressRepository(context);
            var cartRepo = new PetCenterAPI.Repository.CartRepository(context);
            var appointmentRepo = new PetCenterAPI.Repository.AppointmentRepository(context);

            var service = new CheckoutService(
                orderRepo, paymentRepo, voucherRepo, inventoryRepo, productRepo, addressRepo, cartRepo, appointmentRepo,
                hubMock.Object, vnPayMock.Object, moMoMock.Object, NullLogger<CheckoutService>.Instance);
            return (context, service);
        }

        private async Task<Customer> EnsureCustomerInMemoryAsync(PetCenterContext context, Guid customerId)
        {
            var cus = new Customer
            {
                CustomerId = customerId,
                FullName = "Test Customer",
                Email = $"voucher_{Guid.NewGuid():N}@gmail.com",
                PhoneNumber = "0988888888",
                Gender = "Male",
                IsVerified = true,
                IsActive = true
            };
            context.Customers.Add(cus);
            await context.SaveChangesAsync();
            return cus;
        }

        //=====================================================================
        // GetAvailableVouchersAsync() Mock Tests (UTCID01 to UTCID07)
        //=====================================================================

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID01_Success()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "SALE10",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(5),
                MinOrderAmount = 100000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Single(result);
            Assert.Equal("SALE10", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID02_Success_ExpiredDateNull_MinOrderAmountNull_UsageLimitNull()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "SALENULL",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = null,
                MinOrderAmount = null,
                UseageLimit = null,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 50000);

            Assert.Single(result);
            Assert.Equal("SALENULL", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID03_Success_ExpiredDateEqualNow()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "SALEBORDER",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddHours(1),
                MinOrderAmount = 100000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Single(result);
            Assert.Equal("SALEBORDER", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID04_Success_MinOrderAmountEqualOrderAmount()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "SALEEXACT",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(5),
                MinOrderAmount = 100000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 100000);

            Assert.Single(result);
            Assert.Equal("SALEEXACT", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID05_ExcludesVoucher_WhenInactive()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "INACTIVE",
                DiscountPercent = 10,
                IsActive = false,
                ExpiredDate = DateTime.Now.AddDays(5),
                MinOrderAmount = 100000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID06_ExcludesVoucher_WhenExpired()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "EXPIRED",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(-1),
                MinOrderAmount = 100000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID07_ExcludesVoucher_WhenMinOrderAmountGreaterThanOrderAmount()
        {
            var (context, service) = CreateInMemoryCheckoutService();

            var customer = await EnsureCustomerInMemoryAsync(context, Guid.NewGuid());
            var voucher = new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = "HIGHMIN",
                DiscountPercent = 10,
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(5),
                MinOrderAmount = 200000,
                CreateAt = DateTime.Now
            };
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

}
}
