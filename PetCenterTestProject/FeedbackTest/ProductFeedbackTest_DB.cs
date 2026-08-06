using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Common;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterTestProject;

namespace PetCenterTestProject.FeedbackTest
{
    [Collection("DatabaseTests")]
    public class ProductFeedbackTest_DB
    {
        //=========================================================
        // Mock external service only
        //=========================================================
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        //=========================================================
        // Constructor
        //=========================================================
        public ProductFeedbackTest_DB()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        }

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
        private ProductFeedbackRepository CreateRepository(PetCenterContext context)
        {
            return new ProductFeedbackRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================
        private ProductFeedbackService CreateService(PetCenterContext context)
        {
            return new ProductFeedbackService(
                CreateRepository(context),
                _cloudinaryServiceMock.Object);
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
        private Customer BuildCustomer(string email, string fullName)
        {
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                FullName = fullName,
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
        // Helper: Build a ProductFeedback (Status = 1 nghĩa là visible)
        //=========================================================
        private ProductFeedback BuildFeedback(
            Guid customerId,
            Guid productId,
            Guid orderId,
            int status = 1,
            int? rating = 5,
            string? comment = "Great product",
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
                CreatedAt = createdAt ?? DateTime.Now,
                Status = status
            };
        }

        //=========================================================
        // Helper: Seed a full valid Customer + Brand + Category + Product
        //          + Address + Order chain, returns their IDs
        //=========================================================
        private async Task<(Guid CustomerId, Guid ProductId, Guid OrderId)> SeedBaseDataAsync(
            PetCenterContext context, string emailSuffix, string customerName = "Nguyen Van A")
        {
            var customer = BuildCustomer($"customer{emailSuffix}@petcenter.com", customerName);
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
        // Helper: Build a fake IFormFile for upload testing
        //=========================================================
        private IFormFile BuildFormFile(string fileName, string contentType, long length)
        {
            var content = new byte[length];
            var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, length, "MediaFiles", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
            return formFile;
        }

        //=========================================================
        // Helper: Build a fake successful ImageUploadResult
        //=========================================================
        private ImageUploadResult BuildImageUploadResult(string publicId = "feedback-image-1")
        {
            return new ImageUploadResult
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                SecureUrl = new Uri($"https://res.cloudinary.com/demo/image/upload/v1/{publicId}.jpg"),
                PublicId = publicId
            };
        }

        //=========================================================
        // Helper: Build a fake successful VideoUploadResult
        //=========================================================
        private VideoUploadResult BuildVideoUploadResult(string publicId = "feedback-video-1")
        {
            return new VideoUploadResult
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                SecureUrl = new Uri($"https://res.cloudinary.com/demo/video/upload/v1/{publicId}.mp4"),
                PublicId = publicId
            };
        }

        //=========================================================
        // Helper: Build a valid CreateBulkFeedbackRequestDTO
        //=========================================================
        private CreateBulkFeedbackRequestDTO BuildBulkRequest(
            Guid productId,
            Guid orderId,
            int rating = 5,
            string? comment = "Great product",
            List<IFormFile>? mediaFiles = null)
        {
            return new CreateBulkFeedbackRequestDTO
            {
                Feedbacks = new List<CreateFeedbackItemRequestDTO>
                {
                    new CreateFeedbackItemRequestDTO
                    {
                        ProductId = productId,
                        OrderId = orderId,
                        Rating = rating,
                        Comment = comment,
                        MediaFiles = mediaFiles
                    }
                }
            };
        }

        //=========================================================
        // Helper: Build a valid UpdateFeedbackRequestDTO
        //=========================================================
        private UpdateFeedbackRequestDTO BuildUpdateRequest(
            Guid feedbackId,
            int rating = 3,
            string? comment = "Updated comment",
            List<IFormFile>? newMediaFiles = null,
            List<string>? removedPublicIds = null)
        {
            return new UpdateFeedbackRequestDTO
            {
                FeedbackId = feedbackId,
                Rating = rating,
                Comment = comment,
                NewMediaFiles = newMediaFiles,
                RemovedPublicIds = removedPublicIds
            };
        }

        //=========================================================
        // GetFeedbacksByProductIdAsync()
        //=========================================================

        //=========================================================
        // UTCID01 - Repository returns feedback list
        // Expected: MapToResponse executed for every feedback,
        //           MediaFiles mapped correctly, Customer info mapped
        //=========================================================
        [Fact]
        public async Task UTCID01_GetFeedbacksByProductIdAsync_RepositoryReturnsFeedbackList_ReturnMappedList()
        {
            Guid customerId, productId, orderId, feedbackId, activeImageId, activeVideoId, inactiveImageId;

            // Seed toàn bộ dữ liệu bằng context riêng, dispose ngay sau khi xong
            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);

                var seeded = await SeedBaseDataAsync(seedContext, "1", "Nguyen Van A");
                customerId = seeded.CustomerId;
                productId = seeded.ProductId;
                orderId = seeded.OrderId;

                var feedback = BuildFeedback(
                    customerId, productId, orderId,
                    status: 1, rating: 5, comment: "Great product");

                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;

                var imageMedia = new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/feedback1.jpg",
                    PublicId = "feedback1",
                    IsActive = true
                };
                var videoMedia = new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = "https://res.cloudinary.com/demo/video/upload/v1/feedback2.mp4",
                    PublicId = "feedback2",
                    IsActive = true
                };
                var inactiveMedia = new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/inactive.jpg",
                    PublicId = "inactive",
                    IsActive = false // media đã bị xóa mềm, không được trả về
                };

                seedContext.FeedbackImages.AddRange(imageMedia, videoMedia, inactiveMedia);
                await seedContext.SaveChangesAsync();

                activeImageId = imageMedia.ImageId;
                activeVideoId = videoMedia.ImageId;
                inactiveImageId = inactiveMedia.ImageId;
            }

            // Act — dùng context MỚI, hoàn toàn độc lập với context seed,
            // đảm bảo query không bị ảnh hưởng bởi entity đang tracking
            using var context = CreateContext();
            var service = CreateService(context);
            var result = await service.GetFeedbacksByProductIdAsync(productId);

            // Assert
            Assert.Single(result);
            var mapped = result[0];

            Assert.Equal(feedbackId, mapped.FeedbackId);
            Assert.Equal(customerId, mapped.CustomerId);
            Assert.Equal("Nguyen Van A", mapped.CustomerName);
            Assert.Equal(productId, mapped.ProductId);
            Assert.Equal(orderId, mapped.OrderId);
            Assert.Equal(5, mapped.Rating);
            Assert.Equal("Great product", mapped.Comment);

            // Chỉ 2 media active được trả về, media inactive bị loại
            Assert.Equal(2, mapped.MediaFiles.Count);
            Assert.DoesNotContain(mapped.MediaFiles, m => m.MediaId == inactiveImageId);

            var mappedImage = mapped.MediaFiles.First(m => m.MediaId == activeImageId);
            Assert.Equal("image", mappedImage.MediaType);

            var mappedVideo = mapped.MediaFiles.First(m => m.MediaId == activeVideoId);
            Assert.Equal("video", mappedVideo.MediaType);
        }

        //=========================================================
        // UTCID02 - Repository returns empty list
        // Expected: Empty list preserved, empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetFeedbacksByProductIdAsync_RepositoryReturnsEmptyList_ReturnEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            // Act
            var result = await service.GetFeedbacksByProductIdAsync(Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //=========================================================
        //=========================================================
        // CreateBulkFeedbackAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Feedback list empty
        // Expected: ArgumentException - "Feedback list cannot be empty."
        //=========================================================
        [Fact]
        public async Task UTCID01_CreateBulkFeedbackAsync_FeedbackListEmpty_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = new CreateBulkFeedbackRequestDTO
            {
                Feedbacks = new List<CreateFeedbackItemRequestDTO>()
            };

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateBulkFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Feedback list cannot be empty.", ex.Message);
        }

        //=========================================================
        // UTCID02 - Rating < 1
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID02_CreateBulkFeedbackAsync_RatingBelowMin_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildBulkRequest(Guid.NewGuid(), Guid.NewGuid(), rating: 0);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateBulkFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID03 - Rating > 5
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID03_CreateBulkFeedbackAsync_RatingAboveMax_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildBulkRequest(Guid.NewGuid(), Guid.NewGuid(), rating: 6);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateBulkFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID04 - Comment length > 1000
        // Expected: ArgumentException - "Comment cannot exceed 1000 characters."
        //=========================================================
        [Fact]
        public async Task UTCID04_CreateBulkFeedbackAsync_CommentOver1000Chars_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var longComment = new string('A', 1001);
            var request = BuildBulkRequest(Guid.NewGuid(), Guid.NewGuid(), comment: longComment);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateBulkFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Comment cannot exceed 1000 characters.", ex.Message);
        }

        //=========================================================
        // UTCID05 - Comment length = 1000 (boundary hợp lệ, không throw)
        // Expected: Feedback created successfully
        //=========================================================
        [Fact]
        public async Task UTCID05_CreateBulkFeedbackAsync_CommentExactly1000Chars_DoesNotThrow()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "5");
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var exactComment = new string('A', 1000);
            var request = BuildBulkRequest(productId, orderId, comment: exactComment);

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Equal(exactComment, result[0].Comment);
        }

        //=========================================================
        // UTCID06 - Order already reviewed
        // Expected: InvalidOperationException - "This order has already been reviewed."
        //=========================================================
        [Fact]
        public async Task UTCID06_CreateBulkFeedbackAsync_OrderAlreadyReviewed_ThrowInvalidOperationException()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "6");

                var existingFeedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(existingFeedback);
                await seedContext.SaveChangesAsync();
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildBulkRequest(productId, orderId, comment: "Trying again");

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("This order has already been reviewed.", ex.Message);
        }

        //=========================================================
        // UTCID10 - No media, full success path
        // Expected: Feedback created, mapped to DTO, list returned
        //=========================================================
        [Fact]
        public async Task UTCID10_CreateBulkFeedbackAsync_NoMedia_ReturnMappedFeedbackList()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "10");
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildBulkRequest(productId, orderId, comment: "Nice", mediaFiles: null);

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Equal(productId, result[0].ProductId);
            Assert.Equal(orderId, result[0].OrderId);
            Assert.Equal(5, result[0].Rating);
            Assert.Empty(result[0].MediaFiles);

            // Xác nhận đã thực sự lưu vào DB
            var savedFeedback = await context.ProductFeedbacks
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.OrderId == orderId);
            Assert.NotNull(savedFeedback);
        }

        //=========================================================
        // UTCID11 - Image uploaded successfully, media within limit
        // Expected: Feedback created, media saved, mapped to DTO
        //=========================================================
        [Fact]
        public async Task UTCID11_CreateBulkFeedbackAsync_ImageUploadedSuccessfully_ReturnFeedbackWithMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "11");
            }

            using var context = CreateContext();
            var imageFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(imageFile, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("db-uploaded-image-1"));

            var service = CreateService(context);
            var request = BuildBulkRequest(
                productId, orderId, comment: "Nice", mediaFiles: new List<IFormFile> { imageFile });

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].MediaFiles);
            Assert.Equal("db-uploaded-image-1", result[0].MediaFiles[0].PublicId);
            Assert.Equal("image", result[0].MediaFiles[0].MediaType);

            var savedMedia = await context.FeedbackImages
                .Where(m => m.PublicId == "db-uploaded-image-1")
                .ToListAsync();
            Assert.Single(savedMedia);
        }

        //=========================================================
        // UTCID12 - Video uploaded successfully, media within limit
        // Expected: Feedback created, media saved, mapped to DTO
        //=========================================================
        [Fact]
        public async Task UTCID12_CreateBulkFeedbackAsync_VideoUploadedSuccessfully_ReturnFeedbackWithMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "12");
            }

            using var context = CreateContext();
            var videoFile = BuildFormFile("clip.mp4", "video/mp4", 1024);
            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(videoFile, "feedbacks"))
                .ReturnsAsync(BuildVideoUploadResult("db-uploaded-video-1"));

            var service = CreateService(context);
            var request = BuildBulkRequest(
                productId, orderId, comment: "Nice", mediaFiles: new List<IFormFile> { videoFile });

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].MediaFiles);
            Assert.Equal("db-uploaded-video-1", result[0].MediaFiles[0].PublicId);
            Assert.Equal("video", result[0].MediaFiles[0].MediaType);
        }

        //=========================================================
        // UTCID13 - Image > 5MB skipped
        // Expected: Feedback created, no media saved
        //=========================================================
        [Fact]
        public async Task UTCID13_CreateBulkFeedbackAsync_ImageOverSizeLimit_SkipMediaButFeedbackCreated()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "13");
            }

            using var context = CreateContext();
            var oversizedImage = BuildFormFile("big-photo.jpg", "image/jpeg", 6 * 1024 * 1024);

            var service = CreateService(context);
            var request = BuildBulkRequest(
                productId, orderId, comment: "Nice",
                mediaFiles: new List<IFormFile> { oversizedImage });

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID14 - Video > 30MB skipped
        // Expected: Feedback created, no media saved
        //=========================================================
        [Fact]
        public async Task UTCID14_CreateBulkFeedbackAsync_VideoOverSizeLimit_SkipMediaButFeedbackCreated()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "14");
            }

            using var context = CreateContext();
            var oversizedVideo = BuildFormFile("big-clip.mp4", "video/mp4", 31 * 1024 * 1024);

            var service = CreateService(context);
            var request = BuildBulkRequest(
                productId, orderId, comment: "Nice",
                mediaFiles: new List<IFormFile> { oversizedVideo });

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID15 - Media exceeds limit (3 images + 2 videos given,
        //           only 2 images + 1 video processed)
        // Expected: Feedback created, media saved (chỉ trong giới hạn)
        //=========================================================
        [Fact]
        public async Task UTCID15_CreateBulkFeedbackAsync_MediaExceedsLimit_OnlyProcessWithinLimit()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "15");
            }

            using var context = CreateContext();

            var image1 = BuildFormFile("photo1.jpg", "image/jpeg", 1024);
            var image2 = BuildFormFile("photo2.jpg", "image/jpeg", 1024);
            var image3 = BuildFormFile("photo3.jpg", "image/jpeg", 1024);
            var video1 = BuildFormFile("clip1.mp4", "video/mp4", 1024);
            var video2 = BuildFormFile("clip2.mp4", "video/mp4", 1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image1, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("db-image-1"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image2, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("db-image-2"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(video1, "feedbacks"))
                .ReturnsAsync(BuildVideoUploadResult("db-video-1"));

            var service = CreateService(context);
            var request = BuildBulkRequest(
                productId, orderId, comment: "Nice",
                mediaFiles: new List<IFormFile> { image1, image2, image3, video1, video2 });

            // Act
            var result = await service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].MediaFiles.Count);
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "db-image-1");
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "db-image-2");
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "db-video-1");

            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(image3, "feedbacks"), Times.Never);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(video2, "feedbacks"), Times.Never);
        }

        //=========================================================
        //=========================================================
        // UpdateFeedbackAsync()
        // Bỏ UTCID08 (UpdateFeedbackAsync returns null) vì đây là kịch bản
        // race condition (record bị xóa giữa lúc GetFeedbackByIdAsync và
        // UpdateFeedbackAsync chạy) — không tái tạo được với DB thật
        // hoạt động bình thường trong 1 luồng gọi tuần tự. Case này chỉ
        // có ở file Mock.
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Rating < 1
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID01_UpdateFeedbackAsync_RatingBelowMin_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildUpdateRequest(Guid.NewGuid(), rating: 0);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID02 - Rating > 5
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID02_UpdateFeedbackAsync_RatingAboveMax_ThrowArgumentException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildUpdateRequest(Guid.NewGuid(), rating: 6);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateFeedbackAsync(Guid.NewGuid(), request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID03 - Rating = 1 (boundary hợp lệ)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID03_UpdateFeedbackAsync_RatingEqualsMin_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd1");

                var feedback = BuildFeedback(customerId, productId, orderId, comment: "Old comment");
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 1, comment: "Valid comment");

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Rating);
            Assert.Equal("Valid comment", result.Comment);
        }

        //=========================================================
        // UTCID04 - Rating = 5 (boundary hợp lệ)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID04_UpdateFeedbackAsync_RatingEqualsMax_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd2");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 5, comment: "Valid comment");

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result!.Rating);
        }

        //=========================================================
        // UTCID05 - Valid rating (2-4), Valid comment, No media
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID05_UpdateFeedbackAsync_ValidRatingNoMedia_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd3");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: "Valid comment");

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result!.Rating);
            Assert.Equal("Valid comment", result.Comment);
            Assert.Empty(result.MediaFiles);
        }

        //=========================================================
        // UTCID06 - Feedback not found
        // Expected: Return null
        //=========================================================
        [Fact]
        public async Task UTCID06_UpdateFeedbackAsync_FeedbackNotFound_ReturnNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var request = BuildUpdateRequest(Guid.NewGuid(), rating: 3, comment: "Valid comment");

            // Act
            var result = await service.UpdateFeedbackAsync(Guid.NewGuid(), request);

            // Assert
            Assert.Null(result);
        }

        //=========================================================
        // UTCID07 - Feedback exists, Customer is NOT owner
        // Expected: UnauthorizedAccessException
        //=========================================================
        [Fact]
        public async Task UTCID07_UpdateFeedbackAsync_CustomerNotOwner_ThrowUnauthorizedAccessException()
        {
            var (ownerCustomerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (ownerCustomerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd4");

                var feedback = BuildFeedback(ownerCustomerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: "Valid comment");
            var otherCustomerId = Guid.NewGuid();

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateFeedbackAsync(otherCustomerId, request));

            // Assert
            Assert.Equal("You are not allowed to edit this feedback.", ex.Message);
        }

        //=========================================================
        // UTCID09 - Comment = null/empty (hợp lệ)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID09_UpdateFeedbackAsync_CommentNullOrEmpty_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd5");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: null);

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.Comment);
        }

        //=========================================================
        // UTCID10 - Comment length > 1000
        // Expected: ArgumentException - "Comment cannot exceed 1000 characters."
        //=========================================================
        [Fact]
        public async Task UTCID10_UpdateFeedbackAsync_CommentOver1000Chars_ThrowArgumentException()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd6");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var longComment = new string('A', 1001);
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: longComment);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Comment cannot exceed 1000 characters.", ex.Message);
        }

        //=========================================================
        // UTCID11 - Comment length = 1000 (boundary hợp lệ)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID11_UpdateFeedbackAsync_CommentExactly1000Chars_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd7");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var exactComment = new string('A', 1000);
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: exactComment);

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(exactComment, result!.Comment);
        }

        //=========================================================
        // UTCID12 - Valid rating, valid comment, no media
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID12_UpdateFeedbackAsync_ValidRequestNoMedia_ReturnUpdatedDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd8");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var service = CreateService(context);
            var request = BuildUpdateRequest(feedbackId, rating: 4, comment: "Nice update");

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result!.Rating);
            Assert.Equal("Nice update", result.Comment);
        }

        //=========================================================
        // UTCID13 - Remove existing media + upload valid new media
        // Expected: DeleteMediaByPublicIdsAsync executed, new media uploaded,
        //           latest media reloaded, DTO returned
        //=========================================================
        [Fact]
        public async Task UTCID13_UpdateFeedbackAsync_RemoveAndUploadMedia_ReturnUpdatedDtoWithNewMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd9");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;

                var oldMedia = new FeedbackImage
                {
                    ImageId = Guid.NewGuid(),
                    FeedbackId = feedbackId,
                    ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/old.jpg",
                    PublicId = "old-media-db-1",
                    IsActive = true
                };
                seedContext.FeedbackImages.Add(oldMedia);
                await seedContext.SaveChangesAsync();
            }

            using var context = CreateContext();
            var newImageFile = BuildFormFile("new-photo.jpg", "image/jpeg", 1024);

            _cloudinaryServiceMock
                .Setup(x => x.DeleteMediaAsync("old-media-db-1", "image"))
                .Returns(Task.CompletedTask);
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(newImageFile, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("new-media-db-1"));

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Updated with new media",
                newMediaFiles: new List<IFormFile> { newImageFile },
                removedPublicIds: new List<string> { "old-media-db-1" });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.MediaFiles);
            Assert.Equal("new-media-db-1", result.MediaFiles[0].PublicId);

            // Xác nhận media cũ đã bị soft-delete (IsActive = false)
            var oldMediaInDb = await context.FeedbackImages
                .FirstOrDefaultAsync(m => m.PublicId == "old-media-db-1");
            Assert.NotNull(oldMediaInDb);
            Assert.False(oldMediaInDb!.IsActive);

            _cloudinaryServiceMock.Verify(
                x => x.DeleteMediaAsync("old-media-db-1", "image"), Times.Once);
        }

        //=========================================================
        // UTCID14 - Image > 5MB skipped
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID14_UpdateFeedbackAsync_ImageOverSizeLimit_ReturnDtoWithoutNewMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd10");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var oversizedImage = BuildFormFile("big-photo.jpg", "image/jpeg", 6 * 1024 * 1024);

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { oversizedImage });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID15 - Video > 30MB skipped
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID15_UpdateFeedbackAsync_VideoOverSizeLimit_ReturnDtoWithoutNewMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd11");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var oversizedVideo = BuildFormFile("big-clip.mp4", "video/mp4", 31 * 1024 * 1024);

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { oversizedVideo });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID16 - Upload returns non-OK status
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID16_UpdateFeedbackAsync_UploadReturnsNonOk_ReturnDtoWithoutNewMedia()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd12");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var imageFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(imageFile, "feedbacks"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { imageFile });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
        }

        //=========================================================
        // UTCID17 - Exceed image limit (>2), chỉ 2 ảnh đầu tiên được xử lý
        // Expected: MapToResponse executed, DTO returned với đúng 2 media
        //=========================================================
        [Fact]
        public async Task UTCID17_UpdateFeedbackAsync_ExceedImageLimit_OnlyProcessWithinLimit()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd13");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var image1 = BuildFormFile("photo1.jpg", "image/jpeg", 1024);
            var image2 = BuildFormFile("photo2.jpg", "image/jpeg", 1024);
            var image3 = BuildFormFile("photo3.jpg", "image/jpeg", 1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image1, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("db-img-1"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image2, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("db-img-2"));

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { image1, image2, image3 });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.MediaFiles.Count);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(image3, "feedbacks"), Times.Never);
        }

        //=========================================================
        // UTCID18 - Exceed video limit (>1), chỉ 1 video đầu tiên được xử lý
        // Expected: MapToResponse executed, DTO returned với đúng 1 media
        //=========================================================
        [Fact]
        public async Task UTCID18_UpdateFeedbackAsync_ExceedVideoLimit_OnlyProcessWithinLimit()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd14");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var video1 = BuildFormFile("clip1.mp4", "video/mp4", 1024);
            var video2 = BuildFormFile("clip2.mp4", "video/mp4", 1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(video1, "feedbacks"))
                .ReturnsAsync(BuildVideoUploadResult("db-vid-1"));

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { video1, video2 });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(video2, "feedbacks"), Times.Never);
        }

        //=========================================================
        // UTCID19 - Invalid extension ignored
        // Expected: MapToResponse executed, DTO returned (không có media mới)
        //=========================================================
        [Fact]
        public async Task UTCID19_UpdateFeedbackAsync_InvalidExtension_IgnoreFileAndReturnDto()
        {
            var (customerId, productId, orderId) = default((Guid, Guid, Guid));
            Guid feedbackId;

            using (var seedContext = CreateContext())
            {
                await ClearDatabaseAsync(seedContext);
                (customerId, productId, orderId) = await SeedBaseDataAsync(seedContext, "upd15");

                var feedback = BuildFeedback(customerId, productId, orderId);
                seedContext.ProductFeedbacks.Add(feedback);
                await seedContext.SaveChangesAsync();
                feedbackId = feedback.FeedbackId;
            }

            using var context = CreateContext();
            var invalidFile = BuildFormFile("document.pdf", "image/jpeg", 1024);

            var service = CreateService(context);
            var request = BuildUpdateRequest(
                feedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { invalidFile });

            // Act
            var result = await service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }
    }
}
