using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.DTOs.Responses.Import;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.ImportStockTest
{
    public class ImportStockTest_GetAllAsync
    {
        private readonly Mock<IImportStockRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        

        private readonly PetCenterContext _context;

        private readonly ImportStockService _service;

        public ImportStockTest_GetAllAsync()
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
        public async Task UTCID01_GetAllImportsAsync_ReturnImportList_ShouldReturnMappedList()
        {
            // Arrange
            var imports = new List<ImportStock>
    {
        new()
        {
            ImportId = Guid.NewGuid(),
            Status = ImportStock.ImportStatus.Pending
        },
        new()
        {
            ImportId = Guid.NewGuid(),
            Status = ImportStock.ImportStatus.Confirmed
        }
    };

            var expected = new List<ReadImportHeaderResponseDTO>
    {
        new()
        {
            ImportId = imports[0].ImportId,
            Status = ImportStock.ImportStatus.Pending
        },
        new()
        {
            ImportId = imports[1].ImportId,
            Status = ImportStock.ImportStatus.Confirmed
        }
    };

            _repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(imports);

            _mapperMock
                .Setup(x => x.Map<List<ReadImportHeaderResponseDTO>>(imports))
                .Returns(expected);

            // Act
            var result = await _service.GetAllImportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            _mapperMock.Verify(
                x => x.Map<List<ReadImportHeaderResponseDTO>>(imports),
                Times.Once);
        }
        [Fact]
        public async Task UTCID02_GetAllImportsAsync_EmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var imports = new List<ImportStock>();
            var expected = new List<ReadImportHeaderResponseDTO>();

            _repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(imports);

            _mapperMock
                .Setup(x => x.Map<List<ReadImportHeaderResponseDTO>>(imports))
                .Returns(expected);

            // Act
            var result = await _service.GetAllImportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
        [Fact]
        public async Task UTCID03_GetAllImportsAsync_DifferentStatusValues_ShouldReturnCorrectResult()
        {
            // Arrange
            var imports = new List<ImportStock>
    {
        new() { Status = ImportStock.ImportStatus.Pending },
        new() { Status = ImportStock.ImportStatus.Confirmed },
        new() { Status = ImportStock.ImportStatus.Cancelled }
    };

            var expected = new List<ReadImportHeaderResponseDTO>
    {
        new() { Status = ImportStock.ImportStatus.Pending },
        new() { Status = ImportStock.ImportStatus.Confirmed },
        new() { Status = ImportStock.ImportStatus.Cancelled }
    };

            _repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(imports);

            _mapperMock
                .Setup(x => x.Map<List<ReadImportHeaderResponseDTO>>(imports))
                .Returns(expected);

            // Act
            var result = await _service.GetAllImportsAsync();

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal(ImportStock.ImportStatus.Pending, result[0].Status);
            Assert.Equal(ImportStock.ImportStatus.Confirmed, result[1].Status);
            Assert.Equal(ImportStock.ImportStatus.Cancelled, result[2].Status);
        }
        [Fact]
        public async Task UTCID04_GetAllImportsAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.GetAllImportsAsync());

            _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

    }
}
