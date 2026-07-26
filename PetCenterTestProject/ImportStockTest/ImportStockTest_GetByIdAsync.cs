using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.DTOs.Responses.Import;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.ImportStockTest
{
    public class ImportStockTest_GetByIdAsync
    {
        private readonly Mock<IImportStockRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;


        private readonly PetCenterContext _context;

        private readonly ImportStockService _service;

        public ImportStockTest_GetByIdAsync()
        {
            // Repository
            _repositoryMock = new Mock<IImportStockRepository>();

            // Mapper
            _mapperMock = new Mock<IMapper>();

            // HttpClientFactory (không dùng trong method này)


            // DbContext chỉ để thỏa constructor
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .Options;

            _context = new PetCenterContext(options);

            _service = new ImportStockService(
                _repositoryMock.Object,
                _context,
                _mapperMock.Object);
        }
        [Fact]
        public async Task UTCID01_GetByIdAsync_ExistingImport_ShouldReturnImport()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new ImportStock
            {
                ImportId = id
            };

            var expected = new ReadImportResponseDTO
            {
                ImportId = id
            };

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(x => x.Map<ReadImportResponseDTO>(entity))
                .Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.ImportId);

            _repositoryMock.Verify(x => x.GetWithDetailsAsync(id), Times.Once);
            _mapperMock.Verify(x => x.Map<ReadImportResponseDTO>(entity), Times.Once);
        }
        [Fact]
        public async Task UTCID02_GetByIdAsync_NotFound_ShouldReturnNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ReturnsAsync((ImportStock?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.Null(result);

            _mapperMock.Verify(
                x => x.Map<ReadImportResponseDTO>(It.IsAny<ImportStock>()),
                Times.Never);
        }
        [Fact]
        public async Task UTCID03_GetByIdAsync_WithDetails_ShouldReturnDetails()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new ImportStock();

            var expected = new ReadImportResponseDTO
            {
                ImportId = id,
                Details =
        {
            new ReadImportDetailResponseDTO
            {
                SKU = "SKU001",
                Quantity = 10
            }
        }
            };

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(x => x.Map<ReadImportResponseDTO>(entity))
                .Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.Single(result!.Details);
            Assert.Equal("SKU001", result.Details[0].SKU);
        }
        [Fact]
        public async Task UTCID04_GetByIdAsync_EmptyDetails_ShouldReturnEmptyCollection()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new ImportStock();

            var expected = new ReadImportResponseDTO
            {
                ImportId = id,
                Details = new List<ReadImportDetailResponseDTO>()
            };

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(x => x.Map<ReadImportResponseDTO>(entity))
                .Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!.Details);
        }
        [Fact]
        public async Task UTCID05_GetByIdAsync_NullOptionalFields_ShouldReturnSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new ImportStock();

            var expected = new ReadImportResponseDTO
            {
                ImportId = id,
                ImportDate = null,
                Note = null
            };

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(x => x.Map<ReadImportResponseDTO>(entity))
                .Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.ImportDate);
            Assert.Null(result.Note);
        }
        [Fact]
        public async Task UTCID06_GetByIdAsync_RepositoryThrowsException_ShouldThrow()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetWithDetailsAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.GetByIdAsync(id));

            _repositoryMock.Verify(
                x => x.GetWithDetailsAsync(id),
                Times.Once);
        }
    }
}
