using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service;

namespace PetCenterTestProject.AuthTest
{
    public class StaffAuthTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<IStaffAuthRepository> _staffAuthRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly PasswordService _passwordService;
        private readonly StaffAuthService _service;

        //=========================================================
        // Constructor
        //=========================================================
        public StaffAuthTest_Mock()
        {
            _staffAuthRepositoryMock = new Mock<IStaffAuthRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _passwordService = new PasswordService();

            _service = new StaffAuthService(
                _staffAuthRepositoryMock.Object,
                _passwordService,
                _jwtServiceMock.Object);
        }

        //=========================================================
        // Helper: Build Staff entity with real BCrypt hash
        //=========================================================
        private Staff BuildStaff(
            string email = "admin@petcenter.com",
            string rawPassword = "Admin@123",
            bool isActive = true,
            List<Role>? roles = null)
        {
            return new Staff
            {
                StaffId = Guid.NewGuid(),
                FullName = "Admin User",
                PhoneNumber = "0900000000",
                BirthDate = new DateTime(1990, 1, 1),
                Gender = "Male",
                HireDate = DateTime.Now,
                Email = email,
                PasswordHash = _passwordService.Hash(rawPassword),
                IsActive = isActive,
                CreatedAt = DateTime.Now,
                Roles = roles ?? new List<Role>
            {
                new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true }
            }
            };
        }

        //=========================================================
        // LoginAsync()
        // UTCID01 - Staff not found
        // Expected: Authentication failed - InvalidCredentials - "Email or password incorrect"
        //=========================================================
        [Fact]
        public async Task UTCID01_LoginAsync_StaffNotFound_ReturnInvalidCredentials()
        {
            // Arrange
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Staff?)null);

            // Act
            var result = await _service.LoginAsync("notfound@petcenter.com", "Admin@123");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("InvalidCredentials", result.ErrorType);
            Assert.Equal("Email or password incorrect", result.Message);
            Assert.Empty(result.Roles);
        }

        //=========================================================
        // UTCID02 - Staff exists, Account is inactive
        // Expected: Authentication failed - AccountInactive - "...Please contact admin."
        //=========================================================
        [Fact]
        public async Task UTCID02_LoginAsync_AccountInactive_ReturnAccountInactive()
        {
            // Arrange
            var staff = BuildStaff(isActive: false);
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(staff.Email))
                .ReturnsAsync(staff);

            // Act
            var result = await _service.LoginAsync(staff.Email, "Admin@123");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("AccountInactive", result.ErrorType);
            Assert.Equal("Your account has been deactivated. Please contact admin.", result.Message);
            Assert.Empty(result.Roles);
        }

        //=========================================================
        // UTCID03 - Staff exists, Active, Password does not match
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID03_LoginAsync_PasswordNotMatch_ReturnInvalidCredentials()
        {
            // Arrange
            var staff = BuildStaff(rawPassword: "Admin@123", isActive: true);
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(staff.Email))
                .ReturnsAsync(staff);

            // Act
            var result = await _service.LoginAsync(staff.Email, "WrongPassword");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("InvalidCredentials", result.ErrorType);
            Assert.Equal("Email or password incorrect", result.Message);
            Assert.Empty(result.Roles);
        }

        //=========================================================
        // UTCID04 - Staff exists, Active, Password matches, No active role
        // Expected: Authentication failed - NoPermission
        //=========================================================
        [Fact]
        public async Task UTCID04_LoginAsync_NoActiveRole_ReturnNoPermission()
        {
            // Arrange
            var staff = BuildStaff(
                isActive: true,
                roles: new List<Role>
                {
                new Role { RoleName = "Admin", IsActive = false }
                });
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(staff.Email))
                .ReturnsAsync(staff);

            // Act
            var result = await _service.LoginAsync(staff.Email, "Admin@123");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("NoPermission", result.ErrorType);
            Assert.Equal("This account does not have permission to access the system.", result.Message);
            Assert.Empty(result.Roles);
        }

        //=========================================================
        // UTCID05 - Staff exists, Active, Password matches, Has active role
        // Expected: JWT generated - Login success
        //=========================================================
        [Fact]
        public async Task UTCID05_LoginAsync_ValidCredentials_ReturnLoginSuccess()
        {
            // Arrange
            var staff = BuildStaff(
                isActive: true,
                roles: new List<Role>
                {
                new Role { RoleName = "Admin", IsActive = true },
                new Role { RoleName = "Sale Staff", IsActive = false }
                });
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(staff.Email))
                .ReturnsAsync(staff);
            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    staff.StaffId,
                    staff.Email,
                    It.Is<List<string>>(r => r.Count == 1 && r.Contains("Admin")),
                    staff.FullName))
                .Returns("fake-jwt-token");

            // Act
            var result = await _service.LoginAsync(staff.Email, "Admin@123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.Null(result.ErrorType);
            Assert.Equal("Login success", result.Message);
            Assert.Single(result.Roles);
            Assert.Contains("Admin", result.Roles);
        }

        //=========================================================
        // UTCID06 - Repository throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID06_LoginAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.LoginAsync("admin@petcenter.com", "Admin@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID07 - Staff valid, JWT service throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID07_LoginAsync_JwtServiceThrowsException_ThrowException()
        {
            // Arrange
            var staff = BuildStaff(isActive: true);
            _staffAuthRepositoryMock
                .Setup(x => x.GetByEmailAsync(staff.Email))
                .ReturnsAsync(staff);
            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>()))
                .Throws(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.LoginAsync(staff.Email, "Admin@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }
    }
}
