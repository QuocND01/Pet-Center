using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;

namespace PetCenterTestProject.CustomerTest
{
    [Collection("DatabaseTests")]
    public class CustomerProfileTest_DB
    {
        //=========================================================
        // Mapper
        //=========================================================
        private readonly IMapper _mapper;

        //=========================================================
        // Constructor
        //=========================================================
        public CustomerProfileTest_DB()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CustomerMappingProfile>();
            }, NullLoggerFactory.Instance)
            .CreateMapper();
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
        private CustomerService CreateService(PetCenterContext context)
        {
            return new CustomerService(
                CreateRepository(context),
                _mapper);
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
        // Helper: Build a Customer entity
        //=========================================================
        private Customer BuildCustomer(
            string email,
            string fullName,
            string phoneNumber,
            bool? isVerified,
            bool? isActive)
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                Gender = "Male",
                IsVerified = isVerified,
                IsActive = isActive,
                CreatedAt = DateTime.Now
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
        // GetProfileAsync()
        // UTCID01 - Customer exists
        // Expected: Entity mapped to CustomerProfileResponseDTO, DTO returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetProfileAsync_CustomerExists_ReturnProfileDto()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "customer@petcenter.com",
                "Nguyen Van A",
                "0912345678",
                isVerified: true,
                isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetProfileAsync(customer.CustomerId);

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.GetProfileAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        //=========================================================
        //=========================================================
        // UpdateProfileAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID02 - Customer not found
        // Expected: (false, "Customer not found")
        //=========================================================
        [Fact]
        public async Task UTCID02_UpdateProfileAsync_CustomerNotFound_ReturnCustomerNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildUpdateRequest();

            // Act
            var result = await service.UpdateProfileAsync(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Customer not found", result.Message);
        }

        //=========================================================
        // UTCID03 - Account deactivated
        // Expected: (false, "Account is deactivated")
        //=========================================================
        [Fact]
        public async Task UTCID03_UpdateProfileAsync_AccountDeactivated_ReturnAccountDeactivated()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "deactivated@petcenter.com", "Nguyen Van A", "0912345678",
                isVerified: true, isActive: false);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = BuildUpdateRequest(phoneNumber: customer.PhoneNumber!);

            // Act
            var result = await service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Account is deactivated", result.Message);

            var unchanged = await context.Customers
                .AsNoTracking()
                .FirstAsync(c => c.CustomerId == customer.CustomerId);
            Assert.Equal(customer.FullName, unchanged.FullName);
        }

        //=========================================================
        // UTCID11 - Phone unchanged, update thành công
        // Expected: (true, "Profile updated successfully")
        //=========================================================
        [Fact]
        public async Task UTCID11_UpdateProfileAsync_PhoneUnchanged_ReturnSuccess()
        {
            Guid customerId;
            string originalPhone = "0912345678";

            using (var arrangeContext = CreateContext())
            {
                await ClearDatabaseAsync(arrangeContext);

                var customer = BuildCustomer(
                    "active@petcenter.com", "Nguyen Van A", originalPhone,
                    isVerified: true, isActive: true);

                arrangeContext.Customers.Add(customer);
                await arrangeContext.SaveChangesAsync();
                customerId = customer.CustomerId;
            }

            // Act — context mới, mô phỏng 1 request thật sự
            using var actContext = CreateContext();
            var service = CreateService(actContext);
            var request = BuildUpdateRequest(
                fullName: "Nguyen Van Updated",
                phoneNumber: originalPhone);

            var result = await service.UpdateProfileAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);

            var updated = await actContext.Customers
                .AsNoTracking()
                .FirstAsync(c => c.CustomerId == customerId);
            Assert.Equal("Nguyen Van Updated", updated.FullName);
            Assert.Equal(originalPhone, updated.PhoneNumber);
        }

        //=========================================================
        // UTCID12 - Phone changed, không trùng, update thành công
        // Expected: (true, "Profile updated successfully")
        //=========================================================
        [Fact]
        public async Task UTCID12_UpdateProfileAsync_PhoneChangedNoDuplicate_ReturnSuccess()
        {
            Guid customerId;

            // Arrange
            using (var arrangeContext = CreateContext())
            {
                await ClearDatabaseAsync(arrangeContext);

                var customer = BuildCustomer(
                    "active2@petcenter.com", "Nguyen Van A", "0912345678",
                    isVerified: true, isActive: true);

                arrangeContext.Customers.Add(customer);
                await arrangeContext.SaveChangesAsync();
                customerId = customer.CustomerId;
            }

            // Act
            using var actContext = CreateContext();
            var service = CreateService(actContext);
            var request = BuildUpdateRequest(phoneNumber: "0987654321");

            var result = await service.UpdateProfileAsync(customerId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);

            var updated = await actContext.Customers
                .AsNoTracking()
                .FirstAsync(c => c.CustomerId == customerId);
            Assert.Equal("0987654321", updated.PhoneNumber);
        }

        //=========================================================
        // UTCID13 - Phone already exists (duplicate, verified customer khác)
        // Expected: (false, "Phone number is already in use by another account.")
        //=========================================================
        [Fact]
        public async Task UTCID13_UpdateProfileAsync_PhoneAlreadyExists_ReturnPhoneInUse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "active3@petcenter.com", "Nguyen Van A", "0912345678",
                isVerified: true, isActive: true);
            var otherCustomer = BuildCustomer(
                "other@petcenter.com", "Other Customer", "0987654321",
                isVerified: true, isActive: true);

            context.Customers.AddRange(customer, otherCustomer);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                fullName: "Should Not Apply",
                phoneNumber: otherCustomer.PhoneNumber!);

            // Act
            var result = await service.UpdateProfileAsync(customer.CustomerId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Phone number is already in use by another account.", result.Message);

            var unchanged = await context.Customers
                .AsNoTracking()
                .FirstAsync(c => c.CustomerId == customer.CustomerId);
            Assert.Equal("0912345678", unchanged.PhoneNumber);
            Assert.Equal("Nguyen Van A", unchanged.FullName);
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerA = BuildCustomer(
                "a@petcenter.com", "Nguyen Van A", "0912345678",
                isVerified: true, isActive: true);
            var customerB = BuildCustomer(
                "b@petcenter.com", "Nguyen Van B", "0987654321",
                isVerified: true, isActive: true);

            context.Customers.AddRange(customerA, customerB);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.CustomerId == customerA.CustomerId && c.FullName == "Nguyen Van A");
            Assert.Contains(result, c => c.CustomerId == customerB.CustomerId && c.FullName == "Nguyen Van B");
        }

        //=========================================================
        // UTCID02 - Customer list is empty
        // Expected: Empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllCustomersAsync_CustomerListEmpty_ReturnEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer(
                "detail@petcenter.com", "Nguyen Van A", "0912345678",
                isVerified: true, isActive: true);

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetCustomerByIdAsync(customer.CustomerId);

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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.GetCustomerByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
    }
}
