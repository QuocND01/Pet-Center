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
        public class ProductTest_Mock
        {
            private readonly Mock<IProductRepository> _productRepositoryMock;
            private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
            private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
            private readonly IMapper _mapper;
            private readonly ProductService _service;

            public ProductTest_Mock()
            {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
            }, NullLoggerFactory.Instance)
       .CreateMapper();

            _productRepositoryMock = new Mock<IProductRepository>();
                _cloudinaryServiceMock = new Mock<ICloudinaryService>();
                _httpClientFactoryMock = new Mock<IHttpClientFactory>();

                var httpClient = new HttpClient();
                _httpClientFactoryMock.Setup(x => x.CreateClient("OrdersAPI")).Returns(httpClient);

                _service = new ProductService(
                    _productRepositoryMock.Object,
                    _mapper,
                    _cloudinaryServiceMock.Object,
                    _httpClientFactoryMock.Object);
            }

            private IList<ValidationResult> Validate(object model)
            {
                var context = new ValidationContext(model);
                var results = new List<ValidationResult>();
                Validator.TryValidateObject(model, context, results, true);
                return results;
            }

            //=========================================================
            // GetAllProduct()
            //=========================================================

            [Fact]
            public async Task UTCID01_GetAllProductAsync()
            {
                // Arrange
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProduct())
                    .ReturnsAsync(products);

                // Act
                var result = await _service.GetAllProductAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Single(result);
                _productRepositoryMock.Verify(x => x.GetAllProduct(), Times.Once);
            }

            [Fact]
            public async Task UTCID02_GetAllProductAsync()
            {
                // Arrange
                var products = new List<Product>();

                _productRepositoryMock
                    .Setup(x => x.GetAllProduct())
                    .ReturnsAsync(products);

                // Act
                var result = await _service.GetAllProductAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
                _productRepositoryMock.Verify(x => x.GetAllProduct(), Times.Once);
            }

            [Fact]
            public async Task UTCID03_GetAllProductAsync()
            {
                // Arrange
                _productRepositoryMock
                    .Setup(x => x.GetAllProduct())
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetAllProductAsync());

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
                _productRepositoryMock.Verify(x => x.GetAllProduct(), Times.Once);
            }

            //=========================================================
            // GetAllProductAdminAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID02_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetAllProductAdminAsync(spec));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID03_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID04_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID05_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID06_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID07_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID08_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID09_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID10_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID11_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID12_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID13_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID14_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID15_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID16_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID17_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID18_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID19_GetAllProductAdminAsync()
            {
                // Arrange
                var spec = new ProductSpecification { Page = 1, PageSize = 10 };
                var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Royal Canin",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _productRepositoryMock
                    .Setup(x => x.GetAllProductAdminAsync(spec))
                    .ReturnsAsync((products, 1));

                // Act
                var result = await _service.GetAllProductAdminAsync(spec);

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
                var id = Guid.NewGuid();
                var productEntity = new Product
                {
                    ProductId = id,
                    ProductName = "Royal Canin"
                };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                // Act
                var result = await _service.GetProductByIdAsync(id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(id, result.ProductId);
                Assert.Equal("Royal Canin", result.ProductName);
            }

            [Fact]
            public async Task UTCID02_GetProductByIdAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetProductByIdAsync(id));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID03_GetProductByIdAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetProductByIdAsync(id));

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
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID04_AddProductAsync()
            {
                // Arrange
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
                var dto = new CreateProductDTO
                {
                    ProductName = "Royal Canin",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(true);

                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.AddProductAsync(dto));

                // Assert
                Assert.Equal("Product already exists", ex.Message);
            }

            [Fact]
            public async Task UTCID06_AddProductAsync()
            {
                // Arrange
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
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID09_AddProductAsync()
            {
                // Arrange
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
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID11_AddProductAsync()
            {
                // Arrange
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
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID14_AddProductAsync()
            {
                // Arrange
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID15_AddProductAsync()
            {
                // Arrange
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
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID18_AddProductAsync()
            {
                // Arrange
                var dto = new CreateProductDTO();

                // Act
                var result = Validate(dto);

                // Assert
                Assert.NotEmpty(result);
            }

            [Fact]
            public async Task UTCID19_AddProductAsync()
            {
                // Arrange
                var dto = new CreateProductDTO
                {
                    ProductName = "New Product",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<CreateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddProductAsync(dto);

                // Assert
                _productRepositoryMock.Verify(x => x.AddProductAsync(It.IsAny<Product>()), Times.Once);
            }

            //=========================================================
            // UpdateProductAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID02_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID03_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID04_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync((Product?)null);

                // Act
                var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateProductAsync(id, dto));

                // Assert
                Assert.Equal("Product not found", ex.Message);
            }

            [Fact]
            public async Task UTCID05_UpdateProductAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID08_UpdateProductAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "Existing",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(true);

                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.UpdateProductAsync(id, dto));

                // Assert
                Assert.Equal("Product already exists", ex.Message);
            }

            [Fact]
            public async Task UTCID10_UpdateProductAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID13_UpdateProductAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID15_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID16_UpdateProductAsync()
            {
                // Arrange
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
                var dto = new UpdateProductDTO();

                // Act
                var result = Validate(dto);

                // Assert
                Assert.NotEmpty(result);
            }

            [Fact]
            public async Task UTCID18_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.UpdateProductAsync(id, dto));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID19_UpdateProductAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };
                var productEntity = new Product { ProductId = id };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                _productRepositoryMock
                    .Setup(x => x.CheckProductExistAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<ProductAttributeCompareDTO>>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateProductAsync(id, dto);

                // Assert
                _productRepositoryMock.Verify(x => x.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
            }

            [Fact]
            public async Task UTCID22_UpdateProductAsync()
            {
                // Arrange
                var dto = new UpdateProductDTO();

                // Act
                var result = Validate(dto);

                // Assert
                Assert.NotEmpty(result);
            }

            [Fact]
            public async Task UTCID23_UpdateProductAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateProductDTO
                {
                    ProductName = "New Name",
                    BrandId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Attributes = new List<UpdateProductAttributeDTO>()
                };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.UpdateProductAsync(id, dto));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            //=========================================================
            // ChangeProductStatusAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_ChangeProductStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var productEntity = new Product { ProductId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                // Act
                await _service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _productRepositoryMock.Verify(x => x.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID02_ChangeProductStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var productEntity = new Product { ProductId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                // Act
                await _service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _productRepositoryMock.Verify(x => x.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID03_ChangeProductStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var productEntity = new Product { ProductId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync(productEntity);

                // Act
                await _service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _productRepositoryMock.Verify(x => x.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID04_ChangeProductStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ReturnsAsync((Product?)null);

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active));

                // Assert
                Assert.Equal("Product not found", ex.Message);
            }

            [Fact]
            public async Task UTCID05_ChangeProductStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _productRepositoryMock
                    .Setup(x => x.GetProductByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.ChangeProductStatusAsync(id, PetCenterAPI.Common.Status.Active));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

        }
}
