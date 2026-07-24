using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Service;
using Microsoft.Extensions.Options;
using System.Net;
using PetCenterAPI.DTOs.Requests.Register;
using System.ComponentModel.DataAnnotations;

namespace PetCenterTestProject.AuthTest
{
    public class CustomerAuthTest_Mock
    {
        //=========================================================
        // Helper: Fake HttpMessageHandler to simulate Google OAuth response
        //=========================================================
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }

        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly PasswordService _passwordService;
        private readonly IOptions<GoogleAuthSettings> _googleSettings;
        private readonly CustomerAuthService _service;
        private readonly GoogleAuthService _googleService;

        //=========================================================
        // Constructor
        //=========================================================
        public CustomerAuthTest_Mock()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _passwordService = new PasswordService();

            // LoginAsync (CustomerAuthService) không dùng HttpClient
            _service = new CustomerAuthService(
                _customerRepositoryMock.Object,
                _passwordService,
                _jwtServiceMock.Object,
                _emailServiceMock.Object);

            // GoogleAuthService dùng chung repository/email/password mock ở trên,
            // riêng HttpClient mặc định trả về client rỗng (dùng cho
            // GetOrCreateUserFromGoogleAsync — không gọi HTTP thật)
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            _googleSettings = Options.Create(new GoogleAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret"
            });

            _googleService = new GoogleAuthService(
                _googleSettings,
                _customerRepositoryMock.Object,
                _emailServiceMock.Object,
                _passwordService,
                _httpClientFactoryMock.Object);
        }

        //=========================================================
        // Helper: Build Customer entity with real BCrypt hash
        //=========================================================
        private Customer BuildCustomer(
            string email = "customer@petcenter.com",
            string? rawPassword = "Customer@123",
            bool? isVerified = true,
            bool? isActive = true)
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = email,
                FullName = "Customer User",
                PasswordHash = rawPassword == null ? null : _passwordService.Hash(rawPassword),
                IsVerified = isVerified,
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
        // Helper: Build GoogleAuthService with a fake HTTP response
        // (dùng riêng cho ExchangeCodeForIdTokenAsync test)
        //=========================================================
        private GoogleAuthService BuildGoogleServiceWithHttpResponse(
            HttpStatusCode statusCode, string jsonContent)
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

            var httpClient = new HttpClient(handler);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return new GoogleAuthService(
                _googleSettings,
                _customerRepositoryMock.Object,
                _emailServiceMock.Object,
                _passwordService,
                httpClientFactoryMock.Object);
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
        //=========================================================
        // LoginAsync() — Email/Password login
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Customer not found
        // Expected: Authentication failed - InvalidCredentials
        //=========================================================
        [Fact]
        public async Task UTCID01_LoginAsync_CustomerNotFound_ReturnInvalidCredentials()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _service.LoginAsync("notfound@petcenter.com", "Customer@123");

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
            // Arrange
            var customer = BuildCustomer(rawPassword: null);
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.LoginAsync(customer.Email!, "Customer@123");

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
            // Arrange
            var customer = BuildCustomer(rawPassword: "Customer@123");
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.LoginAsync(customer.Email!, "WrongPassword");

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
            // Arrange
            var customer = BuildCustomer(rawPassword: "Customer@123", isVerified: false);
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.LoginAsync(customer.Email!, "Customer@123");

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
            // Arrange
            var customer = BuildCustomer(rawPassword: "Customer@123", isVerified: true, isActive: false);
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.LoginAsync(customer.Email!, "Customer@123");

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
            // Arrange
            var customer = BuildCustomer(rawPassword: "Customer@123", isVerified: true, isActive: true);
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);
            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    customer.CustomerId,
                    customer.Email!,
                    It.Is<List<string>>(r => r.Count == 1 && r.Contains("Customer")),
                    customer.FullName ?? ""))
                .Returns("fake-jwt-token");

            // Act
            var result = await _service.LoginAsync(customer.Email!, "Customer@123");

            // Assert
            Assert.True(result.success);
            Assert.Equal("fake-jwt-token", result.token);
            Assert.Null(result.errorType);
            Assert.Equal("Login success", result.message);
        }

        //=========================================================
        // UTCID07 - Repository throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID07_LoginAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.LoginAsync("customer@petcenter.com", "Customer@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID08 - Customer valid, JWT service throws exception
        // Expected: Exception - "Service Temporarily Unavailable"
        //=========================================================
        [Fact]
        public async Task UTCID08_LoginAsync_JwtServiceThrowsException_ThrowException()
        {
            // Arrange
            var customer = BuildCustomer(rawPassword: "Customer@123", isVerified: true, isActive: true);
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(customer.Email!))
                .ReturnsAsync(customer);
            _jwtServiceMock
                .Setup(x => x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<string>()))
                .Throws(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.LoginAsync(customer.Email!, "Customer@123"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        //=========================================================
        // GetOrCreateUserFromGoogleAsync() — Google login (get or create)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Existing customer found with given email
        // Expected: Existing customer returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetOrCreateUserFromGoogleAsync_ExistingCustomerFound_ReturnExistingCustomer()
        {
            // Arrange
            var existingCustomer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = "existing@gmail.com",
                FullName = "Existing User",
                IsActive = true,
                IsVerified = true
            };

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(existingCustomer.Email!))
                .ReturnsAsync(existingCustomer);

            // Act
            var result = await _googleService.GetOrCreateUserFromGoogleAsync(
                existingCustomer.Email!, existingCustomer.FullName!);

            // Assert
            Assert.Equal(existingCustomer.CustomerId, result.CustomerId);
            Assert.Equal(existingCustomer.Email, result.Email);
            _customerRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
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
            // Arrange
            var email = "newuser@gmail.com";
            var name = "New User";
            string? capturedTempPassword = null;

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(email))
                .ReturnsAsync((Customer?)null);

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);

            _emailServiceMock
                .Setup(x => x.SendWelcomeEmailAsync(email, name, It.IsAny<string>()))
                .Callback<string, string, string>((_, __, tempPassword) =>
                    capturedTempPassword = tempPassword)
                .ReturnsAsync(true);

            // Act
            var result = await _googleService.GetOrCreateUserFromGoogleAsync(email, name);

            // Assert
            Assert.Equal(email, result.Email);
            Assert.Equal(name, result.FullName);
            Assert.True(result.IsActive);
            Assert.True(result.IsVerified);
            Assert.NotNull(result.PasswordHash);
            Assert.NotNull(capturedTempPassword);
            Assert.True(_passwordService.Verify(capturedTempPassword!, result.PasswordHash!));

            _customerRepositoryMock.Verify(
                x => x.AddAsync(It.Is<Customer>(c => c.Email == email)), Times.Once);
            _emailServiceMock.Verify(
                x => x.SendWelcomeEmailAsync(email, name, It.IsAny<string>()), Times.Once);
        }

        //=========================================================
        // UTCID03 - Repository throws exception on lookup
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID03_GetOrCreateUserFromGoogleAsync_RepositoryThrowsOnLookup_ThrowException()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _googleService.GetOrCreateUserFromGoogleAsync("newuser@gmail.com", "New User"));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
            _emailServiceMock.Verify(
                x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        //=========================================================
        // UTCID04 - Customer not found, Repository throws exception on AddAsync
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID04_GetOrCreateUserFromGoogleAsync_RepositoryThrowsOnAdd_ThrowException()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ThrowsAsync(new Exception("Simulated DB insert failure"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _googleService.GetOrCreateUserFromGoogleAsync("newuser@gmail.com", "New User"));

            // Assert
            Assert.Equal("Simulated DB insert failure", ex.Message);
            _emailServiceMock.Verify(
                x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        //=========================================================
        // UTCID05 - Customer not found, Customer added successfully,
        //           Email service throws exception
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID05_GetOrCreateUserFromGoogleAsync_EmailServiceThrowsException_ThrowException()
        {
            // Arrange
            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);

            _emailServiceMock
                .Setup(x => x.SendWelcomeEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _googleService.GetOrCreateUserFromGoogleAsync("newuser@gmail.com", "New User"));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);
            _customerRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
        }

        //=========================================================
        //=========================================================
        // ExchangeCodeForIdTokenAsync() — Google OAuth code exchange
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - HTTP success, response contains id_token
        // Expected: ID token returned successfully
        //=========================================================
        [Fact]
        public async Task UTCID01_ExchangeCodeForIdTokenAsync_SuccessWithIdToken_ReturnIdToken()
        {
            // Arrange
            var service = BuildGoogleServiceWithHttpResponse(
                HttpStatusCode.OK,
                "{\"id_token\":\"fake-id-token-value\"}");

            // Act
            var result = await service.ExchangeCodeForIdTokenAsync("auth-code", "https://redirect.uri");

            // Assert
            Assert.Equal("fake-id-token-value", result);
        }

        //=========================================================
        // UTCID02 - HTTP unsuccessful status
        // Expected: Exception - "Token exchange failed: ..."
        //=========================================================
        [Fact]
        public async Task UTCID02_ExchangeCodeForIdTokenAsync_HttpFailure_ThrowException()
        {
            // Arrange
            var service = BuildGoogleServiceWithHttpResponse(
                HttpStatusCode.BadRequest,
                "{\"error\":\"invalid_grant\"}");

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ExchangeCodeForIdTokenAsync("auth-code", "https://redirect.uri"));

            // Assert
            Assert.StartsWith("Token exchange failed:", ex.Message);
        }

        //=========================================================
        // UTCID03 - HTTP success but response missing id_token
        // Expected: Exception - "id_token not found in Google response"
        //=========================================================
        [Fact]
        public async Task UTCID03_ExchangeCodeForIdTokenAsync_MissingIdToken_ThrowException()
        {
            // Arrange
            var service = BuildGoogleServiceWithHttpResponse(
                HttpStatusCode.OK,
                "{\"access_token\":\"some-access-token\"}");

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ExchangeCodeForIdTokenAsync("auth-code", "https://redirect.uri"));

            // Assert
            Assert.Equal("id_token not found in Google response", ex.Message);
        }

        //=========================================================
        //=========================================================
        // RegisterRequestDTO Validation (DataAnnotations)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - All fields empty/null
        // Expected: Required error for FullName, Email, PhoneNumber,
        //           Password, Gender, BirthDay
        //=========================================================
        [Fact]
        public void UTCID01_RegisterRequestDTO_AllFieldsEmpty_ReturnRequiredErrors()
        {
            // Arrange
            var request = new RegisterRequestDTO
            {
                FullName = "",
                Email = "",
                PhoneNumber = "",
                Password = "",
                BirthDay = null,
                Gender = null
            };

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Full name is required", messages);
            Assert.Contains("Email is required", messages);
            Assert.Contains("Phone number is required", messages);
            Assert.Contains("Password is required", messages);
            Assert.Contains("Please select a gender", messages);
            Assert.Contains("Please select a birthday", messages);
        }

        //=========================================================
        // UTCID02 - FullName invalid format
        // Expected: "Full name must contain letters only and at least 2 characters"
        //=========================================================
        [Fact]
        public void UTCID02_RegisterRequestDTO_FullNameInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildRegisterRequest(fullName: "Nguyen123");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains(
                "Full name must contain letters only and at least 2 characters",
                messages);
        }

        //=========================================================
        // UTCID03 - FullName length > 50
        // Expected: "Full name must not exceed 50 characters"
        //=========================================================
        [Fact]
        public void UTCID03_RegisterRequestDTO_FullNameOver50Chars_ReturnLengthError()
        {
            // Arrange
            var longName = new string('A', 51); // 51 chữ cái, hợp lệ format nhưng vượt độ dài
            var request = BuildRegisterRequest(fullName: longName);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Full name must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID04 - FullName length = 50, but contains a digit (invalid format)
        // Expected: "Full name must contain letters only and at least 2 characters"
        // (Đúng 50 ký tự nên không vi phạm StringLength, nhưng có số nên vi phạm regex)
        //=========================================================
        [Fact]
        public void UTCID04_RegisterRequestDTO_FullNameExactly50CharsInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var name50WithDigit = new string('A', 49) + "1"; // đúng 50 ký tự, có số
            var request = BuildRegisterRequest(fullName: name50WithDigit);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains(
                "Full name must contain letters only and at least 2 characters",
                messages);
            Assert.DoesNotContain("Full name must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID05 - Email invalid format
        // Expected: "Invalid email format"
        //=========================================================
        [Fact]
        public void UTCID05_RegisterRequestDTO_EmailInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildRegisterRequest(email: "not-an-email");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Invalid email format", messages);
        }

        //=========================================================
        // UTCID06 - Email length > 50
        // Expected: "Email must not exceed 50 characters"
        //=========================================================
        [Fact]
        public void UTCID06_RegisterRequestDTO_EmailOver50Chars_ReturnLengthError()
        {
            // Arrange
            var longLocalPart = new string('a', 45); // 45 + "@a.com" = 51 ký tự
            var longEmail = longLocalPart + "@a.com";
            var request = BuildRegisterRequest(email: longEmail);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Email must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID07 - Email length = 50 (boundary hợp lệ)
        // Expected: KHÔNG có lỗi độ dài (đúng ranh giới StringLength(50))
        //=========================================================
        [Fact]
        public void UTCID07_RegisterRequestDTO_EmailExactly50Chars_ReturnNoLengthError()
        {
            // Arrange
            var localPart = new string('a', 44); // 44 + "@a.com" = 50 ký tự đúng
            var email50 = localPart + "@a.com";
            Assert.Equal(50, email50.Length); // xác nhận điều kiện biên trước khi test
            var request = BuildRegisterRequest(email: email50);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.DoesNotContain("Email must not exceed 50 characters", messages);
        }

        //=========================================================
        // UTCID09 - Phone invalid format
        // Expected: "Invalid Vietnamese phone number"
        //=========================================================
        [Fact]
        public void UTCID09_RegisterRequestDTO_PhoneInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildRegisterRequest(phoneNumber: "0123456789"); // đầu số không hợp lệ

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Invalid Vietnamese phone number", messages);
        }

        //=========================================================
        // UTCID010 - Phone length > 15
        // Expected: "Phone Number cannot exceed 15 characters"
        //=========================================================
        [Fact]
        public void UTCID010_RegisterRequestDTO_PhoneOver15Chars_ReturnLengthError()
        {
            // Arrange
            var longPhone = "09123456789012345"; // 18 ký tự, vượt 15
            var request = BuildRegisterRequest(phoneNumber: longPhone);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Phone Number cannot exceed 15 characters", messages);
        }

        //=========================================================
        // UTCID012 - Password length < 6
        // Expected: "Password must be at least 6 characters"
        //=========================================================
        [Fact]
        public void UTCID012_RegisterRequestDTO_PasswordUnder6Chars_ReturnLengthError()
        {
            // Arrange
            var request = BuildRegisterRequest(password: "Ab@1");

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Password must be at least 6 characters", messages);
        }

        //=========================================================
        // UTCID013 - Password invalid format (thiếu uppercase/@/số hoặc có khoảng trắng)
        // Expected: "Password must start with uppercase, contain @, a number, and no spaces"
        //=========================================================
        [Fact]
        public void UTCID013_RegisterRequestDTO_PasswordInvalidFormat_ReturnFormatError()
        {
            // Arrange
            var request = BuildRegisterRequest(password: "abcdef1"); // thiếu uppercase và @

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains(
                "Password must start with uppercase, contain @, a number, and no spaces",
                messages);
        }

        //=========================================================
        // UTCID014 - Age < 16
        // Expected: "You must be at least 16 years old to register"
        //=========================================================
        [Fact]
        public void UTCID014_RegisterRequestDTO_AgeUnder16_ReturnAgeError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
            var request = BuildRegisterRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("You must be at least 16 years old to register", messages);
        }

        //=========================================================
        // UTCID015 - Age > 100
        // Expected: "Date of birth cannot be more than 100 years ago"
        //=========================================================
        [Fact]
        public void UTCID015_RegisterRequestDTO_AgeOver100_ReturnAgeError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-101));
            var request = BuildRegisterRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Date of birth cannot be more than 100 years ago", messages);
        }

        //=========================================================
        // UTCID016 - Birthday is a future date
        // Expected: "Birthday cannot be in the future"
        //=========================================================
        [Fact]
        public void UTCID016_RegisterRequestDTO_FutureBirthday_ReturnFutureDateError()
        {
            // Arrange
            var birthDay = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var request = BuildRegisterRequest(birthDay: birthDay);

            // Act
            var errors = Validate(request);
            var messages = errors.Select(e => e.ErrorMessage).ToList();

            // Assert
            Assert.Contains("Birthday cannot be in the future", messages);
        }

        //=========================================================
        //=========================================================
        // RegisterAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID08 - Email already registered, verified customer exists
        // Expected: "Email is already registered. Please login."
        //=========================================================
        [Fact]
        public async Task UTCID08_RegisterAsync_EmailAlreadyRegisteredVerified_ReturnAlreadyRegistered()
        {
            // Arrange
            var request = BuildRegisterRequest(email: "existing@petcenter.com");
            var existingCustomer = BuildCustomer(email: request.Email, isVerified: true);

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync(existingCustomer);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email is already registered. Please login.", result.Message);
            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        // UTCID011 - Phone number already exists
        // Expected: "Phone number is already in use by another account."
        //=========================================================
        [Fact]
        public async Task UTCID011_RegisterAsync_PhoneAlreadyExists_ReturnPhoneInUse()
        {
            // Arrange
            var request = BuildRegisterRequest(phoneNumber: "0912345678");
            var existingPhoneCustomer = BuildCustomer(email: "other@petcenter.com");

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync(existingPhoneCustomer);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use by another account.", result.Message);
            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        //=========================================================
        // UTCID017 - Repository AddAsync throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID017_RegisterAsync_AddAsyncThrows_ThrowException()
        {
            // Arrange
            var request = BuildRegisterRequest();

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.RegisterAsync(request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID018 - Repository AddOtpAsync throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID018_RegisterAsync_AddOtpAsyncThrows_ThrowException()
        {
            // Arrange
            var request = BuildRegisterRequest();

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);
            _customerRepositoryMock
                .Setup(x => x.AddOtpAsync(It.IsAny<OtpCode>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.RegisterAsync(request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID019 - Send verification email throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID019_RegisterAsync_SendVerificationEmailThrows_ThrowException()
        {
            // Arrange
            var request = BuildRegisterRequest();

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);
            _customerRepositoryMock
                .Setup(x => x.AddOtpAsync(It.IsAny<OtpCode>()))
                .ReturnsAsync(true);
            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(request.Email, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.RegisterAsync(request));

            // Assert
            Assert.Equal("Email service unavailable", ex.Message);
        }

        //=========================================================
        // UTCID020 - Unverified customer exists with existing OTP,
        //            full success path
        // Expected: Registration completed successfully
        //=========================================================
        [Fact]
        public async Task UTCID020_RegisterAsync_UnverifiedCustomerWithExistingOtp_ReturnSuccess()
        {
            // Arrange
            var request = BuildRegisterRequest(email: "unverified@petcenter.com");
            var existingCustomer = BuildCustomer(email: request.Email, isVerified: false);
            var existingOtp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = existingCustomer.CustomerId,
                VerificationCode = "111111",
                VerificationExpire = DateTime.UtcNow.AddMinutes(5),
                LastOtpSentAt = DateTime.UtcNow,
                OtpAttemptCount = 0
            };

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsyncWithoutActiveCheck(request.Email))
                .ReturnsAsync(existingCustomer);
            _customerRepositoryMock
                .Setup(x => x.GetOtpByCustomerIdAsync(existingCustomer.CustomerId))
                .ReturnsAsync(existingOtp);
            _customerRepositoryMock
                .Setup(x => x.DeleteOtpAsync(existingOtp))
                .ReturnsAsync(true);
            _customerRepositoryMock
                .Setup(x => x.DeleteAsync(existingCustomer))
                .ReturnsAsync(true);
            _customerRepositoryMock
                .Setup(x => x.GetByPhoneAsync(request.PhoneNumber))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(true);
            _customerRepositoryMock
                .Setup(x => x.AddOtpAsync(It.IsAny<OtpCode>()))
                .ReturnsAsync(true);
            _emailServiceMock
                .Setup(x => x.SendVerificationEmail(request.Email, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(
                "Verification code sent to your email. Please verify within 5 minutes.",
                result.Message);
            _customerRepositoryMock.Verify(x => x.DeleteOtpAsync(existingOtp), Times.Once);
            _customerRepositoryMock.Verify(x => x.DeleteAsync(existingCustomer), Times.Once);
            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
            _customerRepositoryMock.Verify(x => x.AddOtpAsync(It.IsAny<OtpCode>()), Times.Once);
        }
    }
}
