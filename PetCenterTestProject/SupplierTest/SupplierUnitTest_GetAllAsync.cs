using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace PetCenterTestProject.ServiceTest { 


public class SupplierUnitTest_GetAllAsync
    {
    private readonly Mock<ISupplierRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly SupplierService _service;

    public SupplierUnitTest_GetAllAsync()
    {
        _repositoryMock = new Mock<ISupplierRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new SupplierService(
            _repositoryMock.Object,
            _mapperMock.Object);
    }

    #region GetAllAsync

    /// <summary>
    /// UTCID01
    /// Repository returns supplier list (>=1 record)
    /// Expected: Return mapped supplier list successfully.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnSupplierList_WhenRepositoryHasData()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierName = "Supplier A",
                SupplierPhoneNumber = "0123456789",
                SupplierEmail = "a@test.com"
            },
            new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierName = "Supplier B",
                SupplierPhoneNumber = "0987654321",
                SupplierEmail = "b@test.com"
            }
        };

        var expected = new List<ReadSupplierResponseDTO>
        {
            new ReadSupplierResponseDTO
            {
                SupplierId = suppliers[0].SupplierId,
                SupplierName = suppliers[0].SupplierName
            },
            new ReadSupplierResponseDTO
            {
                SupplierId = suppliers[1].SupplierId,
                SupplierName = suppliers[1].SupplierName
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(suppliers);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers))
            .Returns(expected);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _mapperMock.Verify(
            m => m.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers),
            Times.Once);
    }

    /// <summary>
    /// UTCID02
    /// Repository returns empty list.
    /// Expected: Return empty list.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        var suppliers = new List<Supplier>();

        var expected = new List<ReadSupplierResponseDTO>();

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(suppliers);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers))
            .Returns(expected);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// UTCID03
    /// Supplier contains null optional fields.
    /// Expected: Mapping still succeeds.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnSupplier_WhenOptionalFieldsAreNull()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierName = "Supplier A",
                SupplierEmail = null,
                SupplierAddress = null,
                SupplierPhoneNumber = null
            }
        };

        var expected = new List<ReadSupplierResponseDTO>
        {
            new ReadSupplierResponseDTO
            {
                SupplierId = suppliers[0].SupplierId,
                SupplierName = suppliers[0].SupplierName,
                SupplierEmail = null,
                SupplierAddress = null,
                SupplierPhoneNumber = null
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(suppliers);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers))
            .Returns(expected);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Null(result.First().SupplierEmail);
        Assert.Null(result.First().SupplierAddress);
        Assert.Null(result.First().SupplierPhoneNumber);
    }

    /// <summary>
    /// UTCID04
    /// Repository returns inactive supplier.
    /// Expected: Service returns supplier without filtering.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnInactiveSupplier_WhenRepositoryContainsInactiveSupplier()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierName = "Inactive Supplier",
                IsActive = false
            }
        };

        var expected = new List<ReadSupplierResponseDTO>
        {
            new ReadSupplierResponseDTO
            {
                SupplierId = suppliers[0].SupplierId,
                SupplierName = suppliers[0].SupplierName,
                IsActive = false
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(suppliers);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ReadSupplierResponseDTO>>(suppliers))
            .Returns(expected);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        var supplier = Assert.Single(result);

        Assert.False(supplier.IsActive);
    }

    /// <summary>
    /// UTCID05
    /// Repository throws exception.
    /// Expected: Exception is propagated.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GetAllAsync());

        Assert.Equal(
            "Service Temporarily Unavailable",
            exception.Message);

        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

        #endregion

       
    }
}