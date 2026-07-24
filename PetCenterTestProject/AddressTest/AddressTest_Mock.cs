using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.AddressRequestDTO;

namespace PetCenterTestProject.AddressTest
{
    public class AddressTest_Mock
    {
        //=========================================================
        // Mock & Setup
        //=========================================================
        private readonly Mock<IAddressRepository> _addressRepositoryMock;
        private readonly AddressService _service;

        public AddressTest_Mock()
        {
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _service = new AddressService(_addressRepositoryMock.Object);
        }

        //=========================================================
        // DTO Validation Helper
        //=========================================================
        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        // Helper mồi dữ liệu chuẩn để test Validation không bị nhiễu
        private MutateAddressDTO CreateValidDTO()
        {
            return new MutateAddressDTO
            {
                Province = "Can Tho",
                District = "Ninh Kieu",
                Ward = "Xuan Khanh",
                AddressDetails = "FPT University",
                IsDefault = false
            };
        }

        //=====================================================================
        // Function: AddAddressAsync() (9 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_AddAddressAsync_ProvinceEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.Province = ""; // Cố tình làm sai
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID02_AddAddressAsync_ProvinceTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.Province = new string('A', 101);
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID03_AddAddressAsync_DistrictEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.District = "";
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID04_AddAddressAsync_DistrictTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.District = new string('A', 101);
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID05_AddAddressAsync_AddressDetailsEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.AddressDetails = "";
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public void UTCID06_AddAddressAsync_AddressDetailsTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO();
            dto.AddressDetails = new string('A', 301);
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public async Task UTCID07_AddAddressAsync_NoActiveAddress_ShouldAutoForceDefault()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var dto = CreateValidDTO();
            dto.IsDefault = false; // Người dùng không set mặc định

            // Mock DB trả về danh sách rỗng (khách hàng chưa có địa chỉ nào)
            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId))
                .ReturnsAsync(new List<Address>());

            // Act
            var result = await _service.AddAddressAsync(customerId, dto);

            // Assert
            Assert.True(result);
            // Phải đảm bảo IsDefault được hệ thống ép thành true
            _addressRepositoryMock.Verify(x => x.AddAddressAsync(It.Is<Address>(a => a.IsDefault == true)), Times.Once);
        }

        [Fact]
        public async Task UTCID08_AddAddressAsync_HasActiveAddress_ShouldSaveNormally()
        {
            var customerId = Guid.NewGuid();
            var dto = CreateValidDTO();
            dto.IsDefault = false;

            // Đã có địa chỉ rồi
            var existingAddresses = new List<Address> { new Address { AddressId = Guid.NewGuid(), IsActive = true } };
            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId)).ReturnsAsync(existingAddresses);

            var result = await _service.AddAddressAsync(customerId, dto);

            Assert.True(result);
            _addressRepositoryMock.Verify(x => x.AddAddressAsync(It.Is<Address>(a => a.IsDefault == false)), Times.Once);
            _addressRepositoryMock.Verify(x => x.ResetDefaultAddressAsync(It.IsAny<Guid>()), Times.Never); // Không gọi hàm reset
        }

        [Fact]
        public async Task UTCID09_AddAddressAsync_HasActive_SetToDefault_ShouldResetOthers()
        {
            var customerId = Guid.NewGuid();
            var dto = CreateValidDTO();
            dto.IsDefault = true; // Yêu cầu set làm mặc định mới

            var existingAddresses = new List<Address> { new Address { AddressId = Guid.NewGuid(), IsActive = true } };
            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId)).ReturnsAsync(existingAddresses);

            var result = await _service.AddAddressAsync(customerId, dto);

            Assert.True(result);
            _addressRepositoryMock.Verify(x => x.AddAddressAsync(It.Is<Address>(a => a.IsDefault == true)), Times.Once);
            _addressRepositoryMock.Verify(x => x.ResetDefaultAddressAsync(customerId), Times.Once); // PHẢI gọi hàm reset
        }

        //=====================================================================
        // Function: UpdateAddressAsync() (10 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_UpdateAddressAsync_ProvinceEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID02_UpdateAddressAsync_ProvinceTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID03_UpdateAddressAsync_DistrictEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID04_UpdateAddressAsync_DistrictTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID05_UpdateAddressAsync_AddressDetailsEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public void UTCID06_UpdateAddressAsync_AddressDetailsTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = new string('A', 301);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public async Task UTCID07_UpdateAddressAsync_AddressNotFound_ShouldReturnFalse()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync((Address?)null);

            var result = await _service.UpdateAddressAsync(customerId, addressId, CreateValidDTO());

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID08_UpdateAddressAsync_NormalUpdate_ShouldSuccess()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingAddress = new Address { AddressId = addressId, IsDefault = false };

            var dto = CreateValidDTO();
            dto.IsDefault = false;

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync(existingAddress);

            var result = await _service.UpdateAddressAsync(customerId, addressId, dto);

            Assert.True(result);
            _addressRepositoryMock.Verify(x => x.ResetDefaultAddressAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UTCID09_UpdateAddressAsync_SetToDefault_ShouldResetOthers()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            // Địa chỉ cũ đang không phải mặc định
            var existingAddress = new Address { AddressId = addressId, IsDefault = false };

            var dto = CreateValidDTO();
            dto.IsDefault = true; // Nay update muốn biến thành mặc định

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync(existingAddress);

            var result = await _service.UpdateAddressAsync(customerId, addressId, dto);

            Assert.True(result);
            Assert.True(existingAddress.IsDefault);
            _addressRepositoryMock.Verify(x => x.ResetDefaultAddressAsync(customerId), Times.Once); // Phải gọi reset
        }

        [Fact]
        public async Task UTCID10_UpdateAddressAsync_RepositoryThrowsException()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAddressAsync(customerId, addressId, CreateValidDTO()));
        }

        //=====================================================================
        // Function: DeleteAddressAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_DeleteAddressAsync_EmptyGuid_ShouldReturnFalse()
        {
            var customerId = Guid.NewGuid();
            // Giả lập Repo không tìm thấy ID rỗng
            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(Guid.Empty, customerId)).ReturnsAsync((Address?)null);

            var result = await _service.DeleteAddressAsync(customerId, Guid.Empty);

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID02_DeleteAddressAsync_NormalAddress_ShouldSoftDelete()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingAddress = new Address { AddressId = addressId, IsDefault = false }; // Không phải mặc định

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync(existingAddress);

            var result = await _service.DeleteAddressAsync(customerId, addressId);

            Assert.True(result);
            _addressRepositoryMock.Verify(x => x.DeleteAddressAsync(existingAddress), Times.Once);
        }

        [Fact]
        public async Task UTCID03_DeleteAddressAsync_DefaultAddress_ShouldBlockDeletion()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingAddress = new Address { AddressId = addressId, IsDefault = true }; // Là địa chỉ mặc định

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync(existingAddress);

            var result = await _service.DeleteAddressAsync(customerId, addressId);

            Assert.False(result); // Phải bị block
            _addressRepositoryMock.Verify(x => x.DeleteAddressAsync(It.IsAny<Address>()), Times.Never);
        }

        [Fact]
        public async Task UTCID04_DeleteAddressAsync_AddressNotFound_ShouldReturnFalse()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId)).ReturnsAsync((Address?)null);

            var result = await _service.DeleteAddressAsync(customerId, addressId);

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID05_DeleteAddressAsync_RepositoryThrowsException()
        {
            var addressId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            _addressRepositoryMock.Setup(x => x.GetAddressByIdAsync(addressId, customerId))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.DeleteAddressAsync(customerId, addressId));
        }

        //=====================================================================
        // Function: GetCustomerAddressesAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetCustomerAddressesAsync_HasAddresses_ShouldReturnList()
        {
            var customerId = Guid.NewGuid();
            var addresses = new List<Address>
            {
                new Address { AddressId = Guid.NewGuid(), Province = "Can Tho", IsDefault = true },
                new Address { AddressId = Guid.NewGuid(), Province = "Hau Giang", IsDefault = false }
            };

            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId)).ReturnsAsync(addresses);

            var result = await _service.GetCustomerAddressesAsync(customerId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result.First().IsDefault);
        }

        [Fact]
        public async Task UTCID02_GetCustomerAddressesAsync_NoAddresses_ShouldReturnEmptyList()
        {
            var customerId = Guid.NewGuid();
            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId))
                .ReturnsAsync(new List<Address>());

            var result = await _service.GetCustomerAddressesAsync(customerId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetCustomerAddressesAsync_RepositoryThrowsException()
        {
            var customerId = Guid.NewGuid();
            _addressRepositoryMock.Setup(x => x.GetAddressesByCustomerIdAsync(customerId))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetCustomerAddressesAsync(customerId));
        }
    }
}