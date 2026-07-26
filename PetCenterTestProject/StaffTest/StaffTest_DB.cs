using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.ManageStaff;
using PetCenterAPI.DTOs.Responses.ManageStaff;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using Xunit;
using Role = PetCenterAPI.Models.Role;

namespace PetCenterTestProject.StaffTest
{
    [Collection("DatabaseTests")]
    public class StaffTest_DB
    {
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly PasswordService _passwordService;

        public StaffTest_DB()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
            _passwordService = new PasswordService();
        }

        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=.;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;",
                    builder => builder.EnableRetryOnFailure())
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                .Options;

            return new PetCenterContext(options);
        }

        private StaffRepository CreateRepository(PetCenterContext context)
        {
            return new StaffRepository(context);
        }

        private StaffService CreateService(PetCenterContext context)
        {
            return new StaffService(
                CreateRepository(context),
                _cloudinaryServiceMock.Object,
                _passwordService);
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
            context.MedicalRecords.RemoveRange(context.MedicalRecords);
            context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
            context.AppointmentServices.RemoveRange(context.AppointmentServices);
            context.Appointments.RemoveRange(context.Appointments);
            context.Pets.RemoveRange(context.Pets);
            context.CartDetails.RemoveRange(context.CartDetails);
            context.Carts.RemoveRange(context.Carts);
            context.VetFeedbacks.RemoveRange(context.VetFeedbacks);
            context.Customers.RemoveRange(context.Customers);
            context.VetProfiles.RemoveRange(context.VetProfiles);
            context.Set<Dictionary<string, object>>("StaffRole").RemoveRange(context.Set<Dictionary<string, object>>("StaffRole"));
            context.Staffs.RemoveRange(context.Staffs);
            context.Roles.RemoveRange(context.Roles);
            await context.SaveChangesAsync();
        }

        private async Task EnsureRoleExistsAsync(PetCenterContext context, Guid roleId, string roleName, bool isActive = true)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                context.Roles.Add(new Role
                {
                    RoleId = roleId,
                    RoleName = roleName,
                    IsActive = isActive
                });
                await context.SaveChangesAsync();
            }
            else
            {
                role.RoleName = roleName;
                role.IsActive = isActive;
                await context.SaveChangesAsync();
            }
        }

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
        // SECTION 1: GetAllAsync() Integration Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetAllAsync_ReturnsMappedStaffListSuccessfully()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var role = await context.Roles.FindAsync(roleId);

            var staff = new Staff
            {
                StaffId = Guid.NewGuid(),
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            var item = result.First();
            Assert.Equal(staff.StaffId, item.StaffId);
            Assert.Equal("Nguyen Van A", item.FullName);
            Assert.Equal("nguyenvana@pcss.com", item.Email);
            Assert.Equal("0359998888", item.PhoneNumber);
            Assert.Equal("Male", item.Gender);
            Assert.Equal("Sales", item.RoleName);
            Assert.Equal(roleId, item.RoleId);
        }

        [Fact]
        public async Task UTCID02_GetAllAsync_ReturnsEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetAllAsync_ThrowsException()
        {
            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("Database connection failed"));

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            await Assert.ThrowsAsync<Exception>(() => service.GetAllAsync());
        }

        // =========================================================================
        // SECTION 2: GetByIdAsync() Integration Tests (UTCID01 - UTCID06)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetStaffById_ReturnsStaffDetailsDTOWithoutVetProfile()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetByIdAsync(staffId);

            Assert.NotNull(result);
            Assert.Equal(staffId, result.StaffId);
            Assert.Null(result.VetProfile);
            Assert.Equal("Sales", result.RoleName);
        }

        [Fact]
        public async Task UTCID02_GetStaffById_ReturnsStaffDetailsDTOWithVetProfileAndAverageRating()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(role!);
            
            var vetProfile = new VetProfile
            {
                VetProfileId = Guid.NewGuid(),
                StaffId = staffId,
                ExperienceYears = 5,
                Description = "Expert vet",
                LicenseNumber = "VET-12345",
                IsActive = true
            };
            staff.VetProfile = vetProfile;
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var customer = new Customer { CustomerId = Guid.NewGuid(), FullName = "Customer 1" };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            // Add VetFeedback
            context.VetFeedbacks.Add(new VetFeedback
            {
                VetFeedbackId = Guid.NewGuid(),
                StaffId = staffId,
                CustomerId = customer.CustomerId,
                Star = 5,
                Comment = "Great!",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            });
            context.VetFeedbacks.Add(new VetFeedback
            {
                VetFeedbackId = Guid.NewGuid(),
                StaffId = staffId,
                CustomerId = customer.CustomerId,
                Star = 4,
                Comment = "Good",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetByIdAsync(staffId);

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(role!);

            var vetProfile = new VetProfile
            {
                VetProfileId = Guid.NewGuid(),
                StaffId = staffId,
                ExperienceYears = 5,
                Description = "Expert vet",
                LicenseNumber = "VET-12345",
                IsActive = false // Inactive
            };
            staff.VetProfile = vetProfile;
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetByIdAsync(staffId);

            Assert.NotNull(result);
            Assert.Null(result.VetProfile);
        }

        [Fact]
        public async Task UTCID04_GetStaffById_ReturnsStaffDetailsDTOWithNonAdminRoleName_WhenMultipleRolesIncludingAdmin()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var adminRoleId = Guid.NewGuid();
            var salesRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, adminRoleId, "Admin");
            await EnsureRoleExistsAsync(context, salesRoleId, "Sales");

            var adminRole = await context.Roles.FindAsync(adminRoleId);
            var salesRole = await context.Roles.FindAsync(salesRoleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = new DateTime(2025, 1, 1),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(adminRole!);
            staff.Roles.Add(salesRole!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetByIdAsync(staffId);

            Assert.NotNull(result);
            Assert.Equal("Sales", result.RoleName); // PickRole prefers non-Admin
            Assert.Equal(salesRoleId, result.RoleId);
        }

        [Fact]
        public async Task UTCID05_GetStaffById_ReturnsNull_WhenStaffIdDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID06_GetStaffById_ThrowsException()
        {
            var staffId = Guid.NewGuid();
            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(staffId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            await Assert.ThrowsAsync<Exception>(() => service.GetByIdAsync(staffId));
        }

        // =========================================================================
        // SECTION 3: CreateAsync() Integration Tests (UTCID01 - UTCID15)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateStaff_SuccessNonVet()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-1),
                Password = "password123",
                RoleId = roleId,
                Avatar = null
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.True(result.Success);
            Assert.Equal("Staff created successfully.", result.Message);
            Assert.NotNull(result.StaffId);

            var created = await context.Staffs.Include(s => s.Roles).FirstOrDefaultAsync(s => s.StaffId == result.StaffId);
            Assert.NotNull(created);
            Assert.Equal("Nguyen Van A", created.FullName);
            Assert.Equal("Sales", created.Roles.First().RoleName);
            Assert.Null(created.VetProfile);
        }

        [Fact]
        public async Task UTCID02_CreateStaff_SuccessVet()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");

            var file = CreateFakeAvatarFile();
            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(file, It.IsAny<string>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/avatar.jpg"),
                    PublicId = "staff_avatar"
                });

            var request = new CreateStaffRequestDTO
            {
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-1),
                Password = "password123",
                RoleId = roleId,
                LicenseNumber = "VET-11111",
                ExperienceYears = 3,
                Description = "A vet",
                Avatar = file
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.True(result.Success);
            Assert.NotNull(result.StaffId);

            var created = await context.Staffs.Include(s => s.VetProfile).FirstOrDefaultAsync(s => s.StaffId == result.StaffId);
            Assert.NotNull(created);
            Assert.NotNull(created.VetProfile);
            Assert.Equal("VET-11111", created.VetProfile.LicenseNumber);
            Assert.Equal(3, created.VetProfile.ExperienceYears);
            Assert.Equal("https://cloudinary.com/avatar.jpg", created.Avatar);
        }

        [Fact]
        public async Task UTCID03_CreateStaff_Fail_RoleDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351234567",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = Guid.NewGuid()
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID04_CreateStaff_Fail_RoleExistsButIsInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales", false); // Inactive

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351234567",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID05_CreateStaff_Fail_EmailAlreadyInUse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = Guid.NewGuid(),
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash"
            };
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Email 'a@pcss.com' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID06_CreateStaff_Fail_PhoneNumberAlreadyInUse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = Guid.NewGuid(),
                FullName = "A",
                Email = "other@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash"
            };
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID07_CreateStaff_Fail_AgeUnder18()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-17), // 17 years old
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Staff must be at least 18 years old.", result.Message);
        }

        [Fact]
        public async Task UTCID08_CreateStaff_Success_AgeExactly18()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-18), // Exactly 18
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID09_CreateStaff_Fail_HireDateInFuture()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(2), // Future
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Hire date cannot be in the future.", result.Message);
        }

        [Fact]
        public async Task UTCID10_CreateStaff_Success_HireDateExactlyToday()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow, // Exactly today
                Password = "pass",
                RoleId = roleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID11_CreateStaff_Fail_VetLicenseNumberNullOrWhitespace()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId,
                LicenseNumber = "   ", // Whitespace
                ExperienceYears = 3
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("License number is required for a veterinarian.", result.Message);
        }

        [Fact]
        public async Task UTCID12_CreateStaff_Fail_VetExperienceYearsNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId,
                LicenseNumber = "VET-1234",
                ExperienceYears = null // Null
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Experience years is required for a veterinarian.", result.Message);
        }

        [Fact]
        public async Task UTCID13_CreateStaff_Fail_VetLicenseNumberAlreadyExists()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Veterinarian");

            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = Guid.NewGuid(),
                FullName = "Existing Vet",
                Email = "existing@pcss.com",
                PhoneNumber = "0352223333",
                Gender = "Male",
                PasswordHash = "hash",
                VetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    LicenseNumber = "VET-123",
                    IsActive = true
                }
            };
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId,
                LicenseNumber = "VET-123", // Duplicate
                ExperienceYears = 3
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("License number 'VET-123' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID14_CreateStaff_Fail_AvatarUploadFails()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");

            var file = CreateFakeAvatarFile();
            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(file, It.IsAny<string>()))
                .ReturnsAsync((ImageUploadResult?)null); // Fails

            var request = new CreateStaffRequestDTO
            {
                FullName = "A",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Password = "pass",
                RoleId = roleId,
                Avatar = file
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.CreateAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Failed to upload avatar image.", result.Message);
        }

        [Fact]
        public async Task UTCID15_CreateStaff_ThrowsException()
        {
            var request = new CreateStaffRequestDTO
            { FullName = "A", Email = "a@pcss.com", Password = "p", Gender = "Male" };
            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetRoleByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("Database save failed"));

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(request));
        }

        // =========================================================================
        // SECTION 4: UpdateAsync() Integration Tests (UTCID01 - UTCID18)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateStaff_SuccessNonVet()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var salesRoleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, salesRoleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var salesRole = await context.Roles.FindAsync(salesRoleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Old Name",
                Email = "old@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(-2),
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(salesRole!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "New Name",
                Email = "new@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Female",
                BirthDate = DateTime.UtcNow.AddYears(-22),
                HireDate = DateTime.UtcNow.AddDays(-1),
                RoleId = managerRoleId,
                ResetPassword = false
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);
            Assert.Equal("Staff updated successfully.", result.Message);

            var updated = await context.Staffs.FindAsync(staffId);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated.FullName);
            Assert.Equal("new@pcss.com", updated.Email);
            Assert.Equal("0359998888", updated.PhoneNumber);
            Assert.Equal("Female", updated.Gender);
        }

        [Fact]
        public async Task UTCID02_UpdateStaff_SuccessWithResetPassword()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "old_hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                RoleId = managerRoleId,
                ResetPassword = true
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);

            var updated = await context.Staffs.FindAsync(staffId);
            Assert.NotNull(updated);
            Assert.True(_passwordService.Verify("123456", updated.PasswordHash));
        }

        [Fact]
        public async Task UTCID03_UpdateStaff_SuccessWithAvatarUpload()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PublicId = "old_avatar_id"
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var file = CreateFakeAvatarFile();
            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(file, It.IsAny<string>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/new_avatar.jpg"),
                    PublicId = "new_avatar_id"
                });

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                RoleId = managerRoleId,
                Avatar = file
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);

            var updated = await context.Staffs.FindAsync(staffId);
            Assert.NotNull(updated);
            Assert.Equal("https://cloudinary.com/new_avatar.jpg", updated.Avatar);
            Assert.Equal("new_avatar_id", updated.PublicId);
            _cloudinaryServiceMock.Verify(c => c.DeleteImageAsync("old_avatar_id"), Times.Once);
        }

        [Fact]
        public async Task UTCID04_UpdateStaff_Success_RoleChangedFromVetToNonVet()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var vetRoleId = Guid.NewGuid();
            var salesRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, vetRoleId, "Veterinarian");
            await EnsureRoleExistsAsync(context, salesRoleId, "Sales");

            var vetRole = await context.Roles.FindAsync(vetRoleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                VetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    StaffId = staffId,
                    LicenseNumber = "VET-1",
                    IsActive = true
                }
            };
            staff.Roles.Add(vetRole!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                RoleId = salesRoleId
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);

            var updated = await context.Staffs.Include(s => s.Roles).Include(s => s.VetProfile).FirstOrDefaultAsync(s => s.StaffId == staffId);
            Assert.NotNull(updated);
            Assert.Equal("Sales", updated.Roles.First().RoleName);
            Assert.NotNull(updated.VetProfile);
            Assert.False(updated.VetProfile.IsActive); // Deactivated
        }

        [Fact]
        public async Task UTCID05_UpdateStaff_Success_RoleChangedFromNonVetToVet_NoExistingVetProfile()
        {
            var staffId = Guid.NewGuid();
            var salesRoleId = Guid.NewGuid();
            var vetRoleId = Guid.NewGuid();

            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Old Name",
                Email = "old@pcss.com",
                PhoneNumber = "0123456789",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true
            };

            var salesRole = new Role { RoleId = salesRoleId, RoleName = "Sales", IsActive = true };
            var vetRole = new Role { RoleId = vetRoleId, RoleName = "Veterinarian", IsActive = true };

            staff.Roles.Add(salesRole);

            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            repoMock.Setup(repo => repo.GetRoleByIdAsync(vetRoleId)).ReturnsAsync(vetRole);
            repoMock.Setup(repo => repo.EmailExistsAsync(It.IsAny<string>(), staffId)).ReturnsAsync(false);
            repoMock.Setup(repo => repo.PhoneExistsAsync(It.IsAny<string>(), staffId)).ReturnsAsync(false);

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            var request = new UpdateStaffRequestDTO
            {
                FullName = "New Name",
                Email = "new@pcss.com",
                PhoneNumber = "0123456789",
                Gender = "Female",
                RoleId = vetRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-5),
                Description = "Expert vet"
            };

            var result = await service.UpdateAsync(staffId, request);
            Assert.True(result.Success);
            Assert.Equal("Veterinarian", staff.Roles.First().RoleName);
            Assert.NotNull(staff.VetProfile);
            Assert.True(staff.VetProfile.IsActive);
            Assert.Equal("Expert vet", staff.VetProfile.Description);
        }

        [Fact]
        public async Task UTCID06_UpdateStaff_Success_RoleChangedFromNonVetToVet_WithExistingVetProfile()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var salesRoleId = Guid.NewGuid();
            var vetRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, salesRoleId, "Sales");
            await EnsureRoleExistsAsync(context, vetRoleId, "Veterinarian");

            var salesRole = await context.Roles.FindAsync(salesRoleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                VetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    StaffId = staffId,
                    LicenseNumber = "VET-1",
                    Description = "Old desc",
                    ExperienceYears = 5,
                    IsActive = false // Inactive currently
                }
            };
            staff.Roles.Add(salesRole!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                RoleId = vetRoleId,
                Description = "New desc"
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);

            var updated = await context.Staffs.Include(s => s.VetProfile).FirstOrDefaultAsync(s => s.StaffId == staffId);
            Assert.NotNull(updated);
            Assert.NotNull(updated.VetProfile);
            Assert.True(updated.VetProfile.IsActive); // Reactivated
            Assert.Equal("New desc", updated.VetProfile.Description);
            Assert.Equal(5, updated.VetProfile.ExperienceYears); // Locked, not changed to 0
        }

        [Fact]
        public async Task UTCID07_UpdateStaff_Success_RoleRemainsVet()
        {
            var staffId = Guid.NewGuid();
            var vetRoleId = Guid.NewGuid();

            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Old Name",
                Email = "old@pcss.com",
                PhoneNumber = "0123456789",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true,
                VetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    StaffId = staffId,
                    Description = "Old desc",
                    IsActive = true
                }
            };

            var vetRole = new Role { RoleId = vetRoleId, RoleName = "Veterinarian", IsActive = true };
            staff.Roles.Add(vetRole);

            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(staffId)).ReturnsAsync(staff);
            repoMock.Setup(repo => repo.GetRoleByIdAsync(vetRoleId)).ReturnsAsync(vetRole);
            repoMock.Setup(repo => repo.EmailExistsAsync(It.IsAny<string>(), staffId)).ReturnsAsync(false);
            repoMock.Setup(repo => repo.PhoneExistsAsync(It.IsAny<string>(), staffId)).ReturnsAsync(false);

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            var request = new UpdateStaffRequestDTO
            {
                FullName = "New Name",
                Email = "new@pcss.com",
                PhoneNumber = "0123456789",
                Gender = "Female",
                RoleId = vetRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddDays(-5),
                Description = "New desc"
            };

            var result = await service.UpdateAsync(staffId, request);
            Assert.True(result.Success);
            Assert.Equal("New desc", staff.VetProfile.Description);
        }

        [Fact]
        public async Task UTCID08_UpdateStaff_Fail_StaffDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = new UpdateStaffRequestDTO();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(Guid.NewGuid(), request);

            Assert.False(result.Success);
            Assert.Equal("Staff not found.", result.Message);
        }

        [Fact]
        public async Task UTCID09_UpdateStaff_Fail_StaffInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = false // Inactive
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            { RoleId = roleId, Gender = "Male" };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.False(result.Success);
            Assert.Equal("Cannot update an inactive staff member.", result.Message);
        }

        [Fact]
        public async Task UTCID10_UpdateStaff_Fail_RoleDoesNotExistOrInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                Email = "a@pcss.com",
                Gender = "Male",
                PhoneNumber = "0351112222",
                FullName = "Name",
                RoleId = Guid.NewGuid() // Non-existent
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.False(result.Success);
            Assert.Equal("Selected role does not exist.", result.Message);
        }

        [Fact]
        public async Task UTCID11_UpdateStaff_Fail_EmailInUseByAnotherStaff()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId1 = Guid.NewGuid();
            var staff1 = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId1,
                FullName = "Name 1",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff1.Roles.Add(role!);

            var staffId2 = Guid.NewGuid();
            var staff2 = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId2,
                FullName = "Name 2",
                Email = "b@pcss.com",
                PhoneNumber = "0352223333",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff2.Roles.Add(role!);

            context.Staffs.AddRange(staff1, staff2);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name 2",
                Gender = "Male",
                Email = "a@pcss.com", // Duplicate of staff1
                PhoneNumber = "0352223333",
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId2, request);

            Assert.False(result.Success);
            Assert.Equal("Email 'a@pcss.com' is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID12_UpdateStaff_Fail_PhoneInUseByAnotherStaff()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staff1 = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = Guid.NewGuid(),
                FullName = "Name 1",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff1.Roles.Add(role!);

            var staffId2 = Guid.NewGuid();
            var staff2 = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId2,
                FullName = "Name 2",
                Email = "b@pcss.com",
                PhoneNumber = "0352223333",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff2.Roles.Add(role!);

            context.Staffs.AddRange(staff1, staff2);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name 2",
                Gender = "Male",
                Email = "b@pcss.com",
                PhoneNumber = "0351112222", // Duplicate of staff1
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId2, request);

            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use.", result.Message);
        }

        [Fact]
        public async Task UTCID13_UpdateStaff_Fail_AgeUnder18()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Gender = "Male",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                RoleId = roleId,
                BirthDate = DateTime.UtcNow.AddYears(-17), // 17 years old
                HireDate = DateTime.UtcNow
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.False(result.Success);
            Assert.Equal("Staff must be at least 18 years old.", result.Message);
        }

        [Fact]
        public async Task UTCID14_UpdateStaff_Success_AgeExactly18()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Gender = "Male",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-18), // Exactly 18
                HireDate = DateTime.UtcNow
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID15_UpdateStaff_Fail_HireDateInFuture()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Gender = "Male",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow.AddDays(2) // Future
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.False(result.Success);
            Assert.Equal("Hire date cannot be in the future.", result.Message);
        }

        [Fact]
        public async Task UTCID16_UpdateStaff_Success_HireDateExactlyToday()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Gender = "Male",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UTCID17_UpdateStaff_Fail_AvatarUploadFails()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                StaffId = staffId,
                FullName = "Name",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                Gender = "Male",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var file = CreateFakeAvatarFile();
            _cloudinaryServiceMock.Setup(c => c.UploadImageAsync(file, It.IsAny<string>()))
                .ReturnsAsync((ImageUploadResult?)null);

            var request = new UpdateStaffRequestDTO
            {
                FullName = "Name",
                Gender = "Male",
                Email = "a@pcss.com",
                PhoneNumber = "0351112222",
                RoleId = managerRoleId,
                BirthDate = DateTime.UtcNow.AddYears(-20),
                HireDate = DateTime.UtcNow,
                Avatar = file
            };

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.UpdateAsync(staffId, request);

            Assert.False(result.Success);
            Assert.Equal("Failed to upload avatar image.", result.Message);
        }

        [Fact]
        public async Task UTCID18_UpdateStaff_ThrowsException()
        {
            var staffId = Guid.NewGuid();
            var request = new UpdateStaffRequestDTO
            { FullName = "A", Gender = "Male" };
            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(staffId)).ThrowsAsync(new Exception("Database save failed"));

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(staffId, request));
        }

        // =========================================================================
        // SECTION 5: DeleteAsync() Integration Tests (UTCID01 - UTCID04)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteStaff_Success_SoftDeleteActiveStaff()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = true
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.DeleteAsync(staffId);

            Assert.True(result.Success);
            Assert.Equal("Staff deactivated successfully.", result.Message);

            var updated = await context.Staffs.FindAsync(staffId);
            Assert.NotNull(updated);
            Assert.False(updated.IsActive);
        }

        [Fact]
        public async Task UTCID02_DeleteStaff_Fail_StaffDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Staff not found.", result.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteStaff_Fail_StaffAlreadyInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var roleId = Guid.NewGuid();
            var managerRoleId = Guid.NewGuid();
            await EnsureRoleExistsAsync(context, roleId, "Sales");
            await EnsureRoleExistsAsync(context, managerRoleId, "Manager");
            var role = await context.Roles.FindAsync(roleId);

            var staffId = Guid.NewGuid();
            var staff = new Staff
            {
                CreatedAt = DateTime.UtcNow,
                HireDate = DateTime.UtcNow.AddDays(-10),
                BirthDate = DateTime.UtcNow.AddYears(-30),
                StaffId = staffId,
                FullName = "Nguyen Van A",
                Email = "nguyenvana@pcss.com",
                PhoneNumber = "0359998888",
                Gender = "Male",
                PasswordHash = "hash",
                IsActive = false // Already Inactive
            };
            staff.Roles.Add(role!);
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var service = CreateService(context);
            var result = await service.DeleteAsync(staffId);

            Assert.False(result.Success);
            Assert.Equal("Staff is already inactive.", result.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteStaff_ThrowsException()
        {
            var staffId = Guid.NewGuid();
            var repoMock = new Mock<IStaffRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(staffId)).ThrowsAsync(new Exception("Database save failed"));

            var service = new StaffService(repoMock.Object, _cloudinaryServiceMock.Object, _passwordService);
            await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(staffId));
        }
    }
}
