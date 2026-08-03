using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;

namespace PetCenterTestProject.FeedbackTest
{
    public class AdminFeedbackTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================
        private readonly Mock<IAdminFeedbackRepository> _adminFeedbackRepositoryMock;
        private readonly AdminFeedbackService _service;

        //=========================================================
        // Constructor
        //=========================================================
        public AdminFeedbackTest_Mock()
        {
            _adminFeedbackRepositoryMock = new Mock<IAdminFeedbackRepository>();

            _service = new AdminFeedbackService(
                _adminFeedbackRepositoryMock.Object);
        }

        //=========================================================
        // Helper: Build a FeedbackFilterRequestDTO
        //=========================================================
        private FeedbackFilterRequestDTO BuildFilter(int page = 1, int pageSize = 10)
        {
            return new FeedbackFilterRequestDTO
            {
                Page = page,
                PageSize = pageSize
            };
        }

        //=========================================================
        // Helper: Build a single feedback entity
        //=========================================================
        private ProductFeedback BuildFeedbackItem()
        {
            return new ProductFeedback
            {
                FeedbackId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Customer = new Customer { FullName = "Nguyen Van A", Email = "customer@petcenter.com" },
                ProductId = Guid.NewGuid(),
                Product = new Product { ProductName = "Dog Food" },
                OrderId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great product",
                Reply = null,
                CreatedAt = DateTime.UtcNow,
                Status = 1
            };
        }

        //=========================================================
        // Helper: Build a valid ReplyFeedbackRequestDTO, override fields as needed
        //=========================================================
        private ReplyFeedbackRequestDTO BuildReplyRequest(
            Guid? feedbackId = null,
            Guid? staffId = null,
            string replyContent = "Thank you for your feedback!")
        {
            return new ReplyFeedbackRequestDTO
            {
                FeedbackId = feedbackId ?? Guid.NewGuid(),
                StaffId = staffId ?? Guid.NewGuid(),
                ReplyContent = replyContent
            };
        }

        //=========================================================
        // Helper: Build an existing feedback entity returned by repository
        //=========================================================
        private ProductFeedback BuildExistingFeedback(
            Guid feedbackId,
            string? replyContent = null)
        {
            return new ProductFeedback
            {
                FeedbackId = feedbackId,
                CustomerId = Guid.NewGuid(),
                Customer = new Customer { FullName = "Nguyen Van A", Email = "customer@petcenter.com" },
                ProductId = Guid.NewGuid(),
                Product = new Product { ProductName = "Dog Food Premium" },
                Rating = 5,
                Comment = "Great product!",
                Reply = replyContent,
                CreatedAt = DateTime.Now,
                Status = 1
            };
        }

        //=========================================================
        // Helper: Build a valid UpdateReplyRequestDTO, override fields as needed
        //=========================================================
        private UpdateReplyRequestDTO BuildUpdateReplyRequest(
            Guid? feedbackId = null,
            string replyContent = "Updated reply content")
        {
            return new UpdateReplyRequestDTO
            {
                FeedbackId = feedbackId ?? Guid.NewGuid(),
                ReplyContent = replyContent
            };
        }

        //=========================================================
        // GetAllAsync()
        // UTCID01 - Repository returns feedback list
        // Expected: Start index + RowNumber calculated correctly,
        //           Success = true, paged list returned
        //=========================================================
        [Fact]
        public async Task UTCID01_GetAllAsync_RepositoryReturnsFeedbackList_ReturnPagedListWithCorrectRowNumber()
        {
            // Arrange
            var filter = BuildFilter(page: 2, pageSize: 10);
            var items = new List<ProductFeedback>
            {
                BuildFeedbackItem(),
                BuildFeedbackItem(),
                BuildFeedbackItem()
            };

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetAllAsync(filter))
                .ReturnsAsync((items, 23));

            // Act
            var result = await _service.GetAllAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data!.Items.Count);

            // Start index = (Page - 1) * PageSize + 1 = (2-1)*10 + 1 = 11
            Assert.Equal(11, result.Data.Items[0].RowNumber);
            Assert.Equal(12, result.Data.Items[1].RowNumber);
            Assert.Equal(13, result.Data.Items[2].RowNumber);
        }

        //=========================================================
        // UTCID02 - Repository returns empty list
        // Expected: RowNumber loop skipped, Success = true, empty list returned
        //=========================================================
        [Fact]
        public async Task UTCID02_GetAllAsync_RepositoryReturnsEmptyList_ReturnEmptyPagedResult()
        {
            // Arrange
            var filter = BuildFilter();

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetAllAsync(filter))
                .ReturnsAsync((new List<ProductFeedback>(), 0));

            // Act
            var result = await _service.GetAllAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!.Items);
        }

        //=========================================================
        // UTCID03 - Repository throws exception
        // Expected: Exception thrown
        //=========================================================
        [Fact]
        public async Task UTCID03_GetAllAsync_RepositoryThrowsException_ThrowException()
        {
            // Arrange
            var filter = BuildFilter();

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetAllAsync(filter))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAllAsync(filter));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // UTCID01 - Reply content empty/null/whitespace
        // Expected: (false, "Reply content cannot be empty.")
        //=========================================================
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task UTCID01_ReplyAsync_ReplyContentEmpty_ReturnEmptyError(string? replyContent)
        {
            // Arrange
            var request = BuildReplyRequest(replyContent: replyContent!);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reply content cannot be empty.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        // UTCID02 - Reply content length > 1000
        // Expected: (false, "Reply content cannot exceed 1000 characters.")
        //=========================================================
        [Fact]
        public async Task UTCID02_ReplyAsync_ReplyContentOver1000Chars_ReturnLengthError()
        {
            // Arrange
            var longContent = new string('A', 1001);
            var request = BuildReplyRequest(replyContent: longContent);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reply content cannot exceed 1000 characters.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        //=========================================================
        // UTCID03 - Reply content length = 1000 (boundary hợp lệ),
        //           Feedback exists, has no reply, ReplyAsync succeeds
        // Expected: (true, "Reply submitted successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID03_ReplyAsync_ReplyContentExactly1000Chars_ReturnSuccess()
        {
            // Arrange
            var content1000 = new string('A', 1000);
            Assert.Equal(1000, content1000.Length);
            var feedbackId = Guid.NewGuid();
            var request = BuildReplyRequest(feedbackId: feedbackId, replyContent: content1000);
            var existingFeedback = BuildExistingFeedback(feedbackId, replyContent: null);

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync(existingFeedback);
            _adminFeedbackRepositoryMock
                .Setup(x => x.ReplyAsync(feedbackId, request.StaffId, content1000))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply submitted successfully.", result.Message);
        }

        //=========================================================
        // UTCID04 - Valid content, Feedback not found
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID04_ReplyAsync_FeedbackNotFound_ReturnFeedbackDoesNotExist()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildReplyRequest(feedbackId: feedbackId);

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync((ProductFeedback?)null);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.ReplyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID05 - Valid content, Feedback exists, already replied
        // Expected: (false, "This feedback already has a reply. Please use the update function.")
        //=========================================================
        [Fact]
        public async Task UTCID05_ReplyAsync_AlreadyReplied_ReturnAlreadyRepliedError()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildReplyRequest(feedbackId: feedbackId);
            var existingFeedback = BuildExistingFeedback(feedbackId, replyContent: "Existing reply content");

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync(existingFeedback);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(
                "This feedback already has a reply. Please use the update function.",
                result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.ReplyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID06 - Valid content, Feedback exists, has no reply,
        //           Repository ReplyAsync returns false
        // Expected: (false, "An error occurred while submitting the reply.")
        //=========================================================
        [Fact]
        public async Task UTCID06_ReplyAsync_RepositoryReturnsFalse_ReturnSubmitError()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildReplyRequest(feedbackId: feedbackId);
            var existingFeedback = BuildExistingFeedback(feedbackId, replyContent: null);

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync(existingFeedback);
            _adminFeedbackRepositoryMock
                .Setup(x => x.ReplyAsync(feedbackId, request.StaffId, request.ReplyContent))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while submitting the reply.", result.Message);
        }

        //=========================================================
        // UTCID07 - Valid content, Feedback exists, has no reply,
        //           Repository ReplyAsync returns true
        // Expected: (true, "Reply submitted successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID07_ReplyAsync_ValidRequest_ReturnSuccess()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildReplyRequest(feedbackId: feedbackId, replyContent: "Thanks for your feedback!");
            var existingFeedback = BuildExistingFeedback(feedbackId, replyContent: null);

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync(existingFeedback);
            _adminFeedbackRepositoryMock
                .Setup(x => x.ReplyAsync(feedbackId, request.StaffId, request.ReplyContent))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply submitted successfully.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.ReplyAsync(feedbackId, request.StaffId, request.ReplyContent), Times.Once);
        }

        //=========================================================
        //=========================================================
        // UpdateReplyAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Reply content empty/null/whitespace
        // Expected: (false, "Reply content cannot be empty.")
        //=========================================================
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task UTCID01_UpdateReplyAsync_ReplyContentEmpty_ReturnEmptyError(string? replyContent)
        {
            // Arrange
            var request = BuildUpdateReplyRequest(replyContent: replyContent!);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reply content cannot be empty.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.UpdateReplyAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID02 - Reply content length > 1000
        // Expected: (false, "Reply content cannot exceed 1000 characters.")
        //=========================================================
        [Fact]
        public async Task UTCID02_UpdateReplyAsync_ReplyContentOver1000Chars_ReturnLengthError()
        {
            // Arrange
            var longContent = new string('A', 1001);
            var request = BuildUpdateReplyRequest(replyContent: longContent);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reply content cannot exceed 1000 characters.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.UpdateReplyAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        //=========================================================
        // UTCID03 - Reply content length = 1000 (boundary hợp lệ),
        //           Repository UpdateReplyAsync returns true
        // Expected: (true, "Reply updated successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID03_UpdateReplyAsync_ReplyContentExactly1000Chars_ReturnSuccess()
        {
            // Arrange
            var content1000 = new string('A', 1000);
            Assert.Equal(1000, content1000.Length);
            var feedbackId = Guid.NewGuid();
            var request = BuildUpdateReplyRequest(feedbackId: feedbackId, replyContent: content1000);

            _adminFeedbackRepositoryMock
                .Setup(x => x.UpdateReplyAsync(feedbackId, content1000))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply updated successfully.", result.Message);
        }

        //=========================================================
        // UTCID04 - Valid content, feedback not found,
        //           Repository UpdateReplyAsync returns false
        // Expected: (false, "Feedback does not exist or has no reply yet.")
        //=========================================================
        [Fact]
        public async Task UTCID04_UpdateReplyAsync_FeedbackNotFound_ReturnNotExistOrNoReply()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildUpdateReplyRequest(feedbackId: feedbackId);

            _adminFeedbackRepositoryMock
                .Setup(x => x.UpdateReplyAsync(feedbackId, request.ReplyContent))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist or has no reply yet.", result.Message);
        }

        //=========================================================
        // UTCID05 - Valid content, feedback exists but has no reply yet,
        //           Repository UpdateReplyAsync returns false
        // Expected: (false, "Feedback does not exist or has no reply yet.")
        //=========================================================
        [Fact]
        public async Task UTCID05_UpdateReplyAsync_FeedbackHasNoReplyYet_ReturnNotExistOrNoReply()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildUpdateReplyRequest(feedbackId: feedbackId);

            _adminFeedbackRepositoryMock
                .Setup(x => x.UpdateReplyAsync(feedbackId, request.ReplyContent))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist or has no reply yet.", result.Message);
        }

        //=========================================================
        // UTCID06 - Valid content, Repository UpdateReplyAsync returns true
        // Expected: (true, "Reply updated successfully.")
        //=========================================================
        [Fact]
        public async Task UTCID06_UpdateReplyAsync_ValidRequest_ReturnSuccess()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = BuildUpdateReplyRequest(feedbackId: feedbackId, replyContent: "Updated reply!");

            _adminFeedbackRepositoryMock
                .Setup(x => x.UpdateReplyAsync(feedbackId, request.ReplyContent))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateReplyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply updated successfully.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.UpdateReplyAsync(feedbackId, request.ReplyContent), Times.Once);
        }

        //=========================================================
        //=========================================================
        // DeleteReplyAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Repository DeleteReplyAsync returns true
        // Expected: (true, "Reply deleted.")
        //=========================================================
        [Fact]
        public async Task UTCID01_DeleteReplyAsync_RepositoryReturnsTrue_ReturnSuccess()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.DeleteReplyAsync(feedbackId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteReplyAsync(feedbackId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reply deleted.", result.Message);
            _adminFeedbackRepositoryMock.Verify(x => x.DeleteReplyAsync(feedbackId), Times.Once);
        }

        //=========================================================
        // UTCID02 - Repository DeleteReplyAsync returns false
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID02_DeleteReplyAsync_RepositoryReturnsFalse_ReturnFeedbackDoesNotExist()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.DeleteReplyAsync(feedbackId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteReplyAsync(feedbackId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
        }

        //=========================================================
        //=========================================================
        // ToggleVisibilityAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Feedback exists, set visible (isVisible = true),
        //           Repository returns true
        // Expected: (true, "Feedback is now visible.")
        //=========================================================
        [Fact]
        public async Task UTCID01_ToggleVisibilityAsync_SetVisible_ReturnVisibleSuccess()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.ToggleVisibilityAsync(feedbackId, true))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ToggleVisibilityAsync(feedbackId, true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Feedback is now visible.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.ToggleVisibilityAsync(feedbackId, true), Times.Once);
        }

        //=========================================================
        // UTCID02 - Feedback exists, set hidden (isVisible = false),
        //           Repository returns true
        // Expected: (true, "Feedback has been hidden.")
        //=========================================================
        [Fact]
        public async Task UTCID02_ToggleVisibilityAsync_SetHidden_ReturnHiddenSuccess()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.ToggleVisibilityAsync(feedbackId, false))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ToggleVisibilityAsync(feedbackId, false);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Feedback has been hidden.", result.Message);
            _adminFeedbackRepositoryMock.Verify(
                x => x.ToggleVisibilityAsync(feedbackId, false), Times.Once);
        }

        //=========================================================
        // UTCID03 - Feedback does not exist, Repository returns false
        // Expected: (false, "Feedback does not exist.")
        //=========================================================
        [Fact]
        public async Task UTCID03_ToggleVisibilityAsync_FeedbackNotFound_ReturnFeedbackDoesNotExist()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.ToggleVisibilityAsync(feedbackId, true))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ToggleVisibilityAsync(feedbackId, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback does not exist.", result.Message);
        }

        //=========================================================
        //=========================================================
        // GetByIdAsync() — Service Logic
        //=========================================================
        //=========================================================

        //=========================================================
        // UTCID01 - Repository GetByIdAsync returns feedback
        // Expected: (true, feedback detail returned)
        //=========================================================
        [Fact]
        public async Task UTCID01_GetByIdAsync_FeedbackExists_ReturnFeedbackDetail()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var existingFeedback = BuildExistingFeedback(feedbackId, replyContent: "Thanks!");

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync(existingFeedback);

            // Act
            var result = await _service.GetByIdAsync(feedbackId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(feedbackId, result.Data!.FeedbackId);
            Assert.Equal(existingFeedback.Customer.FullName, result.Data.CustomerName);
            Assert.Equal(existingFeedback.Reply, result.Data.ReplyContent);
        }

        //=========================================================
        // UTCID02 - Repository GetByIdAsync returns null
        // Expected: (false, "Feedback Not Found.")
        //=========================================================
        [Fact]
        public async Task UTCID02_GetByIdAsync_FeedbackNotFound_ReturnFeedbackNotFound()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();

            _adminFeedbackRepositoryMock
                .Setup(x => x.GetByIdAsync(feedbackId))
                .ReturnsAsync((ProductFeedback?)null);

            // Act
            var result = await _service.GetByIdAsync(feedbackId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback Not Found.", result.Message);
            Assert.Null(result.Data);
        }
    }
}
