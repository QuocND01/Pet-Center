using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using PetCenterTestProject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductResponseDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductAttributeResponseDTO;

namespace PetCenterTestProject.ProductTest
{
    public class ProductTest_DB : IAsyncLifetime
    {
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly IMapper _mapper;

        public ProductTest_DB()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
            }, NullLoggerFactory.Instance)
             .CreateMapper();

            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var httpClient = new HttpClient();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("OrdersAPI"))
                .Returns(httpClient);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;Database=PetCenter_Test;User Id=sa;Password=123456;TrustServerCertificate=True;")
                .Options;

            return new PetCenterContext(options);
        }

        private ProductRepository CreateRepository(PetCenterContext context)
        {
            return new ProductRepository(context, _mapper);
        }

        /// <summary>
        /// Integration Test
        /// </summary>
        private ProductService CreateService(PetCenterContext context)
        {
            return new ProductService(
                CreateRepository(context),
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        /// <summary>
        /// Unit Test (Mock Repository)
        /// </summary>
        private ProductService CreateService(IProductRepository repository)
        {
            return new ProductService(
                repository,
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await TestDatabaseCleaner.ClearCatalogAsync(context);
        }

        private async Task<Brand> CreateBrandAsync(
    PetCenterContext context,
    string brandName = "Test Brand")
        {
            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = brandName,
                Status = Status.Active
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            return brand;
        }

        private async Task<Category> CreateCategoryAsync(
    PetCenterContext context,
    string categoryName = "Test Category")
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = categoryName,
                Status = Status.Active
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return category;
        }

        private async Task<Category> CreateCategoryWithAttributesAsync(
    PetCenterContext context,
    string categoryName = "Test Category",
    params string[] attributeNames)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = categoryName,
                Status = Status.Active,
                CategoryAttributes = new List<CategoryAttribute>()
            };

            foreach (var attributeName in attributeNames)
            {
                category.CategoryAttributes.Add(
                    new CategoryAttribute
                    {
                        CategoryAttributeId = Guid.NewGuid(),
                        CategoryId = category.CategoryId,
                        AttributeName = attributeName,
                        IsActive = true
                    });
            }

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return category;
        }

        private async Task<(Brand Brand, Category Category)> CreateProductDependenciesAsync(
    PetCenterContext context)
        {
            var brand = await CreateBrandAsync(context);

            var category = await CreateCategoryWithAttributesAsync(
                context,
                "Test Category",
                "Size",
                "Flavor");

            return (brand, category);
        }

        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);

            return results;
        }

        //=========================================================
        // GetAllProduct()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllProductAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID02_GetAllProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            // Act
            var result = await service.GetAllProductAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetAllProductAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            repositoryMock
                .Setup(x => x.GetAllProduct())
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAllProductAsync());

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // GetAllProductAdminAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID02_GetAllProductAdminAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            repositoryMock
                .Setup(x => x.GetAllProductAdminAsync(It.IsAny<ProductSpecification>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAllProductAdminAsync(new ProductSpecification()));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        [Fact]
        public async Task UTCID03_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID04_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID05_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID06_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID07_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID08_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID09_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID10_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID11_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID12_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID13_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID14_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID15_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID16_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID17_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID18_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID19_GetAllProductAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();
            var spec = new ProductSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllProductAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        //=========================================================
        // GetProductByIdAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetProductByIdAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetProductByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Royal Canin", result.ProductName);
        }

        [Fact]
        public async Task UTCID02_GetProductByIdAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProductByIdAsync(id));
            Assert.Equal("Product not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_GetProductByIdAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            repositoryMock
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetProductByIdAsync(Guid.NewGuid()));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // AddProductAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID02_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID03_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var dto = new CreateProductDTO
            {
                ProductName = "New Product",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<CreateProductAttributeDTO>()
            };

            // Act
            await service.AddProductAsync(dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductName == "New Product");

            Assert.NotNull(product);
        }

        [Fact]
        public async Task UTCID04_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID05_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new CreateProductDTO
            {
                ProductName = "Royal Canin",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<CreateProductAttributeDTO>()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddProductAsync(dto));

            Assert.Equal("Product already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID06_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID07_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID08_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var dependencies = await CreateProductDependenciesAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO { ProductName = "New Product", BrandId = dependencies.Brand.BrandId, CategoryId = dependencies.Category.CategoryId, Attributes = new List<CreateProductAttributeDTO>() };

            // Act
            await service.AddProductAsync(dto);

            // Assert
            var product = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "New Product");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task UTCID09_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID10_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var dependencies = await CreateProductDependenciesAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO { ProductName = "New Product", BrandId = dependencies.Brand.BrandId, CategoryId = dependencies.Category.CategoryId, Attributes = new List<CreateProductAttributeDTO>() };

            // Act
            await service.AddProductAsync(dto);

            // Assert
            var product = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "New Product");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task UTCID11_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID12_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID13_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var dependencies = await CreateProductDependenciesAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO { ProductName = "New Product", BrandId = dependencies.Brand.BrandId, CategoryId = dependencies.Category.CategoryId, Attributes = new List<CreateProductAttributeDTO>() };

            // Act
            await service.AddProductAsync(dto);

            // Assert
            var product = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "New Product");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task UTCID14_AddProductAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            repositoryMock
                .Setup(x => x.CheckProductExistAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<ProductAttributeCompareDTO>>(),
                    null))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(x => x.AddProductAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("image.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.Length).Returns(1024);

            var dto = new CreateProductDTO
            {
                ProductName = "Dog Food",
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Attributes = [],
                ImageFiles = [fileMock.Object]
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "products"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.AddProductAsync(dto));

            // Assert
            Assert.Equal("Failed to upload image", ex.Message);

            repositoryMock.Verify(x =>
                x.AddProductAsync(It.IsAny<Product>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID15_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID16_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID17_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var dependencies = await CreateProductDependenciesAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO { ProductName = "New Product", BrandId = dependencies.Brand.BrandId, CategoryId = dependencies.Category.CategoryId, Attributes = new List<CreateProductAttributeDTO>() };

            // Act
            await service.AddProductAsync(dto);

            // Assert
            var product = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "New Product");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task UTCID18_AddProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID19_AddProductAsync_RepositoryThrowsException_ShouldRollbackUploadedImages()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            repositoryMock
                .Setup(x => x.CheckProductExistAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<ProductAttributeCompareDTO>>(),
                    null))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(x => x.AddProductAsync(It.IsAny<Product>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns("image.jpg");
            fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
            fileMock.Setup(x => x.Length).Returns(1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "products"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://test.com/image.jpg"),
                    PublicId = "public-id"
                });

            var dto = new CreateProductDTO
            {
                ProductName = "Dog Food",
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Attributes = [],
                ImageFiles = [fileMock.Object]
            };

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.AddProductAsync(dto));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);

            _cloudinaryServiceMock.Verify(x =>
                x.DeleteImageAsync("public-id"),
                Times.Once);

            repositoryMock.Verify(x =>
                x.AddProductAsync(It.IsAny<Product>()),
                Times.Once);
        }

        //=========================================================
        // UpdateProductAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID02_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID03_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID04_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateProductDTO { ProductName = "New Name", BrandId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), Attributes = new List<UpdateProductAttributeDTO>() };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateProductAsync(id, dto));
            Assert.Equal("Product not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID06_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID07_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID08_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID09_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id1,
                ProductName = "Existing",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            context.Products.Add(new Product
            {
                ProductId = id2,
                ProductName = "Other",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "Existing",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateProductAsync(id2, dto));

            Assert.Equal("Product already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID10_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID11_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID12_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID13_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID14_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID15_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID16_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID17_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID18_UpdateProductAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Dog Food",
                ProductImages = new List<ProductImage>(),
                ProductAttributes = new List<ProductAttribute>()
            };

            repositoryMock
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(product);

            repositoryMock
                .Setup(x => x.CheckProductExistAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<ProductAttributeCompareDTO>>(),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(false);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns("image.jpg");
            fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
            fileMock.Setup(x => x.Length).Returns(1024);

            var dto = new UpdateProductDTO
            {
                ProductName = "Dog Food",
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                ExistingImages = new List<string>(),
                Attributes = new List<UpdateProductAttributeDTO>(),
                ImageFiles = new List<IFormFile>
        {
            fileMock.Object
        }
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "products"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateProductAsync(product.ProductId, dto));

            // Assert
            Assert.Equal("Failed to upload image", ex.Message);

            repositoryMock.Verify(x =>
                x.UpdateProductAsync(It.IsAny<Product>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID19_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID20_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID21_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dependencies = await CreateProductDependenciesAsync(context);

            var id = Guid.NewGuid();

            context.Products.Add(new Product
            {
                ProductId = id,
                ProductName = "Old Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var dto = new UpdateProductDTO
            {
                ProductName = "New Name",
                BrandId = dependencies.Brand.BrandId,
                CategoryId = dependencies.Category.CategoryId,
                Attributes = new List<UpdateProductAttributeDTO>()
            };

            // Act
            await service.UpdateProductAsync(id, dto);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            Assert.NotNull(product);
            Assert.Equal("New Name", product.ProductName);
        }

        [Fact]
        public async Task UTCID22_UpdateProductAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new UpdateProductDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID23_UpdateProductAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Dog Food",
                ProductImages = new List<ProductImage>(),
                ProductAttributes = new List<ProductAttribute>()
            };

            repositoryMock
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(product);

            repositoryMock
                .Setup(x => x.CheckProductExistAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<ProductAttributeCompareDTO>>(),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(x => x.UpdateProductAsync(It.IsAny<Product>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns("image.jpg");
            fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
            fileMock.Setup(x => x.Length).Returns(1024);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "products"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://test.com/image.jpg"),
                    PublicId = "public-id"
                });

            var dto = new UpdateProductDTO
            {
                ProductName = "Dog Food",
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                ExistingImages = new List<string>(),
                Attributes = new List<UpdateProductAttributeDTO>(),
                ImageFiles = new List<IFormFile>
        {
            fileMock.Object
        }
            };

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateProductAsync(product.ProductId, dto));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);

            repositoryMock.Verify(x =>
                x.UpdateProductAsync(It.IsAny<Product>()),
                Times.Once);

            _cloudinaryServiceMock.Verify(x =>
                x.DeleteImageAsync("public-id"),
                Times.Once);
        }

        //=========================================================
        // ChangeProductStatusAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_ChangeProductStatusAsync_Active_ShouldSuccess()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var brandId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = brandId,
                BrandName = "Test Brand",
                Status = Status.Active
            });

            context.Categories.Add(new Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            context.Products.Add(new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                BrandId = brandId,
                CategoryId = categoryId,
                Status = Status.Inactive
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            await service.ChangeProductStatusAsync(
                productId,
                Status.Active);

            // Assert
            var product = await context.Products
                .FirstAsync(p => p.ProductId == productId);

            Assert.Equal(Status.Active, product.Status);
        }

        [Fact]
        public async Task UTCID02_ChangeProductStatusAsync_Inactive_ShouldSuccess()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var brandId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Tạo Brand trước
            context.Brands.Add(new Brand
            {
                BrandId = brandId,
                BrandName = "Test Brand",
                Status = Status.Active
            });

            // Tạo Category trước
            context.Categories.Add(new Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            // Tạo Product với FK hợp lệ
            context.Products.Add(new Product
            {
                ProductId = productId,
                ProductName = "Test Product",
                BrandId = brandId,
                CategoryId = categoryId,
                Status = Status.Inactive
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            await service.ChangeProductStatusAsync(
                productId,
                Status.Inactive);

            // Assert
            var product = await context.Products
                .FirstAsync(p => p.ProductId == productId);

            Assert.Equal(Status.Inactive, product.Status);
        }

        [Fact]
        public async Task UTCID03_ChangeProductStatusAsync_Deleted_ShouldSuccess()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var brandId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Tạo Brand trước
            context.Brands.Add(new Brand
            {
                BrandId = brandId,
                BrandName = "Test Brand",
                Status = PetCenterAPI.Common.Status.Active
            });

            // Tạo Category trước
            context.Categories.Add(new Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Status = PetCenterAPI.Common.Status.Active
            });

            await context.SaveChangesAsync();

            // Tạo Product đang Active
            context.Products.Add(new Product
            {
                ProductId = productId,
                ProductName = "Dog Food",
                BrandId = brandId,
                CategoryId = categoryId,
                Status = PetCenterAPI.Common.Status.Active
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            await service.ChangeProductStatusAsync(
                productId,
                PetCenterAPI.Common.Status.Deleted);

            // Assert
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            Assert.NotNull(product);
            Assert.Equal(
                PetCenterAPI.Common.Status.Deleted,
                product.Status);
        }

        [Fact]
        public async Task UTCID04_ChangeProductStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active));
            Assert.Equal("Product not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_ChangeProductStatusAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IProductRepository>();

            var productId = Guid.NewGuid();

            // Phải trả về Product để Service vượt qua bước kiểm tra tồn tại
            repositoryMock
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(new Product
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    Status = Status.Active
                });

            // Sau đó mới mock Repository ném Exception
            repositoryMock
                .Setup(x => x.ChangeProductStatusAsync(
                    productId,
                    Status.Inactive))
                .ThrowsAsync(
                    new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ChangeProductStatusAsync(
                    productId,
                    Status.Inactive));

            // Assert
            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);
        }
    }
    }
