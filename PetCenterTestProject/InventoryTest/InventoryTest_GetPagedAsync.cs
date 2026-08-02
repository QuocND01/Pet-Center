using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.InventoryTest
{
    public class InventoryTest_GetPagedAsync
    {
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly InventoryService _service;


        public InventoryTest_GetPagedAsync()
        {
            _repositoryMock = new Mock<IInventoryRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new InventoryService(
                _repositoryMock.Object,
                _mapperMock.Object
                
            );
        }


        //=========================================================
        // UTCID01
        // Valid request + inventory data
        //=========================================================
        [Fact]
        public async Task UTCID01_GetPagedAsync_ShouldReturnInventoryListSuccessfully()
        {
            // Arrange
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    InventoryId = Guid.NewGuid(),
                    SKU = "SKU001",
                    QuantityAvailable = 10
                }
            };


            var responseItems = new List<InventoryItemResponseDTO>
            {
                new InventoryItemResponseDTO
                {
                    SKU = "SKU001",
                    QuantityAvailable = 10
                }
            };


            _repositoryMock
                .Setup(x => x.GetPagedAsync(request))
                .ReturnsAsync((inventories, 1));


            _mapperMock
                .Setup(x => x.Map<List<InventoryItemResponseDTO>>(inventories))
                .Returns(responseItems);


            // Act
            var result = await _service.GetPagedAsync(request);


            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);

            Assert.Equal(1, result.TotalRecords);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);


            _repositoryMock.Verify(
                x => x.GetPagedAsync(request),
                Times.Once);
        }



        //=========================================================
        // UTCID02
        // Empty result
        //=========================================================
        [Fact]
        public async Task UTCID02_GetPagedAsync_EmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            var inventories = new List<Inventory>();


            _repositoryMock
                .Setup(x => x.GetPagedAsync(request))
                .ReturnsAsync((inventories, 0));


            _mapperMock
                .Setup(x => x.Map<List<InventoryItemResponseDTO>>(inventories))
                .Returns(new List<InventoryItemResponseDTO>());


            // Act
            var result = await _service.GetPagedAsync(request);


            // Assert
            Assert.NotNull(result);

            Assert.Empty(result.Items);

            Assert.Equal(0, result.TotalRecords);
            Assert.Equal(0, result.TotalPages);
        }



        //=========================================================
        // UTCID03
        // TotalRecords > PageSize
        //=========================================================
        [Fact]
        public async Task UTCID03_GetPagedAsync_ShouldCalculateTotalPagesCorrectly()
        {
            // Arrange
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            var inventories = new List<Inventory>
            {
                new Inventory()
            };


            _repositoryMock
                .Setup(x => x.GetPagedAsync(request))
                .ReturnsAsync((inventories, 25));


            _mapperMock
                .Setup(x => x.Map<List<InventoryItemResponseDTO>>(inventories))
                .Returns(new List<InventoryItemResponseDTO>());


            // Act
            var result = await _service.GetPagedAsync(request);


            // Assert
            Assert.Equal(25, result.TotalRecords);

            // Ceiling(25 / 10)
            Assert.Equal(3, result.TotalPages);
        }



        //=========================================================
        // UTCID04
        // Repository exception
        //=========================================================
        [Fact]
        public async Task UTCID04_GetPagedAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var request = new InventoryQueryRequestDTO
            {
                Page = 1,
                PageSize = 10
            };


            _repositoryMock
                .Setup(x => x.GetPagedAsync(request))
                .ThrowsAsync(
                    new Exception("Database error")
                );


            // Act
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _service.GetPagedAsync(request)
            );


            // Assert
            Assert.Equal(
                "Database error",
                ex.Message);
        }
    }
}