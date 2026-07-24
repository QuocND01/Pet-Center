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
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Service;
using Microsoft.Extensions.Options;
using PetCenterAPI.DTOs.Requests.Register;

namespace PetCenterTestProject.AuthTest
{
    public class CustomerAuthTest_DB
    {
        //=========================================================
        // Mock external service only
        //=========================================================
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly PasswordService _passwordService;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly IOptions<GoogleAuthSettings> _googleSettings;

        //=========================================================
        // Constructor
        //=========================================================
        public CustomerAuthTest_DB()
        {
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();
            _passwordService = new PasswordService();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());
            _googleSettings = Options.Create(new GoogleAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret"
            });
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
        private CustomerRepository CreateRepository(PetCenterContext context)
        {
            return new CustomerRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================
        private CustomerAuthService CreateService(PetCenterContext context)
        {
            return new CustomerAuthService(
                CreateRepository(context),
                _passwordService,
                _jwtServiceMock.Object,
                _emailServiceMock.Object);
        }

        //=========================================================
        // Create Google Service
        //=========================================================
        private GoogleAuthService CreateGoogleService(PetCenterContext context)
        {
            return new GoogleAuthService(
                _googleSettings,
                CreateRepository(context),
                _emailServiceMock.Object,
                _passwordService,
                _httpClientFactoryMock.Object);
        }

        //=========================================================
        // Clear Database
        //=========================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Pets");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM OtpCodes");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Addresses");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Customers");
        }

        //=========================================================
        // Helper: Build Customer entity with real BCrypt hash
        //=========================================================
        private Customer BuildCustomer(
                string email,
                string? rawPassword,
                bool? isVerified,
                bool? isActive,
                string phoneNumber = "0900000000")
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                FullName = "Customer User",
                PhoneNumber = phoneNumber,
                BirthDay = new DateOnly(1995, 1, 1),
                Gender = "Male",
                Email = email,
                CreatedAt = DateTime.Now,
                IsVerified = isVerified,
                PasswordHash = rawPassword == null ? null : _passwordService.Hash(rawPassword),
                IsActive = isActive
            };
        }

        //=========================================================
        // Helper: Build a valid RegisterRequestDTO, override fields as needed
        //=========================================================
        private RegisterRequestDTO BuildRegisterRequest(
            string fullName = "Nguyen Van A",
            string email = "newcustomer@petcenter.com",
            string phoneNumber = "0912345678",
            string password = "Abc@123",
            DateOnly? birthDay = null,
            string? gender = "Male")
        {
            return new RegisterRequestDTO
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = password,
                BirthDay = birthDay ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                Gender = gender
            };
        }

        //=========================================================
        // LoginAsync()
        // UTCID01 - Customer not found
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID01_LoginAsync_CustomerNotFound_ReturnInvalidCredentials()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync("notfound@petcenter.com", "Customer@123");

            // Assert
            Assert.False(result.success);
            Assert.Null(result.token);
            Assert.Equal("InvalidCredentials", result.errorType);
            Assert.Equal("Email or password incorrect", result.message);
        }

        //=========================================================
        // UTCID02 - Customer exists, PasswordHash is null
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID02_LoginAsync_PasswordHashIsNull_ReturnInvalidCredentials()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "nohash@petcenter.com",
                rawPassword: null,
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(customer.Email!, "Customer@123");

            // Assert
            Assert.False(result.success);
            Assert.Null(result.token);
            Assert.Equal("InvalidCredentials", result.errorType);
            Assert.Equal("Email or password incorrect", result.message);
        }

        //=========================================================
        // UTCID03 - Customer exists, PasswordHash exists, Password does not match
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID03_LoginAsync_PasswordNotMatch_ReturnInvalidCredentials()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(customer.Email!, "WrongPassword");

            // Assert
            Assert.False(result.success);
            Assert.Null(result.token);
            Assert.Equal("InvalidCredentials", result.errorType);
            Assert.Equal("Email or password incorrect", result.message);
        }

        //=========================================================
        // UTCID04 - Customer exists, Password matches, Email not verified
        // Expected: Authentication failed - EmailNotVerified
        //=========================================================
        [Fact]
        public async Task UTCID04_LoginAsync_EmailNotVerified_ReturnEmailNotVerified()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(customer.Email!, "Customer@123");

            // Assert
            Assert.False(result.success);
            Assert.Null(result.token);
            Assert.Equal("EmailNotVerified", result.errorType);
            Assert.Equal("Your account is not verified. Please register again.", result.message);
        }

        //=========================================================
        // UTCID05 - Customer exists, Password matches, Verified, Account inactive
        // Expected: Authentication failed - AccountInactive
        //=========================================================
        [Fact]
        public async Task UTCID05_LoginAsync_AccountInactive_ReturnAccountInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(customer.Email!, "Customer@123");

            // Assert
            Assert.False(result.success);
            Assert.Null(result.token);
            Assert.Equal("AccountInactive", result.errorType);
            Assert.Equal("Your account has been deactivated. Please contact support.", result.message);
        }

        //=========================================================
        // UTCID06 - Customer exists, Password matches, Verified, Active
        // Expected: JWT generated - Login success
        //=========================================================
        [Fact]
        public async Task UTCID06_LoginAsync_ValidCredentials_ReturnLoginSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    customer.CustomerId,
                    customer.Email!,
                    It.Is<List<string>>(r => r.Count == 1 && r.Contains("Customer")),
                    customer.FullName ?? ""))
                .Returns("fake-jwt-token");

            var service = CreateService(context);

            // Act
            var result = await service.LoginAsync(customer.Email!, "Customer@123");

            // Assert
            Assert.True(result.success);
            Assert.Equal("fake-jwt-token", result.token);
            Assert.Null(result.errorType);
            Assert.Equal("Login success", result.message);
        }

        //=========================================================
        // UTCID08 - Customer valid, JWT service throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID08_LoginAsync_JwtServiceThrowsException_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
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
                service.LoginAsync(customer.Email!, "Customer@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // GetOrCreateUserFromGoogleAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Existing customer found with given email
        // Expected: Existing customer returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetOrCreateUserFromGoogleAsync_ExistingCustomerFound_ReturnExistingCustomer()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var existingCustomer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "existing@gmail.com",
                FullName = "Existing User",
                PasswordHash = _passwordService.Hash("Temp@123"),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            context.Customers.Add(existingCustomer);
            await context.SaveChangesAsync();

            var service = CreateGoogleService(context);

            // Act
            var result = await service.GetOrCreateUserFromGoogleAsync(
                existingCustomer.Email!, existingCustomer.FullName!);

            // Assert
            Assert.Equal(existingCustomer.CustomerId, result.CustomerId);
            Assert.Equal(existingCustomer.Email, result.Email);
            _emailServiceMock.Verify(
                x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        //=========================================================
        // UTCID02 - Customer not found -> create new, send welcome email
        // Expected: New customer created and returned, initialized correctly
        //=========================================================
        [Fact]
        public async Task UTCID02_GetOrCreateUserFromGoogleAsync_CustomerNotFound_ReturnNewCustomer()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var email = "newuser@gmail.com";
            var name = "New User";
            string? capturedTempPassword = null;

            _emailServiceMock
                .Setup(x => x.SendWelcomeEmailAsync(email, name, It.IsAny<string>()))
                .Callback<string, string, string>((_, __, tempPassword) =>
                    capturedTempPassword = tempPassword)
                .ReturnsAsync(true);

            var service = CreateGoogleService(context);

            // Act
            var result = await service.GetOrCreateUserFromGoogleAsync(email, name);

            // Assert
            Assert.Equal(email, result.Email);
            Assert.Equal(name, result.FullName);
            Assert.True(result.IsActive);
            Assert.True(result.IsVerified);
            Assert.NotNull(result.PasswordHash);
            Assert.NotNull(capturedTempPassword);
            Assert.True(_passwordService.Verify(capturedTempPassword!, result.PasswordHash!));

            var savedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
            Assert.NotNull(savedCustomer);
            Assert.Equal(result.CustomerId, savedCustomer!.CustomerId);
        }

        //=========================================================
        // UTCID05 - Customer not found, Customer added successfully,
        //           Email service throws exception
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID05_GetOrCreateUserFromGoogleAsync_EmailServiceThrowsException_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var email = "newuser@gmail.com";
            var name = "New User";

            _emailServiceMock
                .Setup(x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            var service = CreateGoogleService(context);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetOrCreateUserFromGoogleAsync(email, name));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);

            var savedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
            Assert.NotNull(savedCustomer);
        }

        //=========================================================
        //=========================================================
        // RegisterAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID08 - Email already registered, verified customer exists
        // Expected: "Email is already registered. Please login."
        //=========================================================
        [Fact]
        public async Task UTCID08_RegisterAsync_EmailAlreadyRegisteredVerified_ReturnAlreadyRegistered()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildRegisterRequest(email: "existing@petcenter.com");
            var existingCustomer = BuildCustomer(request.Email, "Customer@123", isVerified: true, isActive: true);

            context.Customers.Add(existingCustomer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email is already registered. Please login.", result.Message);

            var customerCountWithEmail = await context.Customers
                .CountAsync(c => c.Email == request.Email);
            Assert.Equal(1, customerCountWithEmail);
        }

        //=========================================================
        // UTCID011 - Phone number already exists (verified customer)
        // Expected: "Phone number is already in use by another account."
        //=========================================================
        [Fact]
        public async Task UTCID011_RegisterAsync_PhoneAlreadyExists_ReturnPhoneInUse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildRegisterRequest(email: "newcustomer@petcenter.com", phoneNumber: "0912345678");
            var existingPhoneCustomer = BuildCustomer(
                "other@petcenter.com", "Customer@123", isVerified: true, isActive: true,
                phoneNumber: request.PhoneNumber);

            context.Customers.Add(existingPhoneCustomer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use by another account.", result.Message);

            var newCustomerExists = await context.Customers
                .AnyAsync(c => c.Email == request.Email);
            Assert.False(newCustomerExists);
        }

        //=========================================================
        // UTCID019 - Send verification email throws
        // (Customer & OTP đã ghi DB thành công trước khi email throw)
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID019_RegisterAsync_SendVerificationEmailThrows_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildRegisterRequest(email: "newcustomer2@petcenter.com");

            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(request.Email, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            var service = CreateService(context);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(request));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);

            var savedCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            Assert.NotNull(savedCustomer);

            var savedOtp = await context.OtpCodes.FirstOrDefaultAsync(o => o.CustomerId == savedCustomer!.CustomerId);
            Assert.NotNull(savedOtp);
        }

        //=========================================================
        // UTCID020 - Unverified customer exists with existing OTP,
        //            full success path (customer & OTP cũ bị xoá, tạo mới)
        // Expected: Registration completed successfully
        //=========================================================
        [Fact]
        public async Task UTCID020_RegisterAsync_UnverifiedCustomerWithExistingOtp_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildRegisterRequest(email: "unverified@petcenter.com");
            var existingCustomer = BuildCustomer(request.Email, "OldPass@123", isVerified: false, isActive: false);

            context.Customers.Add(existingCustomer);
            await context.SaveChangesAsync();

            var existingOtp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = existingCustomer.CustomerId,
                VerificationCode = "111111",
                VerificationExpire = DateTime.UtcNow.AddMinutes(5),
                LastOtpSentAt = DateTime.UtcNow,
                OtpAttemptCount = 0
            };

            context.OtpCodes.Add(existingOtp);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(request.Email, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(context);
            var oldCustomerId = existingCustomer.CustomerId;

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(
                "Verification code sent to your email. Please verify within 5 minutes.",
                result.Message);

            var oldCustomerStillExists = await context.Customers.AnyAsync(c => c.CustomerId == oldCustomerId);
            Assert.False(oldCustomerStillExists);

            var oldOtpStillExists = await context.OtpCodes.AnyAsync(o => o.OtpId == existingOtp.OtpId);
            Assert.False(oldOtpStillExists);

            var newCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            Assert.NotNull(newCustomer);
            Assert.NotEqual(oldCustomerId, newCustomer!.CustomerId);
            Assert.False(newCustomer.IsVerified);

            var newOtp = await context.OtpCodes.FirstOrDefaultAsync(o => o.CustomerId == newCustomer.CustomerId);
            Assert.NotNull(newOtp);
        }
    }
}
