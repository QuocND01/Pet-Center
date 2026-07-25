using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;

namespace PetCenterTestProject.CustomerTest
{
    public class CustomerProfileTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly IMapper _mapper;
        private readonly CustomerService _service;

        //=========================================================
        // Constructor
        //=========================================================
        public CustomerProfileTest_Mock()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CustomerMappingProfile>();
            }, NullLoggerFactory.Instance)
            .CreateMapper();

            _service = new CustomerService(
                _customerRepositoryMock.Object,
                _mapper);
        }

        //=========================================================
        // Helper: Build a Customer entity
        //=========================================================
        private Customer BuildCustomer(
            Guid? customerId = null,
            string email = "customer@petcenter.com",
            string fullName = "Nguyen Van A",
            string phoneNumber = "0912345678",
            bool? isVerified = true,
            bool? isActive = true)
        {
            return new Customer
            {
                CustomerId = customerId ?? Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                Gender = "Male",
                IsVerified = isVerified,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        //=========================================================
        // Helper: Build a valid UpdateCustomerProfileRequestDTO, override fields as needed
        //=========================================================
        private UpdateCustomerProfileRequestDTO BuildUpdateRequest(
            string fullName = "Nguyen Van B",
            string phoneNumber = "0912345678",
            DateOnly? birthDay = null,
            string gender = "Male")
        {
            return new UpdateCustomerProfileRequestDTO
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                BirthDay = birthDay ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                Gender = gender
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
        // GetProfileAsync()
        // UTCID01 - Customer exists
        // Expected: Entity mapped to CustomerProfileResponseDTO, DTO returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetProfileAsync_CustomerExists_ReturnProfileDto()
        {
            // Arrange
            var customer = BuildCustomer();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.GetProfileAsync(customer.CustomerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.CustomerId, result!.CustomerId);
            Assert.Equal(customer.FullName, result.FullName);
            Assert.Equal(customer.Email, result.Email);
            Assert.Equal(customer.PhoneNumber, result.PhoneNumber);
            Assert.Equal(customer.BirthDay, result.BirthDay);
            Assert.Equal(customer.Gender, result.Gender);
            Assert.Equal(customer.IsVerified, result.IsVerified);
            Assert.Equal(customer.IsActive, result.IsActive);
        }

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: Null returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetProfileAsync_CustomerNotFound_ReturnNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _service.GetProfileAsync(customerId);

            // Assert
            Assert.Null(result);
        }

        //=========================================================
        // UTCID03 - Repository throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID03_GetProfileAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetProfileAsync(customerId));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // UpdateCustomerProfileRequestDTO Validation (DataAnnotations)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - All fields empty / default
        // BirthDay là DateOnly (không nullable) nên [Required] không kích hoạt được;
        // giá trị default (0001-01-01) rơi vào nhánh "more than 100 years ago"
        // Expected: Required errors for FullName, PhoneNumber, Gender + age error cho BirthDay
        //=========================================================
        [Fact]
        public void UTCID01_UpdateCustomerProfileRequestDTO_AllFieldsEmpty_ReturnRequiredErrors()
        {
            // Arrange
            var request = new UpdateCustomerProfileRequestDTO
            {
                FullName = "",
                PhoneNumber = "",
                BirthDay = default,
                Gender = ""
            };

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Full name is required", messages);
            Assert.Contains("Phone number is required", messages);
            Assert.Contains("Gender is required", messages);
            Assert.Contains("Date of birth cannot be more than 100 years ago", messages);
        }

        //=========================================================
        // UTCID05 - FullName invalid format
        // Expected: "Full name must contain letters only and at least 2 characters"
        //=========================================================
        [Fact]
        public void UTCID05_UpdateCustomerProfileRequestDTO_FullNameInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildUpdateRequest(fullName: "Nguyen123");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains(
                "Full name must contain letters only and at least 2 characters",
                messages);
        }

        //=========================================================
        // UTCID06 - FullName length > 50
        // Expected: "Full name must not exceed 50 characters"
        //=========================================================
        [Fact]
        public void UTCID06_UpdateCustomerProfileRequestDTO_FullNameOver50Chars_ReturnLengthError()
        {
            // Arrange
            var longName = new string('A', 51);
            var request = BuildUpdateRequest(fullName: longName);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Full name must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID07 - FullName length = 50 (boundary hợp lệ)
        // StringLength(50) cho phép đúng 50 ký tự -> KHÔNG lỗi length
        // Expected: KHÔNG có lỗi "must not exceed 50 characters"
        //=========================================================
        [Fact]
        public void UTCID07_UpdateCustomerProfileRequestDTO_FullNameExactly50Chars_ReturnNoLengthError()
        {
            // Arrange
            var name50 = new string('A', 50);
            Assert.Equal(50, name50.Length);
            var request = BuildUpdateRequest(fullName: name50);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.DoesNotContain("Full name must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID08 - Phone invalid format
        // Expected: "Invalid Vietnamese phone number"
        //=========================================================
        [Fact]
        public void UTCID08_UpdateCustomerProfileRequestDTO_PhoneInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildUpdateRequest(phoneNumber: "0123456789");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Invalid Vietnamese phone number", messages);
        }

        //=========================================================
        // UTCID09 - Phone length > 15
        // Expected: "Phone number cannot exceed 15 characters"
        //=========================================================
        [Fact]
        public void UTCID09_UpdateCustomerProfileRequestDTO_PhoneOver15Chars_ReturnLengthError()
        {
            // Arrange
            var longPhone = "0912345678901234"; // 16 ký tự, chắc chắn vượt StringLength(15)
            Assert.True(longPhone.Length > 15); // xác nhận điều kiện biên trước khi test
            var request = BuildUpdateRequest(phoneNumber: longPhone);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Phone Number cannot exceed 15 characters", messages);
        }

        //=========================================================
        // UTCID10 - Phone length = 15 (boundary hợp lệ cho StringLength)
        // Số điện thoại VN hợp lệ luôn đúng 10 ký tự -> chuỗi 15 ký tự vẫn fail
        // regex, nhưng KHÔNG fail StringLength(15)
        // Expected: "Invalid Vietnamese phone number", KHÔNG có lỗi "exceed 15"
        //=========================================================
        [Fact]
        public void UTCID10_UpdateCustomerProfileRequestDTO_PhoneExactly15Chars_ReturnFormatErrorOnly()
        {
            // Arrange
            var phone15 = "091234567012345"[..15];
            Assert.Equal(15, phone15.Length);
            var request = BuildUpdateRequest(phoneNumber: phone15);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Invalid Vietnamese phone number", messages);
            Assert.DoesNotContain("Phone Number cannot exceed 15 characters", messages);
        }

        //=========================================================
        // UTCID14 - Age < 16
        // Expected: "You must be at least 16 years old"
        //=========================================================
        [Fact]
        public void UTCID14_UpdateCustomerProfileRequestDTO_AgeUnder16_ReturnAgeError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
            var request = BuildUpdateRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("You must be at least 16 years old", messages);
        }

        //=========================================================
        // UTCID15 - Age > 100
        // Expected: "Date of birth cannot be more than 100 years ago"
        //=========================================================
        [Fact]
        public void UTCID15_UpdateCustomerProfileRequestDTO_AgeOver100_ReturnAgeError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-101));
            var request = BuildUpdateRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Date of birth cannot be more than 100 years ago", messages);
        }

        //=========================================================
        // UTCID16 - Birthday is a future date
        // Expected: "Date of birth cannot be in the future"
        //=========================================================
        [Fact]
        public void UTCID16_UpdateCustomerProfileRequestDTO_FutureBirthday_ReturnFutureDateError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var request = BuildUpdateRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Date of birth cannot be in the future", messages);
        }

        //=========================================================
        //=========================================================
        // UpdateProfileAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: (false, "Customer not found")
        //=========================================================
        [Fact]
        public async Task UTCID02_UpdateProfileAsync_CustomerNotFound_ReturnCustomerNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildUpdateRequest();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Customer not found", result.Message);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        // UTCID03 - Account deactivated
        // Return tuple bình thường (KHÔNG throw)
        // Expected: (false, "Account is deactivated")
        //=========================================================
        [Fact]
        public async Task UTCID03_UpdateProfileAsync_AccountDeactivated_ReturnAccountDeactivated()
        {
            // Arrange
            var customer = BuildCustomer(isActive: false);
            var request = BuildUpdateRequest(phoneNumber: customer.PhoneNumber!);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Account is deactivated", result.Message);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        // UTCID04 - UpdateAsync throws
        // Expected: Exception (repository ném lỗi thật, Service không catch)
        //=========================================================
        [Fact]
        public async Task UTCID04_UpdateProfileAsync_UpdateAsyncThrows_ThrowException()
        {
            // Arrange
            var customer = BuildCustomer();
            var request = BuildUpdateRequest(phoneNumber: customer.PhoneNumber!);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateProfileAsync(customer.CustomerId, request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID11 - Phone unchanged, update thành công
        // Expected: (true, "Profile updated successfully")
        //=========================================================
        [Fact]
        public async Task UTCID11_UpdateProfileAsync_PhoneUnchanged_ReturnSuccess()
        {
            // Arrange
            var customer = BuildCustomer(phoneNumber: "0912345678");
            var request = BuildUpdateRequest(
                fullName: "Nguyen Van Updated",
                phoneNumber: customer.PhoneNumber!);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);
            Assert.Equal("Nguyen Van Updated", customer.FullName);
            _customerRepositoryMock.Verify(
                x => x.GetByPhoneAsync(It.IsAny<string>()), Times.Never);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(customer), Times.Once);
        }

        //=========================================================
        // UTCID12 - Phone changed, GetByPhoneAsync trả về null, update thành công
        // Expected: (true, "Profile updated successfully")
        //=========================================================
        [Fact]
        public async Task UTCID12_UpdateProfileAsync_PhoneChangedNoDuplicate_ReturnSuccess()
        {
            // Arrange
            var customer = BuildCustomer(phoneNumber: "0912345678");
            var request = BuildUpdateRequest(phoneNumber: "0987654321");

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);
            Assert.Equal("0987654321", customer.PhoneNumber);
            _customerRepositoryMock.Verify(x => x.GetByPhoneAsync(request.PhoneNumber), Times.Once);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(customer), Times.Once);
        }

        //=========================================================
        // UTCID13 - Phone already exists (duplicate)
        // Expected: (false, "Phone number is already in use by another account.")
        //=========================================================
        [Fact]
        public async Task UTCID13_UpdateProfileAsync_PhoneAlreadyExists_ReturnPhoneInUse()
        {
            // Arrange
            var customer = BuildCustomer(phoneNumber: "0912345678");
            var request = BuildUpdateRequest(fullName: "Should Not Apply", phoneNumber: "0987654321");
            var duplicateOwner = BuildCustomer(phoneNumber: request.PhoneNumber);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync(duplicateOwner);

            // Act
            var result = await _service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use by another account.", result.Message);
            Assert.Equal("0912345678", customer.PhoneNumber);
            Assert.Equal("Nguyen Van A", customer.FullName); 
            _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        //=========================================================
        // GetAllCustomersAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Customer list exists
        // Expected: Entity list mapped to CustomerResponseDTO list, list returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllCustomersAsync_CustomerListExists_ReturnCustomerList()
        {
            // Arrange
            var customers = new List<Customer>
    {
        BuildCustomer(fullName: "Nguyen Van A", email: "a@petcenter.com"),
        BuildCustomer(fullName: "Nguyen Van B", email: "b@petcenter.com")
    };

            _customerRepositoryMock
                .Setup(x => x.GetAllCustomersAsync())
                .ReturnsAsync(customers);

            // Act
            var result = await _service.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(customers[0].CustomerId, result[0].CustomerId);
            Assert.Equal(customers[0].FullName, result[0].FullName);
            Assert.Equal(customers[1].CustomerId, result[1].CustomerId);
            Assert.Equal(customers[1].FullName, result[1].FullName);
        }

        //=========================================================
        // UTCID02 - Customer list is empty
        // Expected: Empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllCustomersAsync_CustomerListEmpty_ReturnEmptyList()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetAllCustomersAsync())
                .ReturnsAsync(new List<Customer>());

            // Act
            var result = await _service.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //=========================================================
        // UTCID03 - Repository throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAllCustomersAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetAllCustomersAsync())
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAllCustomersAsync());

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // GetCustomerByIdAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Customer exists
        // Expected: Entity mapped to DTO, Customer DTO returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetCustomerByIdAsync_CustomerExists_ReturnCustomerDto()
        {
            // Arrange
            var customer = BuildCustomer();

            _customerRepositoryMock
                .Setup(x => x.GetCustomerByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.GetCustomerByIdAsync(customer.CustomerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.CustomerId, result!.CustomerId);
            Assert.Equal(customer.FullName, result.FullName);
            Assert.Equal(customer.Email, result.Email);
            Assert.Equal(customer.PhoneNumber, result.PhoneNumber);
        }

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: Null returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetCustomerByIdAsync_CustomerNotFound_ReturnNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetCustomerByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _service.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.Null(result);
        }

        //=========================================================
        // UTCID03 - Repository throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID03_GetCustomerByIdAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetCustomerByIdAsync(customerId))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetCustomerByIdAsync(customerId));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // ChangeCustomerStatusAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Customer exists, UpdateAsync returns true
        // Expected: Return true, status + UpdatedAt assigned
        //=========================================================
        [Fact]
        public async Task UTCID01_ChangeCustomerStatusAsync_CustomerExistsUpdateSucceeds_ReturnTrue()
        {
            // Arrange
            var customer = BuildCustomer(isActive: false);
            var oldUpdatedAt = customer.UpdatedAt;

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(customer))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ChangeCustomerStatusAsync(customer.CustomerId, true);

            // Assert
            Assert.True(result);
            Assert.True(customer.IsActive);
            Assert.NotEqual(oldUpdatedAt, customer.UpdatedAt);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(customer), Times.Once);
        }

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: Return false
        //=========================================================
        [Fact]
        public async Task UTCID02_ChangeCustomerStatusAsync_CustomerNotFound_ReturnFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _service.ChangeCustomerStatusAsync(customerId, true);

            // Assert
            Assert.False(result);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        // UTCID03 - Customer exists, UpdateAsync returns false
        // Expected: Return false, status + UpdatedAt vẫn đã bị gán trước khi update
        //=========================================================
        [Fact]
        public async Task UTCID03_ChangeCustomerStatusAsync_UpdateFails_ReturnFalse()
        {
            // Arrange
            var customer = BuildCustomer(isActive: true);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customer.CustomerId))
                .ReturnsAsync(customer);
            _customerRepositoryMock
                .Setup(x => x.UpdateAsync(customer))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ChangeCustomerStatusAsync(customer.CustomerId, false);

            // Assert
            Assert.False(result);
            Assert.False(customer.IsActive);
            _customerRepositoryMock.Verify(x => x.UpdateAsync(customer), Times.Once);
        }

        //=========================================================
        // UTCID04 - Customer exists, GetByIdAsync throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID04_ChangeCustomerStatusAsync_GetByIdAsyncThrows_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeCustomerStatusAsync(customerId, true));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }
    }
}
