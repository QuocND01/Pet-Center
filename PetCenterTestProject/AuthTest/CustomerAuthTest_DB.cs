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
using Microsoft.Extensions.Configuration;
using PetCenterAPI.DTOs.Requests.CustomerProfile;

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
        private readonly Mock<IConfiguration> _configurationMock;

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
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock
                .Setup(x => x["ClientBaseUrl"])
                .Returns("https://localhost:7010");
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
        // Create Forgot Password Service
        //=========================================================
        private ForgotPasswordService CreateForgotPasswordService(PetCenterContext context)
        {
            return new ForgotPasswordService(
                CreateRepository(context),
                _emailServiceMock.Object,
                _passwordService,
                _configurationMock.Object);
        }

        //=========================================================
        // Clear Database
        //=========================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM FeedbackImages");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM ProductFeedbacks");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM VetFeedbacks");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Orders");
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
        // Helper: Build a valid VerifyOtpRequestDTO
        //=========================================================
        private VerifyOtpRequestDTO BuildVerifyOtpRequest(
            string email,
            string code = "123456")
        {
            return new VerifyOtpRequestDTO
            {
                Email = email,
                Code = code
            };
        }

        //=========================================================
        // Helper: Build an OtpCode entity
        //=========================================================
        private OtpCode BuildOtp(
            Guid customerId,
            string code = "123456",
            DateTime? verificationExpire = null,
            int? otpAttemptCount = 0)
        {
            return new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customerId,
                VerificationCode = code,
                VerificationExpire = verificationExpire ?? DateTime.Now.AddMinutes(5),
                LastOtpSentAt = DateTime.Now,
                OtpAttemptCount = otpAttemptCount
            };
        }

        //=========================================================
        // Helper: Build an OtpCode with a hashed reset token
        //=========================================================
        private OtpCode BuildResetOtp(Guid customerId, string rawToken, bool expired = false)
        {
            return new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customerId,
                PasswordResetToken = _passwordService.Hash(rawToken),
                PasswordResetExpire = expired
                    ? DateTime.Now.AddMinutes(-1)
                    : DateTime.Now.AddMinutes(10)
            };
        }

        //=========================================================
        // Helper: Build a valid ChangePasswordRequestDTO
        //=========================================================
        private ChangePasswordRequestDTO BuildChangePasswordRequest(
            string currentPassword,
            string newPassword,
            string? confirmNewPassword = null)
        {
            return new ChangePasswordRequestDTO
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmNewPassword ?? newPassword
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
                VerificationExpire = DateTime.Now.AddMinutes(5),
                LastOtpSentAt = DateTime.Now,
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

        //=========================================================
        //=========================================================
        // VerifyOtpAsync()
        // Bỏ UTCID09 (UpdateOtpAsync throws) vì không giả lập được
        // lỗi repository qua DB thật.
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID03 - Customer not found
        // Expected: "Registration session not found. Please register again."
        //=========================================================
        [Fact]
        public async Task UTCID03_VerifyOtpAsync_CustomerNotFound_ReturnSessionNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildVerifyOtpRequest("notfound@petcenter.com");
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registration session not found. Please register again.", result.Message);
        }

        //=========================================================
        // UTCID04 - Customer exists, IsVerified = true
        // Expected: "Email already verified. Please login."
        //=========================================================
        [Fact]
        public async Task UTCID04_VerifyOtpAsync_CustomerAlreadyVerified_ReturnAlreadyVerified()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "verified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!);
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email already verified. Please login.", result.Message);
        }

        //=========================================================
        // UTCID05 - Customer exists, IsVerified = false, OTP not found
        // Expected: "No OTP found. Please register again."
        //=========================================================
        [Fact]
        public async Task UTCID05_VerifyOtpAsync_OtpNotFound_ReturnNoOtpFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!);
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No OTP found. Please register again.", result.Message);
        }

        //=========================================================
        // UTCID06 - OTP record exists, OtpAttemptCount >= 5
        // Expected: "Too many incorrect attempts. Please register again."
        //=========================================================
        [Fact]
        public async Task UTCID06_VerifyOtpAsync_TooManyAttempts_ReturnTooManyAttempts()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, otpAttemptCount: 5);
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!);
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Too many incorrect attempts. Please register again.", result.Message);
        }

        //=========================================================
        // UTCID07 - OTP record exists, OTP expired
        // Expected: "Verification code expired. Please resend OTP."
        //=========================================================
        [Fact]
        public async Task UTCID07_VerifyOtpAsync_OtpExpired_ReturnCodeExpired()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(
                customer.CustomerId,
                code: "123456",
                verificationExpire: DateTime.Now.AddMinutes(-1)); // đã hết hạn
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!);
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Verification code expired. Please resend OTP.", result.Message);
        }

        //=========================================================
        // UTCID08 - OTP code not matched, UpdateOtpAsync succeeds
        // Expected: "Invalid code. 4 attempts left."
        //=========================================================
        [Fact]
        public async Task UTCID08_VerifyOtpAsync_CodeNotMatched_ReturnInvalidCodeWithAttemptsLeft()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, code: "123456", otpAttemptCount: 0);
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!, code: "999999");
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid code. 4 attempts left.", result.Message);

            var updatedOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.OtpId == otp.OtpId);
            Assert.NotNull(updatedOtp);
            Assert.Equal(1, updatedOtp!.OtpAttemptCount);
        }

        //=========================================================
        // UTCID010 - OTP code matched, UpdateAsync(Customer) fails
        // Ghi chú: không thể giả lập UpdateAsync throw qua DB thật với
        // repository chuẩn, nên bỏ qua case này ở file DB (chỉ có ở Mock).
        //=========================================================

        //=========================================================
        // UTCID011 - OTP code matched, DeleteOtpAsync fails
        // Ghi chú: tương tự UTCID010, chỉ có ở file Mock.
        //=========================================================

        //=========================================================
        // UTCID012 - OTP code matched, Update customer succeeds,
        //            Delete OTP succeeds
        // Expected: Verification completed successfully
        //=========================================================
        [Fact]
        public async Task UTCID012_VerifyOtpAsync_ValidCode_ReturnVerificationSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, code: "123456", otpAttemptCount: 0);
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var request = BuildVerifyOtpRequest(customer.Email!, code: "123456");
            var service = CreateService(context);

            // Act
            var result = await service.VerifyOtpAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Email verified successfully. You can now login.", result.Message);

            var updatedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);
            Assert.NotNull(updatedCustomer);
            Assert.True(updatedCustomer!.IsVerified);
            Assert.True(updatedCustomer.IsActive);

            var deletedOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.OtpId == otp.OtpId);
            Assert.Null(deletedOtp);
        }

        //=========================================================
        //=========================================================
        // ResendOtpAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: "Registration session not found. Please register again."
        //=========================================================
        [Fact]
        public async Task UTCID02_ResendOtpAsync_CustomerNotFound_ReturnSessionNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.ResendOtpAsync("notfound@petcenter.com");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registration session not found. Please register again.", result.Message);
        }

        //=========================================================
        // UTCID03 - Customer exists, IsVerified = true
        // Expected: "Email already verified. Please login."
        //=========================================================
        [Fact]
        public async Task UTCID03_ResendOtpAsync_CustomerAlreadyVerified_ReturnAlreadyVerified()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "verified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ResendOtpAsync(customer.Email!);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email already verified. Please login.", result.Message);
        }

        //=========================================================
        // UTCID04 - OTP record exists, Last OTP sent < 30 seconds
        // Expected: "Please wait {N} seconds before resending."
        //=========================================================
        [Fact]
        public async Task UTCID04_ResendOtpAsync_CooldownNotElapsed_ReturnPleaseWait()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, otpAttemptCount: 0);
            otp.LastOtpSentAt = DateTime.Now.AddSeconds(-10); // mới gửi cách đây 10s
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ResendOtpAsync(customer.Email!);

            // Assert
            Assert.False(result.Success);
            Assert.StartsWith("Please wait", result.Message);
            Assert.EndsWith("seconds before resending.", result.Message);
        }

        //=========================================================
        // UTCID05 - OTP record does not exist, AddOtpAsync succeeds,
        //           Send verification email succeeds
        // Expected: "New verification code sent."
        //=========================================================
        [Fact]
        public async Task UTCID05_ResendOtpAsync_OtpNotExist_ReturnNewCodeSent()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(customer.Email!, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(context);

            // Act
            var result = await service.ResendOtpAsync(customer.Email!);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New verification code sent.", result.Message);

            var createdOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.CustomerId == customer.CustomerId);
            Assert.NotNull(createdOtp);
        }

        //=========================================================
        // UTCID06 - OTP record exists, Last OTP sent >= 30 seconds,
        //           UpdateOtpAsync succeeds, Send verification email succeeds
        // Expected: "New verification code sent."
        //=========================================================
        [Fact]
        public async Task UTCID06_ResendOtpAsync_CooldownElapsedOtpExists_ReturnNewCodeSent()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, otpAttemptCount: 2, code: "111111");
            otp.LastOtpSentAt = DateTime.Now.AddSeconds(-31); // đã qua cooldown
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(customer.Email!, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(context);

            // Act
            var result = await service.ResendOtpAsync(customer.Email!);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New verification code sent.", result.Message);

            var updatedOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.OtpId == otp.OtpId);
            Assert.NotNull(updatedOtp);
            Assert.Equal(0, updatedOtp!.OtpAttemptCount); // reset về 0 sau khi resend
            Assert.NotEqual("111111", updatedOtp.VerificationCode); // code mới khác code cũ
        }

        //=========================================================
        // UTCID09 - OTP record exists, Last OTP sent >= 30 seconds,
        //           UpdateOtpAsync succeeds, Send verification email throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID09_ResendOtpAsync_SendEmailThrows_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildOtp(customer.CustomerId, otpAttemptCount: 1);
            otp.LastOtpSentAt = DateTime.Now.AddSeconds(-31);
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(customer.Email!, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            var service = CreateService(context);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ResendOtpAsync(customer.Email!));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // SendResetPasswordEmailAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: "This email is not registered in our system."
        //=========================================================
        [Fact]
        public async Task UTCID02_SendResetPasswordEmailAsync_CustomerNotFound_ReturnNotRegistered()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.SendResetPasswordEmailAsync("notfound@petcenter.com");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This email is not registered in our system.", result.Message);
        }

        //=========================================================
        // UTCID03 - Customer exists, IsVerified = false
        // Expected: "This email has not been verified yet. Please complete your registration first."
        //=========================================================
        [Fact]
        public async Task UTCID03_SendResetPasswordEmailAsync_EmailNotVerified_ReturnNotVerified()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "unverified@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: false,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.SendResetPasswordEmailAsync(customer.Email!);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "This email has not been verified yet. Please complete your registration first.",
                result.Message);
        }

        //=========================================================
        // UTCID04 - Customer exists, IsVerified = true, IsActive = false
        // Expected: "This account has been deactivated. Please contact support."
        //=========================================================
        [Fact]
        public async Task UTCID04_SendResetPasswordEmailAsync_AccountInactive_ReturnDeactivated()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "inactive@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.SendResetPasswordEmailAsync(customer.Email!);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This account has been deactivated. Please contact support.", result.Message);
        }

        //=========================================================
        // UTCID05 - OTP record does not exist, AddOtpAsync succeeds,
        //           Send reset email succeeds
        // Expected: "If this email is registered, you will receive a password reset link shortly."
        //=========================================================
        [Fact]
        public async Task UTCID05_SendResetPasswordEmailAsync_OtpNotExist_ReturnResetLinkSent()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendResetPasswordEmailAsync(
                    customer.Email!, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.SendResetPasswordEmailAsync(customer.Email!);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(
                "If this email is registered, you will receive a password reset link shortly.",
                result.Message);

            var createdOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.CustomerId == customer.CustomerId);
            Assert.NotNull(createdOtp);
            Assert.NotNull(createdOtp!.PasswordResetToken);
            Assert.NotNull(createdOtp.PasswordResetExpire);
        }

        //=========================================================
        // UTCID06 - OTP record exists, UpdateOtpAsync succeeds,
        //           Send reset email succeeds
        // Expected: "If this email is registered, you will receive a password reset link shortly."
        //=========================================================
        [Fact]
        public async Task UTCID06_SendResetPasswordEmailAsync_OtpExists_ReturnResetLinkSent()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = "old-hash",
                PasswordResetExpire = DateTime.Now.AddMinutes(-1)
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendResetPasswordEmailAsync(
                    customer.Email!, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.SendResetPasswordEmailAsync(customer.Email!);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(
                "If this email is registered, you will receive a password reset link shortly.",
                result.Message);

            var updatedOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.OtpId == otp.OtpId);
            Assert.NotNull(updatedOtp);
            Assert.NotEqual("old-hash", updatedOtp!.PasswordResetToken);
        }

        //=========================================================
        // UTCID09 - OTP record exists, Send reset email throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID09_SendResetPasswordEmailAsync_SendEmailThrows_ThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            _emailServiceMock
                .Setup(x => x.SendResetPasswordEmailAsync(
                    customer.Email!, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            var service = CreateForgotPasswordService(context);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.SendResetPasswordEmailAsync(customer.Email!));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // ValidateResetTokenAsync()
        // Bỏ UTCID01 (Email empty check nằm ở đầu Service, không đụng
        // repository/DB, đã kiểm chứng đủ ở Mock).
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID02_ValidateResetTokenAsync_CustomerNotFound_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync("notfound@petcenter.com", "some-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID03 - Customer exists, OTP record not found
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID03_ValidateResetTokenAsync_OtpNotFound_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "some-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID04 - OTP record exists, PasswordResetToken = null
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID04_ValidateResetTokenAsync_TokenHashNull_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = null,
                PasswordResetExpire = DateTime.Now.AddMinutes(10)
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "some-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID05 - OTP record exists, PasswordResetExpire = null
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID05_ValidateResetTokenAsync_ExpireNull_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = _passwordService.Hash("some-token"),
                PasswordResetExpire = null
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "some-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID06 - OTP record exists, PasswordResetExpire < Current Time
        // Expected: "This reset link has expired. Please request a new one."
        //=========================================================
        [Fact]
        public async Task UTCID06_ValidateResetTokenAsync_TokenExpired_ReturnExpiredLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = _passwordService.Hash("some-token"),
                PasswordResetExpire = DateTime.Now.AddMinutes(-1) // đã hết hạn
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "some-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("This reset link has expired. Please request a new one.", result.Message);
        }

        //=========================================================
        // UTCID07 - OTP record exists, PasswordResetToken does not match
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID07_ValidateResetTokenAsync_TokenNotMatch_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = _passwordService.Hash("correct-token"),
                PasswordResetExpire = DateTime.Now.AddMinutes(10)
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "wrong-token");

            // Assert
            Assert.False(result.Valid);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID08 - OTP record exists, PasswordResetToken matches
        // Expected: Valid = true, "Token is valid."
        //=========================================================
        [Fact]
        public async Task UTCID08_ValidateResetTokenAsync_TokenMatches_ReturnTokenValid()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                PasswordResetToken = _passwordService.Hash("correct-token"),
                PasswordResetExpire = DateTime.Now.AddMinutes(10)
            };
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ValidateResetTokenAsync(customer.Email!, "correct-token");

            // Assert
            Assert.True(result.Valid);
            Assert.Equal("Token is valid.", result.Message);
        }

        //=========================================================
        //=========================================================
        // ResetPasswordAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - ValidateResetTokenAsync = Invalid (customer not found)
        // Expected: "Invalid or expired reset link."
        //=========================================================
        [Fact]
        public async Task UTCID04_ResetPasswordAsync_TokenInvalid_ReturnInvalidLink()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ResetPasswordAsync(
                "notfound@petcenter.com", "some-token", "Abc@123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid or expired reset link.", result.Message);
        }

        //=========================================================
        // UTCID07 - Valid token, Customer exists, OTP record exists,
        //           UpdateAsync succeeds, UpdateOtpAsync succeeds
        // Expected: Password reset completed successfully, reset token cleared
        //=========================================================
        [Fact]
        public async Task UTCID07_ResetPasswordAsync_OtpExists_ReturnSuccessAndClearToken()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "valid@petcenter.com",
                rawPassword: "Customer@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var otp = BuildResetOtp(customer.CustomerId, "correct-token");
            context.OtpCodes.Add(otp);
            await context.SaveChangesAsync();

            var service = CreateForgotPasswordService(context);

            // Act
            var result = await service.ResetPasswordAsync(
                customer.Email!, "correct-token", "NewPass@123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(
                "Your password has been reset successfully. You can now login.",
                result.Message);

            var updatedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);
            Assert.NotNull(updatedCustomer);
            Assert.True(_passwordService.Verify("NewPass@123", updatedCustomer!.PasswordHash!));

            var updatedOtp = await context.OtpCodes
                .FirstOrDefaultAsync(o => o.OtpId == otp.OtpId);
            Assert.NotNull(updatedOtp);
            Assert.Null(updatedOtp!.PasswordResetToken);
            Assert.Null(updatedOtp.PasswordResetExpire);
        }

        //=========================================================
        //=========================================================
        // ChangePasswordAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - Customer not found
        // Expected: "Customer not found."
        //=========================================================
        [Fact]
        public async Task UTCID04_ChangePasswordAsync_CustomerNotFound_ReturnCustomerNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildChangePasswordRequest("OldPass@123", "NewPass@123");

            // Act
            var result = await service.ChangePasswordAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Customer not found.", result.Message);
        }

        //=========================================================
        // UTCID05 - Customer exists, PasswordHash = null
        // Expected: "Current password is incorrect."
        //=========================================================
        [Fact]
        public async Task UTCID05_ChangePasswordAsync_PasswordHashNull_ReturnCurrentPasswordIncorrect()
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
            var request = BuildChangePasswordRequest("AnyPassword@123", "NewPass@123");

            // Act
            var result = await service.ChangePasswordAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect.", result.Message);
        }

        //=========================================================
        // UTCID06 - Customer exists, Current password incorrect
        // Expected: "Current password is incorrect."
        //=========================================================
        [Fact]
        public async Task UTCID06_ChangePasswordAsync_CurrentPasswordIncorrect_ReturnIncorrect()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "OldPass@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = BuildChangePasswordRequest("WrongPassword", "NewPass@123");

            // Act
            var result = await service.ChangePasswordAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect.", result.Message);
        }

        //=========================================================
        // UTCID08 - Customer exists, Current password correct, UpdateAsync succeeds
        // Expected: "Password changed successfully."
        //=========================================================
        [Fact]
        public async Task UTCID08_ChangePasswordAsync_ValidRequest_ReturnPasswordChangedSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                rawPassword: "OldPass@123",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var service = CreateService(context);
            var request = BuildChangePasswordRequest("OldPass@123", "NewPass@456");

            // Act
            var result = await service.ChangePasswordAsync(customer.CustomerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Password changed successfully.", result.Message);

            var updatedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);
            Assert.NotNull(updatedCustomer);
            Assert.True(_passwordService.Verify("NewPass@456", updatedCustomer!.PasswordHash!));
        }
    }
}
