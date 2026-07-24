using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Security;
using PetCenterAPI.Service;

namespace PetCenterTestProject.AuthTest
{
    public class StaffAuthTest_DB
    {
        //=========================================================
        // Mock external service only
        //=========================================================
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly PasswordService _passwordService;

        //=========================================================
        // Constructor
        //=========================================================
        public StaffAuthTest_DB()
        {
            _jwtServiceMock = new Mock<IJwtService>();
            _passwordService = new PasswordService();
        }

        //=========================================================
        // Create SQL Server Context
        //=========================================================
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=.;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;")
                .Options;

            return new PetCenterContext(options);
        }

        //=========================================================
        // Create Repository
        //=========================================================
        private StaffAuthRepository CreateRepository(PetCenterContext context)
        {
            return new StaffAuthRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================
        private StaffAuthService CreateService(PetCenterContext context)
        {
            return new StaffAuthService(
                CreateRepository(context),
                _passwordService,
                _jwtServiceMock.Object);
        }

        //=========================================================
        // Clear Database
        //=========================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM StaffRoles");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Staffs");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Roles");
        }

        //=========================================================
        // Helper: Build Staff entity with real BCrypt hash
        //=========================================================
        private Staff BuildStaff(
            string email,
            string rawPassword,
            bool isActive,
            List<Role> roles)
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
                Roles = roles
            };
        }

        //=========================================================
        // LoginAsync()
        // UTCID01 - Staff not found
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID01_LoginAsync_StaffNotFound_ReturnInvalidCredentials()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync("notfound@petcenter.com", "Admin@123");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("InvalidCredentials", result.ErrorType);
            Assert.Equal("Email or password incorrect", result.Message);
            Assert.Empty(result.Roles);
        }

        //=========================================================
        // UTCID02 - Staff exists, Account is inactive
        // Expected: Authentication failed - AccountInactive
        //=========================================================
        [Fact]
        public async Task UTCID02_LoginAsync_AccountInactive_ReturnAccountInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff(
                "inactive@petcenter.com",
                "Admin@123",
                isActive: false,
                roles: new List<Role>
            {
                new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true }
            });

            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(staff.Email, "Admin@123");

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff(
                "admin@petcenter.com",
                "Admin@123",
                isActive: true,
                roles: new List<Role>
            {
                new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true }
            });

            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(staff.Email, "WrongPassword");

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff(
                "admin@petcenter.com",
                "Admin@123",
                isActive: true,
                roles: new List<Role>
            {
        new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = false }
            });

            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(staff.Email, "Admin@123");

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff(
                "admin@petcenter.com",
                "Admin@123",
                isActive: true,
                roles: new List<Role>
                {
                new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true },
                new Role { RoleId = Guid.NewGuid(), RoleName = "Sale Staff", IsActive = false }
                });

            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    staff.StaffId,
                    staff.Email,
                    It.Is<List<string>>(r => r.Count == 1 && r.Contains("Admin")),
                    staff.FullName))
                .Returns("fake-jwt-token");

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(staff.Email, "Admin@123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.Null(result.ErrorType);
            Assert.Equal("Login success", result.Message);
            Assert.Single(result.Roles);
            Assert.Contains("Admin", result.Roles);
        }

        //=========================================================
        // UTCID07 - Staff valid, JWT service throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID07_LoginAsync_JwtServiceThrowsException_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff(
                "admin@petcenter.com",
                "Admin@123",
                isActive: true,
                roles: new List<Role>
            {
                new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true }
            });

            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>()))
                .Throws(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(context);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.LoginAsync(staff.Email, "Admin@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }
    }
}
