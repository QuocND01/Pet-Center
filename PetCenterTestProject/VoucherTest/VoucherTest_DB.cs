using PetCenterAPI.Service.Interface;
using PetCenterAPI.DTOs.Responses.Order;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.Hubs;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Requests.ManageVoucher;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;

namespace PetCenterTestProject.VoucherTest
{
    [Collection("DatabaseTests")]
    public class VoucherTest_DB
    {
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
        // Create Repository / Service
        //=========================================================
        private VoucherRepository CreateRepository(PetCenterContext context)
        {
            return new VoucherRepository(context);
        }

        private VoucherService CreateService(PetCenterContext context)
        {
            return new VoucherService(CreateRepository(context));
        }

        //=========================================================
        // Clear Database
        //=========================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.ChangeTracker.Clear();

            context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
            context.MedicalRecords.RemoveRange(context.MedicalRecords);
            context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
            context.AppointmentServices.RemoveRange(context.AppointmentServices);
            context.Appointments.RemoveRange(context.Appointments);
            context.Pets.RemoveRange(context.Pets);
            context.CartDetails.RemoveRange(context.CartDetails);
            context.Carts.RemoveRange(context.Carts);
            context.OtpCodes.RemoveRange(context.OtpCodes);
            context.FeedbackImages.RemoveRange(context.FeedbackImages);
            context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
            context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Payments.RemoveRange(context.Payments);
            context.Orders.RemoveRange(context.Orders);
            context.Addresses.RemoveRange(context.Addresses);
            context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
            context.Vouchers.RemoveRange(context.Vouchers);
            context.Customers.RemoveRange(context.Customers);

            await context.SaveChangesAsync();
        }

        //=========================================================
        // Helper: Build a Voucher entity, override fields as needed
        //=========================================================
        private Voucher BuildVoucher(
            string code,
            int? discountPercent = 10,
            bool? isActive = true,
            DateTime? expiredDate = null,
            decimal? minOrderAmount = 100000,
            decimal? maxDiscountAmount = 50000,
            int? useageLimit = 100,
            string? description = "10% off")
        {
            return new Voucher
            {
                VoucherId = Guid.NewGuid(),
                Code = code,
                DiscountPercent = discountPercent,
                IsActive = isActive,
                ExpiredDate = expiredDate ?? DateTime.Now.AddDays(30),
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                UseageLimit = useageLimit,
                CreateAt = DateTime.Now,
                Description = description
            };
        }

        //=========================================================
        // Helper: Build a valid CreateVoucherRequestDTO, override fields as needed
        //=========================================================
        private CreateVoucherRequestDTO BuildCreateVoucherRequest(
            string code = "SALE10",
            int discountPercent = 10,
            string? description = "10% off",
            decimal minOrderAmount = 100000,
            decimal maxDiscountAmount = 50000,
            int? useageLimit = 100,
            DateTime? expiredDate = null)
        {
            return new CreateVoucherRequestDTO
            {
                Code = code,
                DiscountPercent = discountPercent,
                Description = description,
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                UseageLimit = useageLimit,
                ExpiredDate = expiredDate ?? DateTime.Now.AddDays(30)
            };
        }

        //=========================================================
        // Helper: Build a Customer (dùng để tạo CustomerVoucher usage)
        //=========================================================
        private Customer BuildCustomer(string email)
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                FullName = "Nguyen Van A",
                PhoneNumber = "0912345678",
                BirthDay = new DateOnly(1995, 1, 1),
                Gender = "Male",
                Email = email,
                CreatedAt = DateTime.Now,
                IsVerified = true,
                IsActive = true
            };
        }

        //=========================================================
        //=========================================================
        // GetAllAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Single voucher, UsedCount = 0, IsActive = true, Not Expired
        // Expected: DTO list returned with mapped UsedCount = 0
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllAsync_SingleVoucherUsedCountZeroActive_ReturnMappedList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var voucher = BuildVoucher("SALE10", isActive: true, expiredDate: DateTime.Now.AddDays(10));
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(voucher.VoucherId, result[0].VoucherId);
            Assert.Equal(0, result[0].UsedCount);
            Assert.True(result[0].IsActive);
        }

        //=========================================================
        // UTCID02 - Single voucher, UsedCount > 0, IsActive = false, Not Expired
        // Expected: DTO list returned with mapped UsedCount
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllAsync_SingleVoucherUsedCountPositiveInactive_ReturnMappedList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer("voucher2@petcenter.com");
            var voucher = BuildVoucher("SALE20", isActive: false, expiredDate: DateTime.Now.AddDays(10));

            context.Customers.Add(customer);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            context.CustomerVouchers.Add(new CustomerVoucher
            {
                CustomerId = customer.CustomerId,
                VoucherId = voucher.VoucherId,
                IsUsed = true
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].UsedCount);
            Assert.False(result[0].IsActive);
        }

        //=========================================================
        // UTCID03 - Single voucher, UsedCount > 0, Voucher Expired
        // Expected: DTO list returned with mapped UsedCount, ExpiredDate preserved
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAllAsync_SingleVoucherExpired_ReturnMappedList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer("voucher3@petcenter.com");
            var expiredDate = DateTime.Now.AddDays(-5);
            var voucher = BuildVoucher("SALE30", expiredDate: expiredDate);

            context.Customers.Add(customer);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            context.CustomerVouchers.Add(new CustomerVoucher
            {
                CustomerId = customer.CustomerId,
                VoucherId = voucher.VoucherId,
                IsUsed = true
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].UsedCount);
        }

        //=========================================================
        // UTCID04 - Multiple vouchers returned, UsedCount > 0 each, Not Expired
        // Expected: DTO list returned, mỗi voucher lấy đúng UsedCount riêng
        //=========================================================
        [Fact]
        public async Task UTCID04_GetAllAsync_MultipleVouchers_ReturnMappedListWithCorrectUsedCountEach()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerA = BuildCustomer("voucher4a@petcenter.com");
            var customerB = BuildCustomer("voucher4b@petcenter.com");
            var voucherA = BuildVoucher("SALE40A", expiredDate: DateTime.Now.AddDays(10));
            var voucherB = BuildVoucher("SALE40B", expiredDate: DateTime.Now.AddDays(20));

            context.Customers.AddRange(customerA, customerB);
            context.Vouchers.AddRange(voucherA, voucherB);
            await context.SaveChangesAsync();

            context.CustomerVouchers.AddRange(
                new CustomerVoucher { CustomerId = customerA.CustomerId, VoucherId = voucherA.VoucherId, IsUsed = true },
                new CustomerVoucher { CustomerId = customerB.CustomerId, VoucherId = voucherA.VoucherId, IsUsed = true },
                new CustomerVoucher { CustomerId = customerA.CustomerId, VoucherId = voucherB.VoucherId, IsUsed = true }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            var mappedA = result.First(r => r.VoucherId == voucherA.VoucherId);
            var mappedB = result.First(r => r.VoucherId == voucherB.VoucherId);
            Assert.Equal(2, mappedA.UsedCount); // 2 customer dùng voucherA
            Assert.Equal(1, mappedB.UsedCount); // 1 customer dùng voucherB
        }

        //=========================================================
        // UTCID05 - Single voucher, UsedCount > 0, Not Expired
        // Expected: DTO list returned with mapped UsedCount
        //=========================================================
        [Fact]
        public async Task UTCID05_GetAllAsync_SingleVoucherUsedCountPositiveNotExpired_ReturnMappedList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer("voucher5@petcenter.com");
            var voucher = BuildVoucher("SALE50", expiredDate: DateTime.Now.AddDays(15));

            context.Customers.Add(customer);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            context.CustomerVouchers.Add(new CustomerVoucher
            {
                CustomerId = customer.CustomerId,
                VoucherId = voucher.VoucherId,
                IsUsed = true
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].UsedCount);
        }

        //=========================================================
        // UTCID07 - Repository returns empty list
        // Expected: Empty DTO list returned
        //=========================================================
        [Fact]
        public async Task UTCID07_GetAllAsync_NoVouchersInDb_ReturnEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Empty(result);
        }

        //=========================================================
        //=========================================================
        // ToggleStatusAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Voucher not found
        // Expected: (false, "Voucher not found.")
        //=========================================================
        [Fact]
        public async Task UTCID01_ToggleStatusAsync_VoucherNotFound_ReturnNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(Guid.NewGuid(), true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Voucher not found.", result.Message);
        }

        //=========================================================
        // UTCID02 - Voucher exists, Activate, Expired
        // Expected: (false, "Cannot activate an expired voucher. Please update the expiry date first.")
        //=========================================================
        [Fact]
        public async Task UTCID02_ToggleStatusAsync_ActivateExpiredVoucher_ReturnCannotActivateExpired()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var voucher = BuildVoucher("EXPIRED01", isActive: false, expiredDate: DateTime.Now.AddDays(-1));
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "Cannot activate an expired voucher. Please update the expiry date first.",
                result.Message);

            var unchanged = await context.Vouchers
                .AsNoTracking()
                .FirstAsync(v => v.VoucherId == voucher.VoucherId);
            Assert.False(unchanged.IsActive);
        }

        //=========================================================
        // UTCID03 - Voucher exists, Activate, Not Expired (Future),
        //           Has UsageLimit, Reached usage limit
        // Expected: (false, "Cannot activate voucher that has reached its usage limit.")
        //=========================================================
        [Fact]
        public async Task UTCID03_ToggleStatusAsync_ActivateUsageLimitReached_ReturnCannotActivateLimitReached()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerA = BuildCustomer("toggle3a@petcenter.com");
            var customerB = BuildCustomer("toggle3b@petcenter.com");
            var voucher = BuildVoucher(
                "LIMIT03", isActive: false,
                expiredDate: DateTime.Now.AddDays(10),
                useageLimit: 2);

            context.Customers.AddRange(customerA, customerB);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            context.CustomerVouchers.AddRange(
                new CustomerVoucher { CustomerId = customerA.CustomerId, VoucherId = voucher.VoucherId, IsUsed = true },
                new CustomerVoucher { CustomerId = customerB.CustomerId, VoucherId = voucher.VoucherId, IsUsed = true }
            );
            await context.SaveChangesAsync(); // UsedCount = 2, đạt đúng giới hạn = 2

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "Cannot activate voucher that has reached its usage limit.",
                result.Message);

            var unchanged = await context.Vouchers
                .AsNoTracking()
                .FirstAsync(v => v.VoucherId == voucher.VoucherId);
            Assert.False(unchanged.IsActive);
        }

        //=========================================================
        // UTCID04 - Voucher exists, Activate, Future expiry,
        //           Has UsageLimit, Below usage limit, Update succeeds
        // Expected: (true, "Voucher activated.")
        //=========================================================
        [Fact]
        public async Task UTCID04_ToggleStatusAsync_ActivateBelowUsageLimit_ReturnActivatedSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = BuildCustomer("toggle4@petcenter.com");
            var voucher = BuildVoucher(
                "LIMIT04", isActive: false,
                expiredDate: DateTime.Now.AddDays(10),
                useageLimit: 5);

            context.Customers.Add(customer);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            context.CustomerVouchers.Add(
                new CustomerVoucher { CustomerId = customer.CustomerId, VoucherId = voucher.VoucherId, IsUsed = true }
            );
            await context.SaveChangesAsync(); // UsedCount = 1, dưới giới hạn 5

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher activated.", result.Message);

            var updated = await context.Vouchers
                .AsNoTracking()
                .FirstAsync(v => v.VoucherId == voucher.VoucherId);
            Assert.True(updated.IsActive);
        }

        //=========================================================
        // UTCID05 - Voucher exists, Activate, Null expiry, No UsageLimit
        // Expected: (true, "Voucher activated.")
        //=========================================================
        [Fact]
        public async Task UTCID05_ToggleStatusAsync_ActivateNoExpiryNoUsageLimit_ReturnActivatedSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var voucher = BuildVoucher(
                "NOEXP05", isActive: false,
                expiredDate: null,
                useageLimit: null);

            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(voucher.VoucherId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher activated.", result.Message);

            var updated = await context.Vouchers
                .AsNoTracking()
                .FirstAsync(v => v.VoucherId == voucher.VoucherId);
            Assert.True(updated.IsActive);
        }

        //=========================================================
        // UTCID06 - Voucher exists, Deactivate, Update succeeds
        // Expected: (true, "Voucher deactivated.")
        //=========================================================
        [Fact]
        public async Task UTCID06_ToggleStatusAsync_Deactivate_ReturnDeactivatedSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var voucher = BuildVoucher("DEACT06", isActive: true);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ToggleStatusAsync(voucher.VoucherId, false);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher deactivated.", result.Message);

            var updated = await context.Vouchers
                .AsNoTracking()
                .FirstAsync(v => v.VoucherId == voucher.VoucherId);
            Assert.False(updated.IsActive);
        }

        //=========================================================
        //=========================================================
        // CreateAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - Code length = 20 (boundary hợp lệ), CodeExists = false,
        //           CreateAsync succeeds, ExpiredDate in future
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID04_CreateAsync_CodeExactly20Chars_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var code20 = new string('A', 20);
            Assert.Equal(20, code20.Length);
            var request = BuildCreateVoucherRequest(code: code20);

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(code20, result.Data!.Code);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == code20);
            Assert.NotNull(saved);
        }

        //=========================================================
        // UTCID07 - DiscountPercent = 1 (boundary hợp lệ)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID07_CreateAsync_DiscountPercentExactly1_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(code: "SALE07", discountPercent: 1);
            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE07");
            Assert.NotNull(saved);
            Assert.Equal(1, saved!.DiscountPercent);
        }

        //=========================================================
        // UTCID08 - DiscountPercent = 80, EffectiveRate = 80% (boundary hợp lệ)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID08_CreateAsync_DiscountPercent80AndEffectiveRate80_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE08",
                discountPercent: 80,
                minOrderAmount: 100000,
                maxDiscountAmount: 80000); // 80%

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID09 - EffectiveRate (Max/Min ratio) > 80%
        // Expected: (false, "Effective discount rate (90.0%) exceeds 80%. ...")
        //=========================================================
        [Fact]
        public async Task UTCID09_CreateAsync_EffectiveRateOver80Percent_ReturnEffectiveRateError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE09",
                discountPercent: 50,
                minOrderAmount: 100000,
                maxDiscountAmount: 90000); // 90%

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Effective discount rate (90.0%) exceeds 80%.", result.Message);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE09");
            Assert.Null(saved);
        }

        //=========================================================
        // UTCID10 - Trường hợp hợp lệ tổng quát (happy path)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID10_CreateAsync_ValidRequest_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(code: "SALE10GEN");
            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
            Assert.NotNull(result.Data);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE10GEN");
            Assert.NotNull(saved);
            Assert.True(saved!.IsActive);
        }

        //=========================================================
        // UTCID11 - CodeExists = true (voucher với code đó đã tồn tại)
        // Expected: (false, "Voucher code 'XXX' already exists.")
        //=========================================================
        [Fact]
        public async Task UTCID11_CreateAsync_CodeAlreadyExists_ReturnCodeExistsError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var existingVoucher = BuildVoucher("DUPLICATE11");
            context.Vouchers.Add(existingVoucher);
            await context.SaveChangesAsync();

            var request = BuildCreateVoucherRequest(code: "DUPLICATE11");
            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Voucher code 'DUPLICATE11' already exists.", result.Message);

            var count = await context.Vouchers.CountAsync(v => v.Code == "DUPLICATE11");
            Assert.Equal(1, count); // không tạo thêm bản trùng
        }

        //=========================================================
        // UTCID13 - ExpiredDate in past
        // Expected: (false, "Expiry date must be in the future.")
        //=========================================================
        [Fact]
        public async Task UTCID13_CreateAsync_ExpiredDateInPast_ReturnExpiryError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE13",
                expiredDate: DateTime.Now.AddDays(-1));

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Expiry date must be in the future.", result.Message);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE13");
            Assert.Null(saved);
        }

        //=========================================================
        // UTCID15 - Description length = 100 (boundary hợp lệ)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID15_CreateAsync_DescriptionExactly100Chars_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var description100 = new string('A', 100);
            var request = BuildCreateVoucherRequest(code: "SALE15", description: description100);

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID19 - MaxDiscountAmount = 1 (boundary hợp lệ)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID19_CreateAsync_MaxDiscountAmountExactly1_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE19",
                minOrderAmount: 100000,
                maxDiscountAmount: 1);

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID20 - MaxDiscountAmount = 50,000,000 (boundary hợp lệ)
        // Expected: (true, "Voucher created successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID20_CreateAsync_MaxDiscountAmountExactly50Million_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE20",
                minOrderAmount: 100_000_000,
                maxDiscountAmount: 50_000_000);

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Voucher created successfully.", result.Message);
        }

        //=========================================================
        // UTCID22 - UsageLimit < 1
        // Expected: (false, "Usage limit must be between 1 and 500.")
        //=========================================================
        [Fact]
        public async Task UTCID22_CreateAsync_UsageLimitBelow1_ReturnUsageLimitError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(code: "SALE22", useageLimit: 0);
            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Usage limit must be between 1 and 500.", result.Message);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE22");
            Assert.Null(saved);
        }

        //=========================================================
        // UTCID23 - MaxDiscountAmount >= MinOrderAmount
        // Expected: (false, "Max discount amount must be less than min order amount.")
        //=========================================================
        [Fact]
        public async Task UTCID23_CreateAsync_MaxDiscountGreaterOrEqualMinOrder_ReturnMaxDiscountError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var request = BuildCreateVoucherRequest(
                code: "SALE23",
                minOrderAmount: 100000,
                maxDiscountAmount: 100000);

            var service = CreateService(context);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Max discount amount must be less than min order amount.", result.Message);

            var saved = await context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == "SALE23");
            Assert.Null(saved);
        }
    

        //=====================================================================
        // Helper: Create CheckoutService
        //=====================================================================
        private CheckoutService CreateCheckoutService(PetCenterContext context)
        {
            var hubMock = new Mock<IHubContext<AppHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubMock.Setup(x => x.Clients).Returns(clientsMock.Object);

            var vnPayMock = new Mock<IVnPayService>();
            var moMoMock = new Mock<IMoMoService>();

            return new CheckoutService(context, hubMock.Object, vnPayMock.Object, moMoMock.Object, NullLogger<CheckoutService>.Instance);
        }

        private async Task<Customer> EnsureCustomerAsync(PetCenterContext context, Guid customerId)
        {
            var cus = await context.Customers.FindAsync(customerId);
            if (cus == null)
            {
                cus = new Customer
                {
                    CustomerId = customerId,
                    FullName = "Test Customer",
                    Email = $"voucher_{Guid.NewGuid():N}@gmail.com",
                    PhoneNumber = "0988888888",
                    Gender = "Male",
                    IsVerified = true,
                    IsActive = true
                };
                context.Customers.Add(cus);
                await context.SaveChangesAsync();
            }
            return cus;
        }

        //=====================================================================
        // GetAvailableVouchersAsync() DB Tests (UTCID01 to UTCID07)
        //=====================================================================

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID01_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("SALE10", discountPercent: 10, isActive: true, expiredDate: DateTime.Now.AddDays(5), minOrderAmount: 100000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Single(result);
            Assert.Equal("SALE10", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID02_Success_ExpiredDateNull_MinOrderAmountNull_UsageLimitNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("SALENULL", discountPercent: 10, isActive: true, expiredDate: null, minOrderAmount: null, useageLimit: null);
            voucher.ExpiredDate = null;
            voucher.MinOrderAmount = null;
            voucher.UseageLimit = null;
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 50000);

            Assert.Single(result);
            Assert.Equal("SALENULL", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID03_Success_ExpiredDateEqualNow()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("SALEBORDER", discountPercent: 10, isActive: true, expiredDate: DateTime.Now.AddHours(1), minOrderAmount: 100000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Single(result);
            Assert.Equal("SALEBORDER", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID04_Success_MinOrderAmountEqualOrderAmount()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("SALEEXACT", discountPercent: 10, isActive: true, expiredDate: DateTime.Now.AddDays(5), minOrderAmount: 100000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 100000);

            Assert.Single(result);
            Assert.Equal("SALEEXACT", result[0].Code);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID05_ExcludesVoucher_WhenInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("INACTIVE", discountPercent: 10, isActive: false, expiredDate: DateTime.Now.AddDays(5), minOrderAmount: 100000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID06_ExcludesVoucher_WhenExpired()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("EXPIRED", discountPercent: 10, isActive: true, expiredDate: DateTime.Now.AddDays(-1), minOrderAmount: 100000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableVouchersAsync_UTCID07_ExcludesVoucher_WhenMinOrderAmountGreaterThanOrderAmount()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = await EnsureCustomerAsync(context, Guid.NewGuid());
            var voucher = BuildVoucher("HIGHMIN", discountPercent: 10, isActive: true, expiredDate: DateTime.Now.AddDays(5), minOrderAmount: 200000);
            context.Vouchers.Add(voucher);
            await context.SaveChangesAsync();

            var service = CreateCheckoutService(context);
            var result = await service.GetAvailableVouchersAsync(customer.CustomerId, 150000);

            Assert.Empty(result);
        }

}
}
