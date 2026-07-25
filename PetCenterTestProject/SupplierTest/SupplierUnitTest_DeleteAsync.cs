using AutoMapper;
using Moq;
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
    public class SupplierUnitTest_DeleteAsync
    {
        private readonly Mock<ISupplierRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SupplierService _service;
        public SupplierUnitTest_DeleteAsync()
        {
            _repositoryMock = new Mock<ISupplierRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new SupplierService(
                _repositoryMock.Object,
                _mapperMock.Object);
        }
        #region Unit Test 
        [Fact]
        public async Task UTCID01_DeleteAsync_ExistingActiveSupplier_ShouldReturnTrue()
        {   

            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                IsActive = true
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result);
            Assert.False(supplier.IsActive);

            _repositoryMock.Verify(x => x.Update(supplier), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UTCID02_DeleteAsync_SupplierNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Supplier?)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(x => x.Update(It.IsAny<Supplier>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
        [Fact]
        public async Task UTCID03_DeleteAsync_AlreadyInactiveSupplier_ShouldReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                IsActive = false
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result);
            Assert.False(supplier.IsActive);

            _repositoryMock.Verify(x => x.Update(supplier), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UTCID04_DeleteAsync_SaveChangesThrows_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                SupplierId = id,
                IsActive = true
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database Error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.DeleteAsync(id));
        }
        [Fact]
        public async Task UTCID05_DeleteAsync_GetByIdThrows_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ThrowsAsync(new Exception("Database Error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.DeleteAsync(id));
        }
        #endregion
    }
}
