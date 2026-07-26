using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.SupplierTest
{
    public class SupplierUnitTest_GetByIdAsync
    {
        private readonly Mock<ISupplierRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SupplierService _service;
        public SupplierUnitTest_GetByIdAsync()
        {
            _repositoryMock = new Mock<ISupplierRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new SupplierService(
                _repositoryMock.Object,
                _mapperMock.Object);
        }
        [Fact]
        public async Task UTCID01_GetByIdAsync_ExistingSupplier_ShouldReturnSupplier()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                SupplierName = "ABC Supplier"
            };

            var response = new ReadSupplierResponseDTO
            {
                SupplierId = id,
                SupplierName = "ABC Supplier"
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _mapperMock.Setup(x => x.Map<ReadSupplierResponseDTO>(supplier))
                .Returns(response);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.SupplierId);
            Assert.Equal("ABC Supplier", result.SupplierName);

            _mapperMock.Verify(x => x.Map<ReadSupplierResponseDTO>(supplier), Times.Once);
        }
        [Fact]
        public async Task UTCID02_GetByIdAsync_SupplierNotFound_ShouldReturnNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Supplier?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.Null(result);

            _mapperMock.Verify(
                x => x.Map<ReadSupplierResponseDTO>(It.IsAny<Supplier>()),
                Times.Never);
        }
        [Fact]
        public async Task UTCID03_GetByIdAsync_EmptySupplier_ShouldReturnMappedObject()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier();

            var response = new ReadSupplierResponseDTO();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _mapperMock.Setup(x => x.Map<ReadSupplierResponseDTO>(supplier))
                .Returns(response);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UTCID04_GetByIdAsync_NullOptionalFields_ShouldReturnSupplier()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                SupplierName = "ABC",
                ContactPerson = null,
                SupplierDescription = null,
                TaxId = null
            };

            var response = new ReadSupplierResponseDTO
            {
                SupplierId = id,
                SupplierName = "ABC",
                ContactPerson = null,
                SupplierDescription = null,
                TaxId = null
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _mapperMock.Setup(x => x.Map<ReadSupplierResponseDTO>(supplier))
                .Returns(response);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.TaxId);
            Assert.Null(result.ContactPerson);
            Assert.Null(result.SupplierDescription);
        }
        [Fact]
        public async Task UTCID05_GetByIdAsync_InactiveSupplier_ShouldReturnSupplier()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                IsActive = false
            };

            var response = new ReadSupplierResponseDTO
            {
                SupplierId = id,
                IsActive = false
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _mapperMock.Setup(x => x.Map<ReadSupplierResponseDTO>(supplier))
                .Returns(response);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result!.IsActive);
        }
        [Fact]
        public async Task UTCID06_GetByIdAsync_RepositoryThrows_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ThrowsAsync(new Exception("Database Error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.GetByIdAsync(id));
        }

    }
}
