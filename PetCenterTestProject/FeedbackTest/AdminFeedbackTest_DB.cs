using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;
using PetCenterTestProject;

namespace PetCenterTestProject.FeedbackTest
{
    [Collection("DatabaseTests")]
    public class AdminFeedbackTest_DB
    {
        //=========================================================
        // Create SQL Server Context
        //=========================================================
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;")
                .Options;

            return new PetCenterContext(options);
        }

        //=========================================================
        // Create Repository
        //=========================================================
        private AdminFeedbackRepository CreateRepository(PetCenterContext context)
        {
            return new AdminFeedbackRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================
        private AdminFeedbackService CreateService(PetCenterContext context)
        {
            return new AdminFeedbackService(CreateRepository(context));
        }

        //=========================================================
        // Clear Database
        // Xóa theo thứ tự con -> cha để tránh vi phạm FK constraint.
        //=========================================================
        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await TestDatabaseCleaner.ClearAllAsync(context);
        }

        //=========================================================
        // Helper: Build a Customer
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
                PasswordHash = "hashed-password",
                IsActive = true
            };
        }

        //=========================================================
        // Helper: Build a Brand
        //=========================================================
        private Brand BuildBrand(string brandName)
        {
            return new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = brandName,
                Status = Status.Active
            };
        }

        //=========================================================
        // Helper: Build a Category
        //=========================================================
        private Category BuildCategory(string categoryName)
        {
            return new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = categoryName,
                Status = Status.Active
            };
        }

        //=========================================================
        // Helper: Build a Product
        //=========================================================
        private Product BuildProduct(string productName, Guid brandId, Guid categoryId)
        {
            return new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = productName,
                ProductPrice = 100000,
                BrandId = brandId,
                CategoryId = categoryId,
                AddedAt = DateTime.Now,
                Status = Status.Active
            };
        }

        //=========================================================
        // Helper: Build an Address
        //=========================================================
        private Address BuildAddress(Guid customerId)
        {
            return new Address
            {
                AddressId = Guid.NewGuid(),
                CustomerId = customerId,
                Province = "Can Tho",
                District = "Ninh Kieu",
                Ward = "An Khanh",
                AddressDetails = "123 Nguyen Van Cu",
                IsDefault = true,
                IsActive = true
            };
        }

        //=========================================================
        // Helper: Build an Order
        //=========================================================
        private Order BuildOrder(Guid customerId, Guid addressId)
        {
            return new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = customerId,
                AddressId = addressId,
                AddressSnapshot = "123 Nguyen Van Cu, An Khanh, Ninh Kieu, Can Tho",
                OrderDate = DateTime.Now,
                TotalAmount = 100000,
                Status = 1,
                PaymentMethod = "COD",
                PaymentStatus = 1
            };
        }

        //=========================================================
        // Helper: Build a ProductFeedback
        //=========================================================
        private ProductFeedback BuildFeedback(
            Guid customerId,
            Guid productId,
            Guid orderId,
            int? rating = 5,
            string? comment = "Good product",
            string? reply = null,
            DateTime? createdAt = null)
        {
            return new ProductFeedback
            {
                FeedbackId = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = productId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment,
                Reply = reply,
                ReplyDate = reply != null ? DateTime.Now : null,
                CreatedAt = createdAt ?? DateTime.Now,
                Status = 1
            };
        }

        //=========================================================
        // Helper: Seed a full valid Customer + Brand + Category + Product
        //          + Address + Order chain, returns their IDs
        //=========================================================
        private async Task<(Guid CustomerId, Guid ProductId, Guid OrderId)> SeedBaseDataAsync(
            PetCenterContext context, string emailSuffix)
        {
            var customer = BuildCustomer($"customer{emailSuffix}@petcenter.com");
            var brand = BuildBrand($"Brand{emailSuffix}");
            var category = BuildCategory($"Category{emailSuffix}");

            context.Customers.Add(customer);
            context.Brands.Add(brand);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = BuildProduct($"Product{emailSuffix}", brand.BrandId, category.CategoryId);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var address = BuildAddress(customer.CustomerId);
            context.Addresses.Add(address);
            await context.SaveChangesAsync();

            var order = BuildOrder(customer.CustomerId, address.AddressId);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return (customer.CustomerId, product.ProductId, order.OrderId);
        }

        //=========================================================
        // Helper: Build a Staff
        //=========================================================
        private Staff BuildStaff(string email)
        {
            return new Staff
            {
                StaffId = Guid.NewGuid(),
                FullName = "Staff User",
                Email = email,
                PhoneNumber = "0987654321",
                PasswordHash = "hashed-password",
                Gender = "Male",
                BirthDate = new DateTime(1995, 1, 1),
                HireDate = DateTime.Now,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }

        //=========================================================
        // GetAllAsync()
        // UTCID03 (Repository throws exception) bỏ qua ở file DB:
        // không giả lập được lỗi repository qua DB thật hoạt động
        // bình thường. Case này chỉ có ở file Mock.
        //=========================================================

        //=========================================================
        // UTCID01 - Repository returns feedback list
        // Expected: Start index + RowNumber calculated correctly,
        //           Success = true, paged list returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllAsync_RepositoryReturnsFeedbackList_ReturnPagedListWithCorrectRowNumber()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "1");

            // Seed 3 feedback với CreatedAt khác nhau để kiểm tra sort mặc định (mới nhất trước)
            var feedback1 = BuildFeedback(customerId, productId, orderId,
                rating: 5, comment: "Great", createdAt: DateTime.Now.AddMinutes(-3));
            var feedback2 = BuildFeedback(customerId, productId, orderId,
                rating: 4, comment: "Good", createdAt: DateTime.Now.AddMinutes(-2));
            var feedback3 = BuildFeedback(customerId, productId, orderId,
                rating: 3, comment: "OK", createdAt: DateTime.Now.AddMinutes(-1));

            context.ProductFeedbacks.AddRange(feedback1, feedback2, feedback3);
            await context.SaveChangesAsync();

            var filter = new FeedbackFilterRequestDTO { Page = 1, PageSize = 10 };
            var service = CreateService(context);

            // Act
            var result = await service.GetAllAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data!.TotalCount);
            Assert.Equal(3, result.Data.Items.Count);

            // Start index = (1-1)*10 + 1 = 1
            Assert.Equal(1, result.Data.Items[0].RowNumber);
            Assert.Equal(2, result.Data.Items[1].RowNumber);
            Assert.Equal(3, result.Data.Items[2].RowNumber);

            // Mặc định sort theo CreatedAt giảm dần -> feedback3 (mới nhất) đứng đầu
            Assert.Equal(feedback3.FeedbackId, result.Data.Items[0].FeedbackId);

            // Kiểm tra mapping đúng
            var mapped = result.Data.Items[0];
            Assert.Equal("Nguyen Van A", mapped.CustomerName);
            Assert.Equal("customer1@petcenter.com", mapped.CustomerEmail);
            Assert.Equal("Product1", mapped.ProductName);
            Assert.False(mapped.HasReply);
            Assert.True(mapped.IsVisible);
        }

        //=========================================================
        // UTCID02 - Repository returns empty list
        // Expected: RowNumber loop skipped, Success = true, empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllAsync_RepositoryReturnsEmptyList_ReturnEmptyPagedResult()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var filter = new FeedbackFilterRequestDTO { Page = 1, PageSize = 10 };
            var service = CreateService(context);

            // Act
            var result = await service.GetAllAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!.Items);
            Assert.Equal(0, result.Data.TotalCount);
        }

        //=========================================================
        //=========================================================
        // ReplyAsync() — Service Logic (DB thật)
        // UTCID01/02 (validate rỗng/độ dài) và UTCID03/06 (phụ thuộc
        // Repository.ReplyAsync trả true/false) bỏ qua ở file DB:
        // UTCID01/02 không chạm DB, còn UTCID03/06 đã được cover đủ
        // ở file Mock (ép kết quả repository ổn định hơn qua DB thật).
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - Valid content, Feedback not found
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID04_ReplyAsync_FeedbackNotFound_ReturnFeedbackDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = new ReplyFeedbackRequestDTO
            {
                FeedbackId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                ReplyContent = "Thanks for your feedback!"
            };

            // Act
            var result = await service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
        }

        //=========================================================
        // UTCID05 - Valid content, Feedback exists, already replied
        // Expected: (false, "This feedback already has a reply. Please use the update function.")
        //=========================================================
        [Fact]
        public async Task UTCID05_ReplyAsync_AlreadyReplied_ReturnAlreadyRepliedError()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "5");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                reply: "Existing reply content");

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = new ReplyFeedbackRequestDTO
            {
                FeedbackId = feedback.FeedbackId,
                StaffId = Guid.NewGuid(),
                ReplyContent = "New reply attempt"
            };

            // Act
            var result = await service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "This feedback already has a reply. Please use the update function.",
                result.Message);

            var unchanged = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Equal("Existing reply content", unchanged.Reply);
        }

        //=========================================================
        // UTCID07 - Valid content, Feedback exists, has no reply
        // Expected: (true, "Reply submitted successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID07_ReplyAsync_ValidRequest_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "7");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                reply: null);

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var staff = BuildStaff("staff7@petcenter.com");
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = new ReplyFeedbackRequestDTO
            {
                FeedbackId = feedback.FeedbackId,
                StaffId = staff.StaffId,
                ReplyContent = "Thanks for your feedback!"
            };

            // Act
            var result = await service.ReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply submitted successfully.", result.Message);

            var updated = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Equal("Thanks for your feedback!", updated.Reply);
            Assert.Equal(staff.StaffId, updated.StaffId);
            Assert.NotNull(updated.ReplyDate);
        }

        //=========================================================
        //=========================================================
        // UpdateReplyAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID04 - Valid content, feedback not found
        // Expected: (false, "Feedback does not exist or has no reply yet.")
        //=========================================================
        [Fact]
        public async Task UTCID04_UpdateReplyAsync_FeedbackNotFound_ReturnNotExistOrNoReply()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = new UpdateReplyRequestDTO
            {
                FeedbackId = Guid.NewGuid(),
                ReplyContent = "Updated reply content"
            };

            // Act
            var result = await service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist or has no reply yet.", result.Message);
        }

        //=========================================================
        // UTCID05 - Valid content, feedback exists but has no reply yet
        // Expected: (false, "Feedback does not exist or has no reply yet.")
        //=========================================================
        [Fact]
        public async Task UTCID05_UpdateReplyAsync_FeedbackHasNoReplyYet_ReturnNotExistOrNoReply()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "5u");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                reply: null); // chưa có reply

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = new UpdateReplyRequestDTO
            {
                FeedbackId = feedback.FeedbackId,
                ReplyContent = "Updated reply content"
            };

            // Act
            var result = await service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist or has no reply yet.", result.Message);
        }

        //=========================================================
        // UTCID06 - Valid content, feedback exists with an existing reply
        // Expected: (true, "Reply updated successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID06_UpdateReplyAsync_ValidRequest_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "6u");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                reply: "Old reply content"); // đã có reply từ trước

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var request = new UpdateReplyRequestDTO
            {
                FeedbackId = feedback.FeedbackId,
                ReplyContent = "New updated reply content"
            };

            // Act
            var result = await service.UpdateReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply updated successfully.", result.Message);

            var updated = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Equal("New updated reply content", updated.Reply);
        }

        //=========================================================
        //=========================================================
        // DeleteReplyAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Feedback exists with a reply, delete succeeds
        // Expected: (true, "Reply deleted.")
        //=========================================================
        [Fact]
        public async Task UTCID01_DeleteReplyAsync_FeedbackExists_ReturnSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var staff = BuildStaff("staffdelete1@petcenter.com");
            context.Staffs.Add(staff);
            await context.SaveChangesAsync();

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "1d");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                reply: "Existing reply content");
            feedback.StaffId = staff.StaffId;

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.DeleteReplyAsync(feedback.FeedbackId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply deleted.", result.Message);

            var updated = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Null(updated.Reply);
            Assert.Null(updated.StaffId);
            Assert.Null(updated.ReplyDate);
        }

        //=========================================================
        // UTCID02 - Feedback not found
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID02_DeleteReplyAsync_FeedbackNotFound_ReturnFeedbackDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.DeleteReplyAsync(Guid.NewGuid());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
        }

        //=========================================================
        //=========================================================
        // ToggleVisibilityAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Feedback exists, set visible (isVisible = true)
        // Expected: (true, "Feedback is now visible.")
        //=========================================================
        [Fact]
        public async Task UTCID01_ToggleVisibilityAsync_SetVisible_ReturnVisibleSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "1v");
            var feedback = BuildFeedback(customerId, productId, orderId);
            feedback.Status = 0; // bắt đầu ở trạng thái ẩn

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ToggleVisibilityAsync(feedback.FeedbackId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Feedback is now visible.", result.Message);

            var updated = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Equal(1, updated.Status);
        }

        //=========================================================
        // UTCID02 - Feedback exists, set hidden (isVisible = false)
        // Expected: (true, "Feedback has been hidden.")
        //=========================================================
        [Fact]
        public async Task UTCID02_ToggleVisibilityAsync_SetHidden_ReturnHiddenSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "2v");
            var feedback = BuildFeedback(customerId, productId, orderId);
            feedback.Status = 1; // bắt đầu ở trạng thái hiển thị

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.ToggleVisibilityAsync(feedback.FeedbackId, false);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Feedback has been hidden.", result.Message);

            var updated = await context.ProductFeedbacks
                .AsNoTracking()
                .FirstAsync(f => f.FeedbackId == feedback.FeedbackId);
            Assert.Equal(0, updated.Status);
        }

        //=========================================================
        // UTCID03 - Feedback does not exist
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID03_ToggleVisibilityAsync_FeedbackNotFound_ReturnFeedbackDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.ToggleVisibilityAsync(Guid.NewGuid(), true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
        }

        //=========================================================
        //=========================================================
        // GetByIdAsync() — Service Logic (DB thật)
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Feedback exists
        // Expected: (true, feedback detail returned)
        //=========================================================
        [Fact]
        public async Task UTCID01_GetByIdAsync_FeedbackExists_ReturnFeedbackDetail()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var (customerId, productId, orderId) = await SeedBaseDataAsync(context, "1g");
            var feedback = BuildFeedback(
                customerId, productId, orderId,
                rating: 4, comment: "Nice product", reply: "Thank you!");

            context.ProductFeedbacks.Add(feedback);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetByIdAsync(feedback.FeedbackId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(feedback.FeedbackId, result.Data!.FeedbackId);
            Assert.Equal("Nguyen Van A", result.Data.CustomerName);
            Assert.Equal("Nice product", result.Data.Comment);
            Assert.Equal("Thank you!", result.Data.ReplyContent);
            Assert.True(result.Data.HasReply);
        }

        //=========================================================
        // UTCID02 - Feedback does not exist
        // Expected: (false, "Feedback Not Found.")
        //=========================================================
        [Fact]
        public async Task UTCID02_GetByIdAsync_FeedbackNotFound_ReturnFeedbackNotFound()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback Not Found.", result.Message);
            Assert.Null(result.Data);
        }
    }
}
