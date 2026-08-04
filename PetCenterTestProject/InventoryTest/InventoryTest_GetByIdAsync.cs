using AutoMapper;
using Moq;
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
    public class InventoryTest_GetByIdAsync
    {
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly InventoryService _service;


        public InventoryTest_GetByIdAsync()
        {
            _repositoryMock = new Mock<IInventoryRepository>();

            _mapperMock = new Mock<IMapper>();


            _service = new InventoryService(
                _repositoryMock.Object,
                _mapperMock.Object,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<InventoryService>.Instance
            );
        }



        //=========================================================
        // UTCID01
        // Existing Inventory + batch data
        //=========================================================

        [Fact]
        public async Task UTCID01_GetByIdAsync_ExistingInventory_ShouldReturnDetail()
        {
            // Arrange

            var inventoryId = Guid.NewGuid();


            var entity = new Inventory
            {
                InventoryId = inventoryId,

                ProductId = Guid.NewGuid(),

                QuantityAvailable = 100,

                Product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Dog Food"
                }
            };


            var dto = new InventoryDetailResponseDTO
            {
                InventoryId = inventoryId,

                ProductName = "Dog Food",

                QuantityAvailable = 100,

                Batches = new List<InventoryBatchResponseDTO>
        {
            new InventoryBatchResponseDTO
            {
                ImportStockDetailsId = Guid.NewGuid(),

                BatchCode = "BATCH001",

                Quantity = 50,

                StockLeft = 50,

                BatchStatus = BatchStatus.Active
            }
        }
            };


            _repositoryMock
                .Setup(x => x.GetByIdAsync(inventoryId))
                .ReturnsAsync(entity);


            _mapperMock
                .Setup(x => x.Map<InventoryDetailResponseDTO>(entity))
                .Returns(dto);



            // Act

            var result =
                await _service.GetByIdAsync(inventoryId);



            // Assert

            Assert.NotNull(result);


            Assert.Equal(
                inventoryId,
                result.InventoryId);


            Assert.Single(result.Batches);


            Assert.Equal(
                "BATCH001",
                result.Batches[0].BatchCode);



            _repositoryMock.Verify(
                x => x.GetByIdAsync(inventoryId),
                Times.Once);
        }




        //=========================================================
        // UTCID02
        // Inventory not found
        //=========================================================

        [Fact]
        public async Task UTCID02_GetByIdAsync_NotFound_ShouldReturnNull()
        {
            // Arrange

            var inventoryId = Guid.NewGuid();


            _repositoryMock
                .Setup(x => x.GetByIdAsync(inventoryId))
                .ReturnsAsync((Inventory?)null);



            // Act

            var result =
                await _service.GetByIdAsync(inventoryId);



            // Assert

            Assert.Null(result);


            _mapperMock.Verify(
                x => x.Map<InventoryDetailResponseDTO>(
                    It.IsAny<Inventory>()),
                Times.Never);
        }




        //=========================================================
        // UTCID03
        // Empty batch list
        //=========================================================

        [Fact]
        public async Task UTCID03_GetByIdAsync_EmptyBatch_ShouldReturnInventory()
        {
            // Arrange

            var inventoryId = Guid.NewGuid();


            var entity = new Inventory
            {
                InventoryId = inventoryId,

                
            };
            var batch = new ImportStockDetail
            {
                

               
            };


            var dto = new InventoryDetailResponseDTO
            {
                InventoryId = inventoryId,

                Batches = new List<InventoryBatchResponseDTO>()
            };


            _repositoryMock
                .Setup(x => x.GetByIdAsync(inventoryId))
                .ReturnsAsync(entity);


            _mapperMock
                .Setup(x => x.Map<InventoryDetailResponseDTO>(entity))
                .Returns(dto);



            // Act

            var result =
                await _service.GetByIdAsync(inventoryId);



            // Assert

            Assert.NotNull(result);

            Assert.Empty(result.Batches);
        }




        //=========================================================
        // UTCID04
        // Nullable fields
        //=========================================================

        [Fact]
        public async Task UTCID04_GetByIdAsync_NullOptionalFields_ShouldReturnSuccessfully()
        {
            // Arrange

            var inventoryId = Guid.NewGuid();


            var entity = new Inventory
            {
                InventoryId = inventoryId
            };


            var dto = new InventoryDetailResponseDTO
            {
                InventoryId = inventoryId,

                ProductImage = null
            };


            _repositoryMock
                .Setup(x => x.GetByIdAsync(inventoryId))
                .ReturnsAsync(entity);


            _mapperMock
                .Setup(x => x.Map<InventoryDetailResponseDTO>(entity))
                .Returns(dto);



            // Act

            var result =
                await _service.GetByIdAsync(inventoryId);



            // Assert

            Assert.NotNull(result);

            Assert.Null(result.ProductImage);
        }




        //=========================================================
        // UTCID05
        // Repository exception
        //=========================================================

        [Fact]
        public async Task UTCID05_GetByIdAsync_RepositoryThrowsException_ShouldThrow()
        {
            // Arrange

            var inventoryId = Guid.NewGuid();


            _repositoryMock
                .Setup(x => x.GetByIdAsync(inventoryId))
                .ThrowsAsync(
                    new Exception("Database error"));



            // Act + Assert

            var ex =
                await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetByIdAsync(inventoryId));



            Assert.Equal(
                "Database error",
                ex.Message);
        }
    }
}