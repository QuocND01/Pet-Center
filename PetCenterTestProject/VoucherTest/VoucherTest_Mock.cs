using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
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
    }
}
