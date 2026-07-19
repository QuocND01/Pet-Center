using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Moq;
using PetCenterAPI.DTOs.Requests.ManageStaff;
using PetCenterAPI.DTOs.Responses.ManageStaff;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using Xunit;

namespace PetCenterTestProject
{
    public class StaffTest
    {
        private readonly Mock<IStaffRepository> _staffRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly PasswordService _passwordService;
        private readonly StaffService _service;

        public StaffTest()
        {
            _staffRepositoryMock = new Mock<IStaffRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
            _passwordService = new PasswordService();

            _service = new StaffService(
                _staffRepositoryMock.Object,
                _cloudinaryServiceMock.Object,
                _passwordService);
        }

        // Helper: Create a fake IFormFile for avatar testing
        private IFormFile CreateFakeAvatarFile(string filename = "avatar.jpg", string contentType = "image/jpeg", long length = 100)
        {
            var stream = new MemoryStream(new byte[length]);
            return new FormFile(stream, 0, length, "Avatar", filename)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        // =========================================================================
        // SECTION 1: GetAllAsync() Unit Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetAllAsync_ReturnsMappedStaffListSuccessfully()
        {
            // Arrange
            var role = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var staffList = new List<Staff>
            {
                new Staff
                {
                    StaffId = Guid.NewGuid(),
                    FullName = "Nguyen Van A",
                    Email = "nguyenvana@pcss.com",
                    PhoneNumber = "0359998888",
                    Gender = "Male",
                    BirthDate = new DateTime(1995, 1, 1),
                    HireDate = new DateTime(2025, 1, 1),
                    IsActive = true,
                    Roles = new List<PetCenterAPI.Models.Role> { role }
                }
            };

            _staffRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(staffList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var item = result.First();
            Assert.Equal(staffList[0].StaffId, item.StaffId);
            Assert.Equal("Nguyen Van A", item.FullName);
            Assert.Equal("nguyenvana@pcss.com", item.Email);
            Assert.Equal("0359998888", item.PhoneNumber);
            Assert.Equal("Male", item.Gender);
            Assert.Equal("Staff", item.RoleName);
            Assert.Equal(role.RoleId, item.RoleId);
        }

        [Fact]
        public async Task UTCID02_GetAllAsync_ReturnsEmptyList()
        {
            // Arrange
            _staffRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Staff>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetAllAsync_ThrowsException()
        {
            // Arrange
            _staffRepositoryMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync());
        }

        // =========================================================================
        // SECTION 2: GetByIdAsync() Unit Tests (UTCID01 - UTCID06)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetStaffById_ReturnsStaffDetailsDTOWithoutVetProfile()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { role },
                VetProfile = null
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.GetByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(staffId, result.StaffId);
            Assert.Null(result.VetProfile);
            Assert.Equal("Staff", result.RoleName);
        }

        [Fact]
        public async Task UTCID02_GetStaffById_ReturnsStaffDetailsDTOWithVetProfileAndAverageRating()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Veterinarian", IsActive = true };
            var vetProfile = new VetProfile
            {
                VetProfileId = Guid.NewGuid(),
                StaffId = staffId,
                ExperienceYears = 5,
                Description = "Expert vet",
                LicenseNumber = "VET-12345",
                IsActive = true
            };
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { role },
                VetProfile = vetProfile
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetVetAverageRatingAsync(staffId)).ReturnsAsync(4.5m);

            // Act
            var result = await _service.GetByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.VetProfile);
            Assert.Equal(vetProfile.VetProfileId, result.VetProfile.VetProfileId);
            Assert.Equal(5, result.VetProfile.ExperienceYears);
            Assert.Equal("Expert vet", result.VetProfile.Description);
            Assert.Equal("VET-12345", result.VetProfile.LicenseNumber);
            Assert.Equal(4.5m, result.VetProfile.Rating);
        }

        [Fact]
        public async Task UTCID03_GetStaffById_ReturnsStaffDetailsDTOWithoutVetProfile_WhenVetProfileInactive()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Veterinarian", IsActive = true };
            var vetProfile = new VetProfile
            {
                VetProfileId = Guid.NewGuid(),
                StaffId = staffId,
                ExperienceYears = 5,
                Description = "Expert vet",
                LicenseNumber = "VET-12345",
                IsActive = false // Inactive
            };
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { role },
                VetProfile = vetProfile
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.GetByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.VetProfile); // Should be null because VetProfile is inactive
        }

        [Fact]
        public async Task UTCID04_GetStaffById_ReturnsStaffDetailsDTOWithNonAdminRoleName_WhenMultipleRolesIncludingAdmin()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var adminRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true };
            var staffRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { adminRole, staffRole }, // Has both
                VetProfile = null
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.GetByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Staff", result.RoleName); // PickRole prefers non-Admin
            Assert.Equal(staffRole.RoleId, result.RoleId);
        }

        [Fact]
        public async Task UTCID05_GetStaffById_ReturnsNull_WhenStaffIdDoesNotExist()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync((Staff?)null);

            // Act
            var result = await _service.GetByIdAsync(staffId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID06_GetStaffById_ThrowsException()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(staffId));
        }

        // =========================================================================
        // SECTION 3: CreateAsync() Unit Tests (UTCID01 - UTCID15)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateStaff_SuccessNonVet()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "  nguyen van a  ",
                Email = "NGUYENVANA@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-5),
                RoleId = roleId,
                Password = "password123",
                Avatar = null
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff created successfully.", result.Message);
            Assert.NotNull(result.StaffId);

            _staffRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Staff>(s =>
                s.FullName == "Nguyen Van A" && // Normalized FullName
                s.Email == "nguyenvana@pcss.com" && // Trimmed and lowercased
                s.PhoneNumber == "0359998888" &&
                s.Gender == "Male" &&
                s.IsActive == true &&
                s.VetProfile == null
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID02_CreateStaff_SuccessVet()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var avatarFile = CreateFakeAvatarFile();
            var request = new CreateStaffRequestDTO
            {
                FullName = "  nguyen van a  ",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-10),
                RoleId = roleId,
                Password = "password123",
                Avatar = avatarFile,
                LicenseNumber = "  VET-12345  ",
                Description = "  Specialist  ",
                ExperienceYears = 5
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.LicenseNumberExistsAsync("VET-12345", null)).ReturnsAsync(false);

            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(avatarFile, "petcenter/staff"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    PublicId = "staff_avatar_1",
                    SecureUrl = new Uri("https://res.cloudinary.com/staff_avatar_1.jpg")
                });

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff created successfully.", result.Message);
            Assert.NotNull(result.StaffId);

            _staffRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Staff>(s =>
                s.FullName == "Nguyen Van A" &&
                s.Avatar == "https://res.cloudinary.com/staff_avatar_1.jpg" &&
                s.PublicId == "staff_avatar_1" &&
                s.VetProfile != null &&
                s.VetProfile.LicenseNumber == "VET-12345" &&
                s.VetProfile.Description == "Specialist" &&
                s.VetProfile.ExperienceYears == 5 &&
                s.VetProfile.IsActive == true
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID03_CreateStaff_Fail_RoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var request = new CreateStaffRequestDTO { FullName = "Nguyen Van A", Password = "password", RoleId = roleId, Email = "test@pcss.com", PhoneNumber = "0359998888" };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync((PetCenterAPI.Models.Role?)null);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.StaffId);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID04_CreateStaff_Fail_RoleExistsButIsInactive()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = false };
            var request = new CreateStaffRequestDTO { FullName = "Nguyen Van A", Password = "password", RoleId = roleId, Email = "test@pcss.com", PhoneNumber = "0359998888" };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.StaffId);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID05_CreateStaff_Fail_EmailAlreadyInUse()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO { FullName = "Nguyen Van A", Password = "password", RoleId = roleId, Email = "duplicate@pcss.com", PhoneNumber = "0359998888" };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("duplicate@pcss.com", null)).ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.StaffId);
            Assert.Equal("Email 'duplicate@pcss.com' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID06_CreateStaff_Fail_PhoneNumberAlreadyInUse()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO { FullName = "Nguyen Van A", Password = "password", RoleId = roleId, Email = "new@pcss.com", PhoneNumber = "0359998888" };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.StaffId);
            Assert.Equal("Phone number is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID07_CreateStaff_Fail_AgeUnder18()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-17) // 17 years old
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.StaffId);
            Assert.Equal("Staff must be at least 18 years old.", result.Message);
        }

        [Fact]
        public async Task UTCID08_CreateStaff_Success_AgeExactly18()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-18), // Exactly 18
                HireDate = DateTime.UtcNow.AddDays(-2),
                Password = "password123"
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff created successfully.", result.Message);
            Assert.NotNull(result.StaffId);
        }

        [Fact]
        public async Task UTCID09_CreateStaff_Fail_HireDateInFuture()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(1) // Future
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Hire date cannot be in the future.", result.Message);
        }

        [Fact]
        public async Task UTCID10_CreateStaff_Success_HireDateExactlyToday()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow, // Today
                Password = "password123"
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff created successfully.", result.Message);
        }

        [Fact]
        public async Task UTCID11_CreateStaff_Fail_VetLicenseNumberNullOrWhitespace()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                LicenseNumber = "   " // Whitespace
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("License number is required for a veterinarian.", result.Message);
        }

        [Fact]
        public async Task UTCID12_CreateStaff_Fail_VetExperienceYearsNull()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                LicenseNumber = "VET-12345",
                ExperienceYears = null // Null
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Experience years is required for a veterinarian.", result.Message);
        }

        [Fact]
        public async Task UTCID13_CreateStaff_Fail_VetLicenseNumberAlreadyExists()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                LicenseNumber = "VET-12345",
                ExperienceYears = 3
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.LicenseNumberExistsAsync("VET-12345", null)).ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("License number 'VET-12345' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID14_CreateStaff_Fail_AvatarUploadFails()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var avatarFile = CreateFakeAvatarFile();
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                Avatar = avatarFile
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);

            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(avatarFile, "petcenter/staff"))
                .ReturnsAsync(new ImageUploadResult
                {
                    Error = new Error { Message = "Cloudinary limit exceeded" }
                });

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to upload avatar image.", result.Message);
        }

        [Fact]
        public async Task UTCID15_CreateStaff_ThrowsException()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Password = "password123",
                RoleId = roleId,
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2)
            };

            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", null)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Staff>())).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(request));
        }

        // =========================================================================
        // SECTION 4: UpdateAsync() Unit Tests (UTCID01 - UTCID18)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateStaff_SuccessNonVet()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var oldRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var newRole = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Old Name",
                Email = "old@pcss.com",
                PhoneNumber = "0351112222",
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { oldRole }
            };

            var request = new UpdateStaffRequestDTO
            {
                FullName = "  new name  ",
                Email = "NEW@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Female",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-30),
                RoleId = roleId,
                Avatar = null,
                ResetPassword = false
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(newRole);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff updated successfully.", result.Message);
            Assert.Equal("New Name", staff.FullName);
            Assert.Equal("new@pcss.com", staff.Email);
            Assert.Equal("0359998888", staff.PhoneNumber);
            Assert.Equal("Female", staff.Gender);
            Assert.Single(staff.Roles);
            Assert.Equal("Staff", staff.Roles.First().RoleName);

            _staffRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_UpdateStaff_SuccessWithResetPassword()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-30),
                RoleId = roleId,
                ResetPassword = true // Reset password requested
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.True(_passwordService.Verify("123456", staff.PasswordHash));
        }

        [Fact]
        public async Task UTCID03_UpdateStaff_SuccessWithAvatarUpload()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role }, PublicId = "old_avatar_id" };
            var avatarFile = CreateFakeAvatarFile();
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-30),
                RoleId = roleId,
                Avatar = avatarFile
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(avatarFile, "petcenter/staff"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    PublicId = "new_avatar_id",
                    SecureUrl = new Uri("https://res.cloudinary.com/new_avatar_id.jpg")
                });

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("https://res.cloudinary.com/new_avatar_id.jpg", staff.Avatar);
            Assert.Equal("new_avatar_id", staff.PublicId);

            // Verify old image is deleted
            _cloudinaryServiceMock.Verify(c => c.DeleteImageAsync("old_avatar_id"), Times.Once);
        }

        [Fact]
        public async Task UTCID04_UpdateStaff_Success_RoleChangedFromVetToNonVet()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var vetRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Veterinarian", IsActive = true };
            var nonVetRole = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var vetProfile = new VetProfile { VetProfileId = Guid.NewGuid(), StaffId = staffId, IsActive = true };
            var staff = new Staff
            {
                StaffId = staffId,
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { vetRole },
                VetProfile = vetProfile
            };

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                RoleId = roleId
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(nonVetRole);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.False(staff.VetProfile.IsActive); // Should be deactivated
        }

        [Fact]
        public async Task UTCID05_UpdateStaff_Success_RoleChangedFromNonVetToVet_NoExistingVetProfile()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var oldRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var newRole = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var staff = new Staff
            {
                StaffId = staffId,
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { oldRole },
                VetProfile = null // No initial vet profile
            };

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                RoleId = roleId,
                Description = "Experienced vet profile description"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(newRole);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(staff.VetProfile);
            Assert.Equal("Experienced vet profile description", staff.VetProfile.Description);
            Assert.Equal(0, staff.VetProfile.ExperienceYears); // ExperienceYears starts at 0 on change
            Assert.True(staff.VetProfile.IsActive);
        }

        [Fact]
        public async Task UTCID06_UpdateStaff_Success_RoleChangedFromNonVetToVet_WithExistingVetProfile()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var oldRole = new PetCenterAPI.Models.Role { RoleId = Guid.NewGuid(), RoleName = "Staff", IsActive = true };
            var newRole = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var vetProfile = new VetProfile { VetProfileId = Guid.NewGuid(), StaffId = staffId, IsActive = false, Description = "Old Desc" };
            var staff = new Staff
            {
                StaffId = staffId,
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { oldRole },
                VetProfile = vetProfile // Has inactive profile
            };

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                RoleId = roleId,
                Description = "New Desc"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(newRole);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(staff.VetProfile);
            Assert.Equal("New Desc", staff.VetProfile.Description);
            Assert.True(staff.VetProfile.IsActive); // Reactivated
        }

        [Fact]
        public async Task UTCID07_UpdateStaff_Success_RoleRemainsVet()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Veterinarian", IsActive = true };
            var vetProfile = new VetProfile { VetProfileId = Guid.NewGuid(), StaffId = staffId, IsActive = true, Description = "Old Desc", ExperienceYears = 5, LicenseNumber = "VET-1" };
            var staff = new Staff
            {
                StaffId = staffId,
                IsActive = true,
                Roles = new List<PetCenterAPI.Models.Role> { role },
                VetProfile = vetProfile
            };

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "test@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                RoleId = roleId,
                Description = "New Desc"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New Desc", staff.VetProfile.Description);
            Assert.Equal(5, staff.VetProfile.ExperienceYears); // Locked: not updated
            Assert.Equal("VET-1", staff.VetProfile.LicenseNumber); // Locked: not updated
        }

        [Fact]
        public async Task UTCID08_UpdateStaff_Fail_StaffDoesNotExist()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var request = new UpdateStaffRequestDTO { FullName = "Name", Email = "test@pcss.com" };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync((Staff?)null);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Staff not found.", result.Message);
        }

        [Fact]
        public async Task UTCID09_UpdateStaff_Fail_StaffInactive()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var staff = new Staff { StaffId = staffId, IsActive = false }; // Inactive
            var request = new UpdateStaffRequestDTO { FullName = "Name", Email = "test@pcss.com" };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot update an inactive staff member.", result.Message);
        }

        [Fact]
        public async Task UTCID10_UpdateStaff_Fail_RoleDoesNotExistOrInactive()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var staff = new Staff { StaffId = staffId, IsActive = true };
            var request = new UpdateStaffRequestDTO { FullName = "Name", Email = "test@pcss.com", RoleId = roleId };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync((PetCenterAPI.Models.Role?)null);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID11_UpdateStaff_Fail_EmailInUseByAnotherStaff()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "duplicate@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("duplicate@pcss.com", staffId)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email 'duplicate@pcss.com' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID12_UpdateStaff_Fail_PhoneInUseByAnotherStaff()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID13_UpdateStaff_Fail_AgeUnder18()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-17), // 17
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Staff must be at least 18 years old.", result.Message);
        }

        [Fact]
        public async Task UTCID14_UpdateStaff_Success_AgeExactly18()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-18), // Exactly 18
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID15_UpdateStaff_Fail_HireDateInFuture()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(2), // Future
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Hire date cannot be in the future.", result.Message);
        }

        [Fact]
        public async Task UTCID16_UpdateStaff_Success_HireDateExactlyToday()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow, // Today
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID17_UpdateStaff_Fail_AvatarUploadFails()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var avatarFile = CreateFakeAvatarFile();
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888",
                Avatar = avatarFile
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);

            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(avatarFile, "petcenter/staff"))
                .ReturnsAsync(new ImageUploadResult
                {
                    Error = new Error { Message = "Upload error" }
                });

            // Act
            var result = await _service.UpdateAsync(staffId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to upload avatar image.", result.Message);
        }

        [Fact]
        public async Task UTCID18_UpdateStaff_ThrowsException()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new PetCenterAPI.Models.Role { RoleId = roleId, RoleName = "Staff", IsActive = true };
            var staff = new Staff { StaffId = staffId, IsActive = true, Roles = new List<PetCenterAPI.Models.Role> { role } };
            var request = new UpdateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Email = "new@pcss.com",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                PhoneNumber = "0359998888"
            };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
            _staffRepositoryMock.Setup(repo => repo.EmailExistsAsync("new@pcss.com", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.PhoneExistsAsync("0359998888", staffId)).ReturnsAsync(false);
            _staffRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception("Database save error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(staffId, request));
        }

        // =========================================================================
        // SECTION 5: DeleteAsync() Unit Tests (UTCID01 - UTCID04)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteStaff_Success_SoftDeleteActiveStaff()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var staff = new Staff { StaffId = staffId, IsActive = true };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.DeleteAsync(staffId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Staff deactivated successfully.", result.Message);
            Assert.False(staff.IsActive); // Must be set to false

            _staffRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UTCID02_DeleteStaff_Fail_StaffDoesNotExist()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync((Staff?)null);

            // Act
            var result = await _service.DeleteAsync(staffId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Staff not found.", result.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteStaff_Fail_StaffAlreadyInactive()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var staff = new Staff { StaffId = staffId, IsActive = false }; // Already inactive

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);

            // Act
            var result = await _service.DeleteAsync(staffId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Staff is already inactive.", result.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteStaff_ThrowsException()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var staff = new Staff { StaffId = staffId, IsActive = true };

            _staffRepositoryMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            _staffRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteAsync(staffId));
        }
    }
}
