using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.Service;
using Microsoft.AspNetCore.Http;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using CloudinaryDotNet.Actions;

namespace PetCenterTestProject.FeedbackTest
{
    public class ProductFeedbackTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<IProductFeedbackRepository> _productFeedbackRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly ProductFeedbackService _service;

        //=========================================================
        // Constructor
        //=========================================================
        public ProductFeedbackTest_Mock()
        {
            _productFeedbackRepositoryMock = new Mock<IProductFeedbackRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _service = new ProductFeedbackService(
                _productFeedbackRepositoryMock.Object,
                _cloudinaryServiceMock.Object);
        }

        //=========================================================
        // Helper: Build a ProductFeedback entity with Customer + FeedbackImages
        //=========================================================
        private ProductFeedback BuildFeedback(
            Guid productId,
            string customerName = "Nguyen Van A",
            string? comment = "Great product",
            List<FeedbackImage>? images = null)
        {
            return new ProductFeedback
            {
                FeedbackId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Customer = new Customer
                {
                    CustomerId = Guid.NewGuid(),
                    FullName = customerName
                },
                ProductId = productId,
                OrderId = Guid.NewGuid(),
                Rating = 5,
                Comment = comment,
                Reply = null,
                CreatedAt = DateTime.UtcNow,
                Status = 1,
                FeedbackImages = images ?? new List<FeedbackImage>()
            };
        }

        //=========================================================
        // Helper: Build a valid CreateBulkFeedbackRequestDTO
        //=========================================================
        private CreateBulkFeedbackRequestDTO BuildBulkRequest(
            Guid? orderId = null,
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
                        ProductId = Guid.NewGuid(),
                        OrderId = orderId ?? Guid.NewGuid(),
                        Rating = rating,
                        Comment = comment,
                        MediaFiles = mediaFiles
                    }
                }
            };
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
        // Helper: Build an existing ProductFeedback entity
        //=========================================================
        private ProductFeedback BuildExistingFeedback(
            Guid customerId,
            Guid? feedbackId = null,
            List<FeedbackImage>? images = null)
        {
            return new ProductFeedback
            {
                FeedbackId = feedbackId ?? Guid.NewGuid(),
                CustomerId = customerId,
                Customer = new Customer { CustomerId = customerId, FullName = "Nguyen Van A" },
                ProductId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Old comment",
                CreatedAt = DateTime.UtcNow,
                Status = 1,
                FeedbackImages = images ?? new List<FeedbackImage>()
            };
        }

        //=========================================================
        // GetFeedbacksByProductIdAsync()
        // UTCID01 - Repository returns feedback list
        // Expected: MapToResponse executed for every feedback,
        //           MediaFiles mapped correctly, Customer info mapped
        //=========================================================
        [Fact]
        public async Task UTCID01_GetFeedbacksByProductIdAsync_RepositoryReturnsFeedbackList_ReturnMappedList()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var imageMedia = new FeedbackImage
            {
                ImageId = Guid.NewGuid(),
                ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/feedback1.jpg",
                PublicId = "feedback1",
                IsActive = true
            };
            var videoMedia = new FeedbackImage
            {
                ImageId = Guid.NewGuid(),
                ImageUrl = "https://res.cloudinary.com/demo/video/upload/v1/feedback2.mp4",
                PublicId = "feedback2",
                IsActive = true
            };

            var feedback1 = BuildFeedback(
                productId,
                customerName: "Nguyen Van A",
                comment: "Great product",
                images: new List<FeedbackImage> { imageMedia, videoMedia });

            var feedback2 = BuildFeedback(
                productId,
                customerName: "Tran Thi B",
                comment: "Good quality");

            var feedbackList = new List<ProductFeedback> { feedback1, feedback2 };

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbacksByProductIdAsync(productId))
                .ReturnsAsync(feedbackList);

            // Act
            var result = await _service.GetFeedbacksByProductIdAsync(productId);

            // Assert
            Assert.Equal(2, result.Count);

            // Feedback 1 — kiểm tra mapping đầy đủ + media
            var mapped1 = result.First(r => r.FeedbackId == feedback1.FeedbackId);
            Assert.Equal(feedback1.CustomerId, mapped1.CustomerId);
            Assert.Equal("Nguyen Van A", mapped1.CustomerName);
            Assert.Equal(productId, mapped1.ProductId);
            Assert.Equal(feedback1.OrderId, mapped1.OrderId);
            Assert.Equal(5, mapped1.Rating);
            Assert.Equal("Great product", mapped1.Comment);

            Assert.Equal(2, mapped1.MediaFiles.Count);
            var mappedImage = mapped1.MediaFiles.First(m => m.MediaId == imageMedia.ImageId);
            Assert.Equal(imageMedia.ImageUrl, mappedImage.MediaUrl);
            Assert.Equal(imageMedia.PublicId, mappedImage.PublicId);
            Assert.Equal("image", mappedImage.MediaType);

            var mappedVideo = mapped1.MediaFiles.First(m => m.MediaId == videoMedia.ImageId);
            Assert.Equal(videoMedia.ImageUrl, mappedVideo.MediaUrl);
            Assert.Equal("video", mappedVideo.MediaType);

            // Feedback 2 — không có media
            var mapped2 = result.First(r => r.FeedbackId == feedback2.FeedbackId);
            Assert.Equal("Tran Thi B", mapped2.CustomerName);
            Assert.Empty(mapped2.MediaFiles);
        }

        //=========================================================
        // UTCID02 - Repository returns empty list
        // Expected: Empty list preserved, empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetFeedbacksByProductIdAsync_RepositoryReturnsEmptyList_ReturnEmptyList()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbacksByProductIdAsync(productId))
                .ReturnsAsync(new List<ProductFeedback>());

            // Act
            var result = await _service.GetFeedbacksByProductIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //=========================================================
        // UTCID03 - Repository throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID03_GetFeedbacksByProductIdAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbacksByProductIdAsync(productId))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetFeedbacksByProductIdAsync(productId));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
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
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new CreateBulkFeedbackRequestDTO
            {
                Feedbacks = new List<CreateFeedbackItemRequestDTO>()
            };

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Feedback list cannot be empty.", ex.Message);
        }

        //=========================================================
        // UTCID02 - Feedback list valid, Rating < 1
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID02_CreateBulkFeedbackAsync_RatingBelowMin_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildBulkRequest(rating: 0);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID03 - Feedback list valid, Rating > 5
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID03_CreateBulkFeedbackAsync_RatingAboveMax_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildBulkRequest(rating: 6);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
        }

        //=========================================================
        // UTCID04 - Rating valid, Comment length > 1000
        // Expected: ArgumentException - "Comment cannot exceed 1000 characters."
        //=========================================================
        [Fact]
        public async Task UTCID04_CreateBulkFeedbackAsync_CommentOver1000Chars_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var longComment = new string('A', 1001);
            var request = BuildBulkRequest(rating: 5, comment: longComment);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Comment cannot exceed 1000 characters.", ex.Message);
        }

        //=========================================================
        // UTCID05 - Rating valid, Comment length = 1000 (boundary hợp lệ)
        //=========================================================
        [Fact]
        public async Task UTCID05_CreateBulkFeedbackAsync_CommentExactly1000Chars_DoesNotThrow()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var exactComment = new string('A', 1000);
            var request = BuildBulkRequest(rating: 5, comment: exactComment);

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert — không throw, luồng tiếp tục và trả về danh sách hợp lệ
            Assert.Single(result);
            Assert.Equal(exactComment, result[0].Comment);
        }

        //=========================================================
        // UTCID06 - Rating valid, Comment valid, Order already reviewed
        // Expected: InvalidOperationException - "This order has already been reviewed."
        //=========================================================
        [Fact]
        public async Task UTCID06_CreateBulkFeedbackAsync_OrderAlreadyReviewed_ThrowInvalidOperationException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildBulkRequest(rating: 5, comment: "Nice");

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("This order has already been reviewed.", ex.Message);
            _productFeedbackRepositoryMock.Verify(
                x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()), Times.Never);
        }

        //=========================================================
        // UTCID07 - Order not reviewed, CreateBulkAsync throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID07_CreateBulkFeedbackAsync_CreateBulkAsyncThrows_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildBulkRequest(rating: 5, comment: "Nice");

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID08 - CreateBulkAsync succeeds, AddMediaRangeAsync throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID08_CreateBulkFeedbackAsync_AddMediaRangeAsyncThrows_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var mediaFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { mediaFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(mediaFile, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult());

            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID09 - AddMediaRangeAsync succeeds, GetImagesByFeedbackIdAsync throws
        // Expected: Exception
        //=========================================================
        [Fact]
        public async Task UTCID09_CreateBulkFeedbackAsync_GetImagesByFeedbackIdAsyncThrows_ThrowException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var mediaFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { mediaFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(mediaFile, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult());

            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Returns(Task.CompletedTask);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateBulkFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID10 - No media, full success path
        // Expected: Feedback created, mapped to DTO, list returned
        //           (không assert message, theo Option A đã thống nhất)
        //=========================================================
        [Fact]
        public async Task UTCID10_CreateBulkFeedbackAsync_NoMedia_ReturnMappedFeedbackList()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildBulkRequest(rating: 5, comment: "Nice", mediaFiles: null);

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Equal(request.Feedbacks[0].ProductId, result[0].ProductId);
            Assert.Equal(request.Feedbacks[0].OrderId, result[0].OrderId);
            Assert.Equal(5, result[0].Rating);
            Assert.Equal("Nice", result[0].Comment);
            Assert.Equal(customerId, result[0].CustomerId);
            Assert.Empty(result[0].MediaFiles);

            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID11 - Image uploaded successfully, media within limit
        // Expected: Feedback created, media saved, mapped to DTO
        //=========================================================
        [Fact]
        public async Task UTCID11_CreateBulkFeedbackAsync_ImageUploadedSuccessfully_ReturnFeedbackWithMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var imageFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { imageFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);

            var uploadResult = BuildImageUploadResult("uploaded-image-1");
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(imageFile, "feedbacks"))
                .ReturnsAsync(uploadResult);

            List<FeedbackImage>? capturedMedia = null;
            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Callback<List<FeedbackImage>>(media => capturedMedia = media)
                .Returns(Task.CompletedTask);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedMedia ?? new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].MediaFiles);
            Assert.Equal("uploaded-image-1", result[0].MediaFiles[0].PublicId);
            Assert.Equal("image", result[0].MediaFiles[0].MediaType);

            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(imageFile, "feedbacks"), Times.Once);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Once);
        }

        //=========================================================
        // UTCID12 - Video uploaded successfully, media within limit
        // Expected: Feedback created, media saved, mapped to DTO
        //=========================================================
        [Fact]
        public async Task UTCID12_CreateBulkFeedbackAsync_VideoUploadedSuccessfully_ReturnFeedbackWithMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var videoFile = BuildFormFile("clip.mp4", "video/mp4", 1024);
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { videoFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);

            var uploadResult = BuildVideoUploadResult("uploaded-video-1");
            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(videoFile, "feedbacks"))
                .ReturnsAsync(uploadResult);

            List<FeedbackImage>? capturedMedia = null;
            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Callback<List<FeedbackImage>>(media => capturedMedia = media)
                .Returns(Task.CompletedTask);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedMedia ?? new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].MediaFiles);
            Assert.Equal("uploaded-video-1", result[0].MediaFiles[0].PublicId);
            Assert.Equal("video", result[0].MediaFiles[0].MediaType);

            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(videoFile, "feedbacks"), Times.Once);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID13 - Image > 5MB skipped
        // Expected: Feedback created, no media saved (file bị bỏ qua do quá size)
        //=========================================================
        [Fact]
        public async Task UTCID13_CreateBulkFeedbackAsync_ImageOverSizeLimit_SkipMediaButFeedbackCreated()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var oversizedImage = BuildFormFile("big-photo.jpg", "image/jpeg", 6 * 1024 * 1024); // > 5MB
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { oversizedImage });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].MediaFiles);

            // File quá size bị skip trước khi gọi upload, nên Cloudinary không hề được gọi
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
        }

        //=========================================================
        // UTCID14 - Video > 30MB skipped
        // Expected: Feedback created, no media saved (file bị bỏ qua do quá size)
        //=========================================================
        [Fact]
        public async Task UTCID14_CreateBulkFeedbackAsync_VideoOverSizeLimit_SkipMediaButFeedbackCreated()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var oversizedVideo = BuildFormFile("big-clip.mp4", "video/mp4", 31 * 1024 * 1024); // > 30MB
            var request = BuildBulkRequest(
                rating: 5, comment: "Nice", mediaFiles: new List<IFormFile> { oversizedVideo });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].MediaFiles);

            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
        }

        //=========================================================
        // UTCID15 - Media exceeds limit (3 images + 2 videos given,
        //           only 2 images + 1 video processed)
        // Expected: Feedback created, media saved (chỉ trong giới hạn),
        //           mapped to DTO
        //=========================================================
        [Fact]
        public async Task UTCID15_CreateBulkFeedbackAsync_MediaExceedsLimit_OnlyProcessWithinLimit()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var image1 = BuildFormFile("photo1.jpg", "image/jpeg", 1024);
            var image2 = BuildFormFile("photo2.jpg", "image/jpeg", 1024);
            var image3 = BuildFormFile("photo3.jpg", "image/jpeg", 1024); // vượt limit 2 ảnh, phải bị bỏ
            var video1 = BuildFormFile("clip1.mp4", "video/mp4", 1024);
            var video2 = BuildFormFile("clip2.mp4", "video/mp4", 1024); // vượt limit 1 video, phải bị bỏ

            var request = BuildBulkRequest(
                rating: 5, comment: "Nice",
                mediaFiles: new List<IFormFile> { image1, image2, image3, video1, video2 });

            _productFeedbackRepositoryMock
                .Setup(x => x.HasFeedbackForOrderAsync(
                    request.Feedbacks[0].OrderId, customerId))
                .ReturnsAsync(false);
            _productFeedbackRepositoryMock
                .Setup(x => x.CreateBulkAsync(It.IsAny<List<ProductFeedback>>()))
                .ReturnsAsync((List<ProductFeedback> input) => input);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image1, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("image-1"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image2, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("image-2"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(video1, "feedbacks"))
                .ReturnsAsync(BuildVideoUploadResult("video-1"));

            List<FeedbackImage>? capturedMedia = null;
            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Callback<List<FeedbackImage>>(media => capturedMedia = media)
                .Returns(Task.CompletedTask);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedMedia ?? new List<FeedbackImage>());

            // Act
            var result = await _service.CreateBulkFeedbackAsync(customerId, request);

            // Assert
            Assert.Single(result);
            // Chỉ 2 ảnh + 1 video được xử lý = 3 media, image3 và video2 bị bỏ qua
            Assert.Equal(3, result[0].MediaFiles.Count);
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "image-1");
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "image-2");
            Assert.Contains(result[0].MediaFiles, m => m.PublicId == "video-1");

            // image3 và video2 không được upload vì đã vượt limit trước khi tới lượt xử lý
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(image3, "feedbacks"), Times.Never);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(video2, "feedbacks"), Times.Never);
        }

        //=========================================================
        //=========================================================
        // UpdateFeedbackAsync()
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Rating < 1
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID01_UpdateFeedbackAsync_RatingBelowMin_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildUpdateRequest(Guid.NewGuid(), rating: 0);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Rating must be between 1 and 5.", ex.Message);
            _productFeedbackRepositoryMock.Verify(
                x => x.GetFeedbackByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        // UTCID02 - Rating > 5
        // Expected: ArgumentException - "Rating must be between 1 and 5."
        //=========================================================
        [Fact]
        public async Task UTCID02_UpdateFeedbackAsync_RatingAboveMax_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = BuildUpdateRequest(Guid.NewGuid(), rating: 6);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateFeedbackAsync(customerId, request));

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
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 1, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

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
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 5, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

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
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

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
            // Arrange
            var customerId = Guid.NewGuid();
            var feedbackId = Guid.NewGuid();
            var request = BuildUpdateRequest(feedbackId, rating: 3, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(feedbackId))
                .ReturnsAsync((ProductFeedback?)null);

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.Null(result);
            _productFeedbackRepositoryMock.Verify(
                x => x.UpdateFeedbackAsync(It.IsAny<ProductFeedback>()), Times.Never);
        }

        //=========================================================
        // UTCID07 - Feedback exists, Customer is NOT owner
        // Expected: UnauthorizedAccessException - "You are not allowed to edit this feedback."
        //=========================================================
        [Fact]
        public async Task UTCID07_UpdateFeedbackAsync_CustomerNotOwner_ThrowUnauthorizedAccessException()
        {
            // Arrange
            var ownerCustomerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(ownerCustomerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);

            // Act
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateFeedbackAsync(otherCustomerId, request));

            // Assert
            Assert.Equal("You are not allowed to edit this feedback.", ex.Message);
            _productFeedbackRepositoryMock.Verify(
                x => x.UpdateFeedbackAsync(It.IsAny<ProductFeedback>()), Times.Never);
        }

        //=========================================================
        // UTCID08 - Feedback exists, Customer is owner, UpdateFeedbackAsync returns null
        // Expected: Return null
        //=========================================================
        [Fact]
        public async Task UTCID08_UpdateFeedbackAsync_RepositoryUpdateReturnsNull_ReturnNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: "Valid comment");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync((ProductFeedback?)null);

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.Null(result);
            _productFeedbackRepositoryMock.Verify(
                x => x.GetImagesByFeedbackIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        // UTCID09 - Comment = null/empty (hợp lệ theo code, không throw)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID09_UpdateFeedbackAsync_CommentNullOrEmpty_ReturnUpdatedDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: null);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.Comment);
            Assert.Equal(3, result.Rating);
        }

        //=========================================================
        // UTCID10 - Comment length > 1000
        // Expected: ArgumentException - "Comment cannot exceed 1000 characters."
        //=========================================================
        [Fact]
        public async Task UTCID10_UpdateFeedbackAsync_CommentOver1000Chars_ThrowArgumentException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var longComment = new string('A', 1001);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: longComment);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateFeedbackAsync(customerId, request));

            // Assert
            Assert.Equal("Comment cannot exceed 1000 characters.", ex.Message);
            _productFeedbackRepositoryMock.Verify(
                x => x.UpdateFeedbackAsync(It.IsAny<ProductFeedback>()), Times.Never);
        }

        //=========================================================
        // UTCID11 - Comment length = 1000 (boundary hợp lệ)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID11_UpdateFeedbackAsync_CommentExactly1000Chars_ReturnUpdatedDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var exactComment = new string('A', 1000);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 3, comment: exactComment);

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(exactComment, result!.Comment);
        }

        //=========================================================
        // UTCID12 - Valid rating, valid comment, No media (again, distinct scenario
        //           from UTCID05 per design table — same behavior verified)
        // Expected: MapToResponse executed, DTO returned, update successful
        //=========================================================
        [Fact]
        public async Task UTCID12_UpdateFeedbackAsync_ValidRequestNoMedia_ReturnUpdatedDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var request = BuildUpdateRequest(existing.FeedbackId, rating: 4, comment: "Nice update");

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result!.Rating);
            Assert.Equal("Nice update", result.Comment);
            Assert.Empty(result.MediaFiles);
        }

        //=========================================================
        // UTCID13 - Remove existing media + upload valid new media
        // Expected: DeleteMediaAsync executed, DeleteMediaByPublicIdsAsync executed,
        //           new media uploaded, latest media reloaded, DTO returned
        //=========================================================
        [Fact]
        public async Task UTCID13_UpdateFeedbackAsync_RemoveAndUploadMedia_ReturnUpdatedDtoWithNewMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var oldMedia = new FeedbackImage
            {
                ImageId = Guid.NewGuid(),
                ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/old.jpg",
                PublicId = "old-media-1",
                IsActive = true
            };
            var existing = BuildExistingFeedback(customerId, images: new List<FeedbackImage> { oldMedia });

            var newImageFile = BuildFormFile("new-photo.jpg", "image/jpeg", 1024);
            var request = BuildUpdateRequest(
                existing.FeedbackId,
                rating: 3,
                comment: "Updated with new media",
                newMediaFiles: new List<IFormFile> { newImageFile },
                removedPublicIds: new List<string> { "old-media-1" });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);

            // Lần gọi GetImagesByFeedbackIdAsync đầu tiên (trong khối Remove Media)
            // trả về media cũ để xác định mediaType cần xóa qua Cloudinary
            _productFeedbackRepositoryMock
                .SetupSequence(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage> { oldMedia })   // dùng trong khối Remove Media
                .ReturnsAsync(new List<FeedbackImage>())              // dùng trong khối Upload Media (để tính remaining slots)
                .ReturnsAsync(new List<FeedbackImage>                 // dùng ở bước Reload latest media cuối cùng
                {
                    new FeedbackImage
                    {
                        ImageId = Guid.NewGuid(),
                        ImageUrl = "https://res.cloudinary.com/demo/image/upload/v1/new.jpg",
                        PublicId = "new-media-1",
                        IsActive = true
                    }
                });

            _cloudinaryServiceMock
                .Setup(x => x.DeleteMediaAsync("old-media-1", "image"))
                .Returns(Task.CompletedTask);
            _productFeedbackRepositoryMock
                .Setup(x => x.DeleteMediaByPublicIdsAsync(
                    It.Is<List<string>>(ids => ids.Contains("old-media-1"))))
                .Returns(Task.CompletedTask);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(newImageFile, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("new-media-1"));

            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.MediaFiles);
            Assert.Equal("new-media-1", result.MediaFiles[0].PublicId);

            _cloudinaryServiceMock.Verify(
                x => x.DeleteMediaAsync("old-media-1", "image"), Times.Once);
            _productFeedbackRepositoryMock.Verify(
                x => x.DeleteMediaByPublicIdsAsync(
                    It.Is<List<string>>(ids => ids.Contains("old-media-1"))), Times.Once);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(newImageFile, "feedbacks"), Times.Once);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Once);
        }

        //=========================================================
        // UTCID14 - Image > 5MB skipped
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID14_UpdateFeedbackAsync_ImageOverSizeLimit_ReturnDtoWithoutNewMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var oversizedImage = BuildFormFile("big-photo.jpg", "image/jpeg", 6 * 1024 * 1024); // > 5MB
            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { oversizedImage });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
        }

        //=========================================================
        // UTCID15 - Video > 30MB skipped
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID15_UpdateFeedbackAsync_VideoOverSizeLimit_ReturnDtoWithoutNewMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var oversizedVideo = BuildFormFile("big-clip.mp4", "video/mp4", 31 * 1024 * 1024); // > 30MB
            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { oversizedVideo });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
        }

        //=========================================================
        // UTCID16 - Upload returns non-OK status
        // Expected: MapToResponse executed, DTO returned (media không được thêm)
        //=========================================================
        [Fact]
        public async Task UTCID16_UpdateFeedbackAsync_UploadReturnsNonOk_ReturnDtoWithoutNewMedia()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var imageFile = BuildFormFile("photo.jpg", "image/jpeg", 1024);
            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { imageFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(imageFile, "feedbacks"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest // non-OK
                });

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(imageFile, "feedbacks"), Times.Once);
            _productFeedbackRepositoryMock.Verify(
                x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()), Times.Never);
        }

        //=========================================================
        // UTCID17 - Exceed image limit (>2), chỉ 2 ảnh đầu tiên được xử lý
        // Expected: MapToResponse executed, DTO returned với đúng 2 media
        //=========================================================
        [Fact]
        public async Task UTCID17_UpdateFeedbackAsync_ExceedImageLimit_OnlyProcessWithinLimit()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);

            var image1 = BuildFormFile("photo1.jpg", "image/jpeg", 1024);
            var image2 = BuildFormFile("photo2.jpg", "image/jpeg", 1024);
            var image3 = BuildFormFile("photo3.jpg", "image/jpeg", 1024); // vượt limit 2, phải bị bỏ

            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { image1, image2, image3 });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);

            // Không có media hiện có -> remainingImageSlots = 2, remainingVideoSlots = 1
            _productFeedbackRepositoryMock
                .SetupSequence(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>())   // dùng để tính remaining slots
                .ReturnsAsync(new List<FeedbackImage>       // dùng ở bước reload cuối
                {
                    new FeedbackImage { ImageId = Guid.NewGuid(), ImageUrl = "url1", PublicId = "img-1", IsActive = true },
                    new FeedbackImage { ImageId = Guid.NewGuid(), ImageUrl = "url2", PublicId = "img-2", IsActive = true }
                });

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image1, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("img-1"));
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(image2, "feedbacks"))
                .ReturnsAsync(BuildImageUploadResult("img-2"));

            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

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
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);

            var video1 = BuildFormFile("clip1.mp4", "video/mp4", 1024);
            var video2 = BuildFormFile("clip2.mp4", "video/mp4", 1024); // vượt limit 1, phải bị bỏ

            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { video1, video2 });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);

            _productFeedbackRepositoryMock
                .SetupSequence(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>())
                .ReturnsAsync(new List<FeedbackImage>
                {
                    new FeedbackImage { ImageId = Guid.NewGuid(), ImageUrl = "video-url1", PublicId = "vid-1", IsActive = true }
                });

            _cloudinaryServiceMock
                .Setup(x => x.UploadVideoAsync(video1, "feedbacks"))
                .ReturnsAsync(BuildVideoUploadResult("vid-1"));

            _productFeedbackRepositoryMock
                .Setup(x => x.AddMediaRangeAsync(It.IsAny<List<FeedbackImage>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(video2, "feedbacks"), Times.Never);
        }

        //=========================================================
        // UTCID19 - Invalid extension ignored (file không thuộc danh sách
        //           extension cho phép, bị lọc bỏ trước khi upload)
        // Expected: MapToResponse executed, DTO returned (không có media mới)
        //=========================================================
        [Fact]
        public async Task UTCID19_UpdateFeedbackAsync_InvalidExtension_IgnoreFileAndReturnDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existing = BuildExistingFeedback(customerId);
            var invalidFile = BuildFormFile("document.pdf", "image/jpeg", 1024); // ext .pdf không hợp lệ

            var request = BuildUpdateRequest(
                existing.FeedbackId, rating: 3, comment: "Valid comment",
                newMediaFiles: new List<IFormFile> { invalidFile });

            _productFeedbackRepositoryMock
                .Setup(x => x.GetFeedbackByIdAsync(existing.FeedbackId))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.UpdateFeedbackAsync(existing))
                .ReturnsAsync(existing);
            _productFeedbackRepositoryMock
                .Setup(x => x.GetImagesByFeedbackIdAsync(existing.FeedbackId))
                .ReturnsAsync(new List<FeedbackImage>());

            // Act
            var result = await _service.UpdateFeedbackAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.MediaFiles);
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _cloudinaryServiceMock.Verify(
                x => x.UploadVideoAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }
    }
}
