using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;
using static PetCenterAPI.DTOs.Requests.PrescriptionItem.PrescriptionItemRequestDTO;
using static PetCenterAPI.DTOs.Responses.PrescriptionItem.PrescriptionItemResponseDTO;

namespace PetCenterTestProject
{
    public class PrescriptionItemTest
    {
        private readonly Mock<IPrescriptionItemRepository> _repoMock;
        private readonly PrescriptionItemService _service;

        public PrescriptionItemTest()
        {
            _repoMock = new Mock<IPrescriptionItemRepository>();
            _service = new PrescriptionItemService(_repoMock.Object);
        }

        // =========================================================================
        // SECTION 1: GetByRecordIdAsync() Unit Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByRecordIdAsync_HasItems_ReturnsList()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var items = new List<PrescriptionItem>
            {
                new PrescriptionItem { PrescriptionItemId = Guid.NewGuid(), RecordId = recordId, MedicineName = "Paracetamol", Quantity = 10 }
            };
            _repoMock.Setup(repo => repo.GetByRecordIdAsync(recordId)).ReturnsAsync(items);

            // Act
            var result = await _service.GetByRecordIdAsync(recordId);

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Paracetamol", item.MedicineName);
            Assert.Equal(10, item.Quantity);
        }

        [Fact]
        public async Task UTCID02_GetByRecordIdAsync_NoItems_ReturnsEmpty()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByRecordIdAsync(recordId)).ReturnsAsync(new List<PrescriptionItem>());

            // Act
            var result = await _service.GetByRecordIdAsync(recordId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetByRecordIdAsync_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByRecordIdAsync(recordId)).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByRecordIdAsync(recordId));
        }

        // =========================================================================
        // SECTION 2: GetByIdAsync() Unit Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByIdAsync_Exists_ReturnsDTO()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, MedicineName = "Amoxicillin", Quantity = 5 };
            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _service.GetByIdAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(itemId, result.PrescriptionItemId);
            Assert.Equal("Amoxicillin", result.MedicineName);
        }

        [Fact]
        public async Task UTCID02_GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((PrescriptionItem?)null);

            // Act
            var result = await _service.GetByIdAsync(itemId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetByIdAsync_ThrowsException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(itemId));
        }

        // =========================================================================
        // SECTION 3: CreateAsync() Unit Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateAsync_StatusDrafted_Success()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO { RecordId = recordId, MedicineName = "Ibuprofen", Quantity = 2 };

            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _repoMock.Verify(repo => repo.AddAsync(It.Is<PrescriptionItem>(p =>
                p.RecordId == recordId &&
                p.MedicineName == "Ibuprofen" &&
                p.Quantity == 2
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID02_CreateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO { RecordId = recordId };

            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Completed);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID03_CreateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO { RecordId = recordId };

            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Cancelled);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_CreateAsync_RecordNotFound_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO { RecordId = recordId };

            // Let status return null (not found) and mock AddAsync to throw constraint violation
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int?)null);
            _repoMock.Setup(repo => repo.AddAsync(It.IsAny<PrescriptionItem>())).ThrowsAsync(new Exception("foreign key constraint violation"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
            Assert.Contains("foreign key constraint violation", ex.Message);
        }

        [Fact]
        public async Task UTCID05_CreateAsync_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO { RecordId = recordId };

            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            _repoMock.Setup(repo => repo.AddAsync(It.IsAny<PrescriptionItem>())).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
        }

        // =========================================================================
        // SECTION 4: UpdateAsync() Unit Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateAsync_StatusDrafted_Success()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            var dto = new UpdatePrescriptionItemDTO { MedicineName = "New Med", Quantity = 2 };

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);

            // Act
            await _service.UpdateAsync(itemId, dto);

            // Assert
            Assert.Equal("New Med", item.MedicineName);
            Assert.Equal(2, item.Quantity);
            _repoMock.Verify(repo => repo.UpdateAsync(item), Times.Once);
        }

        [Fact]
        public async Task UTCID02_UpdateAsync_NotFound_ThrowsException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var dto = new UpdatePrescriptionItemDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((PrescriptionItem?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(itemId, dto));
            Assert.Equal("Prescription item not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_UpdateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };
            var dto = new UpdatePrescriptionItemDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Completed);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(itemId, dto));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_UpdateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };
            var dto = new UpdatePrescriptionItemDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Cancelled);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(itemId, dto));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateAsync_ThrowsException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };
            var dto = new UpdatePrescriptionItemDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            _repoMock.Setup(repo => repo.UpdateAsync(item)).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(itemId, dto));
        }

        // =========================================================================
        // SECTION 5: DeleteAsync() Unit Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteAsync_StatusDrafted_Success()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);

            // Act
            await _service.DeleteAsync(itemId);

            // Assert
            _repoMock.Verify(repo => repo.DeleteAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task UTCID02_DeleteAsync_NotFound_ThrowsException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((PrescriptionItem?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteAsync(itemId));
            Assert.Equal("Prescription item not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Completed);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(itemId));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Cancelled);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(itemId));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID05_DeleteAsync_ThrowsException()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem { PrescriptionItemId = itemId, RecordId = recordId };

            _repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            _repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            _repoMock.Setup(repo => repo.DeleteAsync(itemId)).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteAsync(itemId));
        }
    }
}
