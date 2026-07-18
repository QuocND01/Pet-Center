using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System.ComponentModel.DataAnnotations;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;

namespace PetCenterTestProject
{
    public class ProductTest
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        private readonly IMapper _mapper;
        private readonly ProductService _service;

        public ProductTest()
        {
            //=========================================================
            // AutoMapper
            //=========================================================
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<ProductAttributeProfile>();

                // Thêm các profile khác nếu ProductDTO sử dụng
                // cfg.AddProfile<CategoryProfile>();
                // cfg.AddProfile<CategoryAttributeProfile>();
                // cfg.AddProfile<BrandProfile>();
            }, NullLoggerFactory.Instance);

            _mapper = config.CreateMapper();

            //=========================================================
            // Mock
            //=========================================================
            _productRepositoryMock = new Mock<IProductRepository>();

            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Fake HttpClient
            var httpClient = new HttpClient();

            _httpClientFactoryMock
                .Setup(x => x.CreateClient("OrdersAPI"))
                .Returns(httpClient);

            //=========================================================
            // Service
            //=========================================================
            _service = new ProductService(
                _productRepositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object,
                _httpClientFactoryMock.Object);
        }

        //=========================================================
        // Helper
        //=========================================================

        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);

            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                model,
                context,
                results,
                true);

            return results;
        }


        //=========================================================
        // Helper
        // Create ODataQueryOptions
        //=========================================================

        private ODataQueryOptions<ReadProductDTOForCustomer> CreateODataQueryOptions(
            string query = "")
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ReadProductDTOForCustomer>("Products");

            var model = builder.GetEdmModel();

            var context = new ODataQueryContext(
                model,
                typeof(ReadProductDTOForCustomer),
                new ODataPath());

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Method = "GET";

            httpContext.Request.QueryString = new QueryString(query);

            return new ODataQueryOptions<ReadProductDTOForCustomer>(
                context,
                httpContext.Request);
        }







        //=========================================================
        // GetAllProductAsync()
        // UTCID01 - Active products exist
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllProductAsync_ActiveProductsExist_ShouldReturnProductList()
        {
            // Arrange

            var products = new List<Product>
    {
        new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Royal Canin",
            ProductPrice = 100,
            Status = Status.Active
        },
        new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Pedigree",
            ProductPrice = 200,
            Status = Status.Active
        }
    };

            _productRepositoryMock
                .Setup(x => x.GetAllProduct())
                .ReturnsAsync(products);

            // Act

            var result = await _service.GetAllProductAsync();

            // Assert

            Assert.NotNull(result);

            Assert.Equal(2, result.Count);

            Assert.All(result,
                p => Assert.NotEqual(Guid.Empty, p.ProductId));

            _productRepositoryMock.Verify(
                x => x.GetAllProduct(),
                Times.Once);
        }


        //=========================================================
        // GetAllProductAsync()
        // UTCID02 - No active products exist
        //=========================================================

        [Fact]
        public async Task UTCID02_GetAllProductAsync_NoActiveProducts_ShouldReturnEmptyList()
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

            _productRepositoryMock.Verify(
                x => x.GetAllProduct(),
                Times.Once);
        }


        //=========================================================
        // GetAllProductAsync()
        // UTCID03 - Repository throws exception
        //=========================================================

        [Fact]
        public async Task UTCID03_GetAllProductAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange

            _productRepositoryMock
                .Setup(x => x.GetAllProduct())
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act

            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAllProductAsync());

            // Assert

            Assert.Equal(
                "Service Temporarily Unavailable",
                exception.Message);

            _productRepositoryMock.Verify(
                x => x.GetAllProduct(),
                Times.Once);
        }
    }
}