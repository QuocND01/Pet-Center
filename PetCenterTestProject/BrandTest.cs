using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Brand;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static PetCenterAPI.DTOs.Responses.Brand.BrandResposeDTO;

namespace PetCenterTestProject
{
    public class BrandTest
    {

        private readonly Mock<IBrandRepository> _brandRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;
        private readonly BrandService _service;

        public BrandTest()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BrandProfile>();
            }, NullLoggerFactory.Instance);

            _mapper = config.CreateMapper();

            _brandRepositoryMock = new Mock<IBrandRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _service = new BrandService(
                _brandRepositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);
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

        //=====================================================================
        // Function: GetAllBrand()
        // Test Requirement:
        // Verify that the function returns all active brands for customer display.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetAllBrand() returns all active brands
        /// when active brands exist.
        /// Expected:
        /// - Return active brand list.
        /// </summary>
        [Fact]
        public void UTCID01_GetAllBrand_ReturnActiveBrandList()
        {
            // Arrange
            var brands = new List<Brand>
    {
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        },
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            Status = Status.Active
        }
    }.AsQueryable();

            _brandRepositoryMock
                .Setup(x => x.GetAllBrand())
                .Returns(brands);

            // Act
            var result = _service.GetAllBrand().ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }



        /// <summary>
        /// UTCID02
        /// Verify that GetAllBrand() returns an empty list
        /// when no active brands exist.
        /// Expected:
        /// - Return empty list.
        /// </summary>
        [Fact]
        public void UTCID02_GetAllBrand_ReturnEmptyList()
        {
            // Arrange
            _brandRepositoryMock
                .Setup(x => x.GetAllBrand())
                .Returns(new List<Brand>().AsQueryable());

            // Act
            var result = _service.GetAllBrand().ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        /// <summary>
        /// UTCID03
        /// Verify that GetAllBrand() throws an exception
        /// when repository throws an exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public void UTCID03_GetAllBrand_RepositoryThrowsException()
        {
            // Arrange
            _brandRepositoryMock
                .Setup(x => x.GetAllBrand())
                .Throws(new Exception("Service Temporarily Unavailable"));

            // Act + Assert
            var ex = Assert.Throws<Exception>(() =>
                _service.GetAllBrand().ToList());

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=====================================================================
        // Function: GetAllBrandAdminAsync()
        // Test Requirement:
        // Verify that the function retrieves the paginated brand list
        // according to the search specification.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetAllBrandAdminAsync() returns paged brand list
        /// when keyword is empty and brands exist.
        /// Expected:
        /// - Return paged brand list.
        /// </summary>
        [Fact]
        public async Task UTCID01_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenKeywordEmpty()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Search = "",
                Page = 1,
                PageSize = 10
            };

            var brands = new List<Brand>
    {
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        },
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            Status = Status.Inactive
        }
    };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ReturnsAsync((brands, 2));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
        }

        /// <summary>
        /// UTCID02
        /// Verify that GetAllBrandAdminAsync() returns paged brand list
        /// when keyword matches an existing brand.
        /// Expected:
        /// - Return matching brand.
        /// </summary>
        [Fact]
        public async Task UTCID02_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenKeywordExists()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Search = "Royal",
                Page = 1,
                PageSize = 10
            };

            var brands = new List<Brand>
    {
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        }
    };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        /// <summary>
        /// UTCID03
        /// Verify that GetAllBrandAdminAsync() returns empty paged list
        /// when keyword does not match any brand.
        /// Expected:
        /// - Return empty paged list.
        /// </summary>
        [Fact]
        public async Task UTCID03_GetAllBrandAdminAsync_ReturnEmptyPagedList_WhenKeywordNotExists()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Search = "ABCXYZ",
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ReturnsAsync((new List<Brand>(), 0));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        /// <summary>
        /// UTCID04
        /// Verify that GetAllBrandAdminAsync() returns only active brands.
        /// Expected:
        /// - Return paged active brand list.
        /// </summary>
        [Fact]
        public async Task UTCID04_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenStatusActive()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Status = Status.Active,
                Page = 1,
                PageSize = 10
            };

            var brands = new List<Brand>
    {
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        }
    };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(Status.Active, result.Data.First().Status);
        }

        /// <summary>
        /// UTCID05
        /// Verify that GetAllBrandAdminAsync() returns only inactive brands.
        /// Expected:
        /// - Return paged inactive brand list.
        /// </summary>
        [Fact]
        public async Task UTCID05_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenStatusInactive()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Status = Status.Inactive,
                Page = 1,
                PageSize = 10
            };

            var brands = new List<Brand>
    {
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            Status = Status.Inactive
        }
    };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(Status.Inactive, result.Data.First().Status);
        }

        /// <summary>
        /// UTCID06
        /// Verify that GetAllBrandAdminAsync() throws exception
        /// when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public async Task UTCID06_GetAllBrandAdminAsync_RepositoryThrowsException()
        {
            // Arrange
            var spec = new BrandSpecification
            {
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAllBrandAdminAsync(spec));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=====================================================================
        // Function: GetBrandByIdAsync()
        // Test Requirement:
        // Verify that the function retrieves brand information by BrandId.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetBrandByIdAsync() returns brand information
        /// when BrandId exists.
        /// Expected:
        /// - Return brand information.
        /// </summary>
        [Fact]
        public async Task UTCID01_GetBrandByIdAsync_ReturnBrandInformation_WhenBrandExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                BrandDescription = "Pet Food",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            // Act
            var result = await _service.GetBrandByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.BrandId);
            Assert.Equal("Royal Canin", result.BrandName);
        }

        /// <summary>
        /// UTCID02
        /// Verify that GetBrandByIdAsync() throws KeyNotFoundException
        /// when BrandId does not exist.
        /// Expected:
        /// - Throw KeyNotFoundException.
        /// - Message = "Brand not found."
        /// </summary>
        [Fact]
        public async Task UTCID02_GetBrandByIdAsync_BrandNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetBrandByIdAsync(id));

            Assert.Equal("Brand not found.", ex.Message);
        }


        /// <summary>
        /// UTCID03
        /// Verify that GetBrandByIdAsync() throws exception
        /// when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public async Task UTCID03_GetBrandByIdAsync_RepositoryThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetBrandByIdAsync(id));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=====================================================================
        // Function: AddBrandAsync()
        // Test Requirement:
        // Verify that the function adds a new brand correctly.
        //=====================================================================
        /// <summary>
        /// UTCID01
        /// Verify AddBrandAsync() when BrandName is null.
        /// Expected:
        /// - Validation error.
        /// - Message = "Brand name is required".
        /// </summary>
        [Fact]
        public void UTCID01_CreateBrandDTO_BrandNameNull_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = null!,
                BrandDescription = ""
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name is required");
        }

        /// <summary>
        /// UTCID02
        /// Verify AddBrandAsync() when BrandName is empty.
        /// Expected:
        /// - Validation error.
        /// - Message = "Brand name is required".
        /// </summary>
        [Fact]
        public void UTCID02_CreateBrandDTO_BrandNameEmpty_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = ""
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name is required");
        }

        /// <summary>
        /// UTCID03
        /// Verify AddBrandAsync() when BrandName exceeds 200 characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID03_CreateBrandDTO_BrandNameTooLong_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 201)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name cannot exceed 200 characters");
        }

        /// <summary>
        /// UTCID04
        /// Verify AddBrandAsync() when BrandName contains special characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID04_CreateBrandDTO_BrandNameContainsSpecialCharacter_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal@Canin"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name cannot contain special characters");
        }



        [Fact]
        public async Task UTCID05_AddBrandAsync_ShouldAddBrandSuccessfully()
        {
            // Arrange
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Pet Food"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            _brandRepositoryMock
                .Setup(x => x.AddBrandAsync(It.IsAny<Brand>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddBrandAsync(dto);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }



        [Fact]
        public async Task UTCID06_AddBrandAsync_ShouldThrow_WhenBrandAlreadyExists()
        {
            // Arrange
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(true);

            // Act

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddBrandAsync(dto));

            // Assert

            Assert.Equal("Brand already exists", ex.Message);
        }


        [Fact]
        public async Task UTCID07_AddBrandAsync_UploadImageSuccess_ShouldAddBrand()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            var file = new FormFile(stream, 0, stream.Length, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Pet food",
                BrandLogo = file
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            var uploadResult = new ImageUploadResult
            {
                StatusCode = HttpStatusCode.OK,
                PublicId = "brand/logo1",
                SecureUrl = new Uri("https://res.cloudinary.com/demo/image/upload/logo.jpg")
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "brands"))
                .ReturnsAsync(uploadResult);

            _brandRepositoryMock
                .Setup(x => x.AddBrandAsync(It.IsAny<Brand>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddBrandAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(It.IsAny<IFormFile>(), "brands"),
                Times.Once);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }


        [Fact]
        public async Task UTCID08_AddBrandAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            var file = new FormFile(stream, 0, stream.Length, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandLogo = file
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddBrandAsync(dto));

            // Assert
            Assert.Equal("Failed to upload brand logo", ex.Message);
        }


        /// <summary>
        /// UTCID09
        /// Verify AddBrandAsync() when description exceeds 2000 characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID09_CreateBrandDTO_DescriptionTooLong_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2001)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Description cannot exceed 2000 characters");
        }

        /// <summary>
        /// UTCID10
        /// Verify AddBrandAsync() when description length equals 2000.
        /// Expected:
        /// - Brand is added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID10_AddBrandAsync_DescriptionLength2000_ShouldSuccess()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID11
        /// Verify AddBrandAsync() when BrandName length equals 200.
        /// Expected:
        /// - Brand is added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID11_AddBrandAsync_BrandNameLength200_ShouldSuccess()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID12
        /// Verify AddBrandAsync() when BrandName length equals 200
        /// and description is empty.
        /// Expected:
        /// - Brand is added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID12_AddBrandAsync_BrandName200AndDescriptionEmpty_ShouldSuccess()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 200),
                BrandDescription = ""
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID13
        /// Verify AddBrandAsync() when description is valid.
        /// Expected:
        /// - Brand is added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID13_AddBrandAsync_ValidDescription_ShouldSuccess()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "High quality pet food."
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, null))
                .ReturnsAsync(false);

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }


        //=====================================================================
        // Function: UpdateBrandAsync()
        // Test Requirement:
        // Verify that UpdateBrandAsync() updates brand information correctly.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify UpdateBrandAsync() when BrandName is null.
        /// Expected:
        /// - Validation error.
        /// - Message = "Brand name is required".
        /// </summary>
        [Fact]
        public void UTCID01_UpdateBrandDTO_BrandNameNull_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = null!
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name is required");
        }

        /// <summary>
        /// UTCID02
        /// Verify UpdateBrandAsync() when BrandName is empty.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID02_UpdateBrandDTO_BrandNameEmpty_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = ""
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name is required");
        }


        /// <summary>
        /// UTCID03
        /// Verify UpdateBrandAsync() when BrandName exceeds 200 characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID03_UpdateBrandDTO_BrandNameTooLong_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = new string('A', 201)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name cannot exceed 200 characters");
        }

        /// <summary>
        /// UTCID04
        /// Verify UpdateBrandAsync() when BrandName contains special characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID04_UpdateBrandDTO_SpecialCharacter_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal@Canin"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name cannot contain special characters");
        }


        /// <summary>
        /// UTCID05
        /// Verify UpdateBrandAsync() when BrandId does not exist.
        /// Expected:
        /// - Throw KeyNotFoundException.
        /// </summary>
        [Fact]
        public async Task UTCID05_UpdateBrandAsync_BrandNotFound_ShouldThrowKeyNotFoundException()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Brand not found", ex.Message);
        }

        /// <summary>
        /// UTCID06
        /// Verify UpdateBrandAsync() when BrandName already exists.
        /// Expected:
        /// - Throw InvalidOperationException.
        /// </summary>
        [Fact]
        public async Task UTCID06_UpdateBrandAsync_DuplicateBrand_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old Brand"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Brand already exists", ex.Message);
        }

        /// <summary>
        /// UTCID07
        /// Verify UpdateBrandAsync() when uploading a new logo succeeds.
        /// Expected:
        /// - Brand updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID07_UpdateBrandAsync_UploadImageSuccess_ShouldUpdateSuccessfully()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                PublicId = ""
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Updated",
                BrandLogo = file
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            var upload = new CloudinaryDotNet.Actions.ImageUploadResult
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                SecureUrl = new Uri("https://demo.com/logo.jpg"),
                PublicId = "logo123"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(upload);

            await _service.UpdateBrandAsync(id, dto);

            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID08
        /// Verify UpdateBrandAsync() when uploading logo fails.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Failed to upload brand logo".
        /// </summary>
        [Fact]
        public async Task UTCID08_UpdateBrandAsync_UploadImageFail_ShouldThrowException()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal"
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new CloudinaryDotNet.Actions.ImageUploadResult());

            var ex = await Assert.ThrowsAsync<Exception>(
                () => _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Failed to upload brand logo", ex.Message);
        }


        /// <summary>
        /// UTCID09
        /// Verify UpdateBrandAsync() when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public async Task UTCID09_UpdateBrandAsync_RepositoryThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        /// <summary>
        /// UTCID10
        /// Verify UpdateBrandDTO when description exceeds 2000 characters.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID10_UpdateBrandDTO_DescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2001)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage == "Description cannot exceed 2000 characters");
        }

        /// <summary>
        /// UTCID11
        /// Verify UpdateBrandAsync() when BrandName length equals 200.
        /// Expected:
        /// - Brand updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID11_UpdateBrandAsync_BrandNameLength200_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateBrandAsync(id, dto);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID12
        /// Verify UpdateBrandAsync() when description length equals 2000.
        /// Expected:
        /// - Brand updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID12_UpdateBrandAsync_DescriptionLength2000_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateBrandAsync(id, dto);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID13
        /// Verify UpdateBrandAsync() deletes old logo
        /// when existing brand already has logo.
        /// Expected:
        /// - Upload new logo.
        /// - Delete old logo.
        /// - Update successfully.
        /// </summary>
        [Fact]
        public async Task UTCID13_UpdateBrandAsync_DeleteOldLogo_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                PublicId = "old-public-id"
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Updated",
                BrandLogo = file
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            var uploadResult = new CloudinaryDotNet.Actions.ImageUploadResult
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                SecureUrl = new Uri("https://demo.com/logo.jpg"),
                PublicId = "new-public-id"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(uploadResult);

            _cloudinaryServiceMock
                .Setup(x => x.DeleteImageAsync("old-public-id"))
                .ReturnsAsync(new CloudinaryDotNet.Actions.DeletionResult());

            // Act
            await _service.UpdateBrandAsync(id, dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.DeleteImageAsync("old-public-id"),
                Times.Once);

            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID14
        /// Verify UpdateBrandAsync() when description is valid.
        /// Expected:
        /// - Brand updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID14_UpdateBrandAsync_ValidDescription_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = "High quality pet food."
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateBrandAsync(id, dto);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }

        /// <summary>
        /// UTCID15
        /// Verify UpdateBrandAsync() with valid BrandName
        /// and valid Description.
        /// Expected:
        /// - Brand updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID15_UpdateBrandAsync_ValidData_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old Brand"
            };

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Premium pet food."
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(dto.BrandName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateBrandAsync(id, dto);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.IsAny<Brand>()),
                Times.Once);
        }


        //=====================================================================
        // Function: ChangeBrandStatusAsync()
        // Test Requirement:
        // Verify that ChangeBrandStatusAsync() changes brand status correctly.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify ChangeBrandStatusAsync() changes status to Active.
        /// Expected:
        /// - Brand status changed successfully.
        /// </summary>
        [Fact]
        public async Task UTCID01_ChangeBrandStatusAsync_Active_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                Status = Status.Inactive
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            // Act
            await _service.ChangeBrandStatusAsync(id, Status.Active);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Active),
                Times.Once);
        }

        /// <summary>
        /// UTCID02
        /// Verify ChangeBrandStatusAsync() changes status to Inactive.
        /// Expected:
        /// - Brand status changed successfully.
        /// </summary>
        [Fact]
        public async Task UTCID02_ChangeBrandStatusAsync_Inactive_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            // Act
            await _service.ChangeBrandStatusAsync(id, Status.Inactive);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Inactive),
                Times.Once);
        }

        /// <summary>
        /// UTCID03
        /// Verify ChangeBrandStatusAsync() changes status to Deleted.
        /// Expected:
        /// - Brand status changed successfully.
        /// </summary>
        [Fact]
        public async Task UTCID03_ChangeBrandStatusAsync_Deleted_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            // Act
            await _service.ChangeBrandStatusAsync(id, Status.Deleted);

            // Assert
            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Deleted),
                Times.Once);
        }

        /// <summary>
        /// UTCID04
        /// Verify ChangeBrandStatusAsync() throws exception
        /// when BrandId does not exist.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Brand not found".
        /// </summary>
        [Fact]
        public async Task UTCID04_ChangeBrandStatusAsync_BrandNotFound_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeBrandStatusAsync(id, Status.Active));

            Assert.Equal("Brand not found", ex.Message);

            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(It.IsAny<Guid>(), It.IsAny<Status>()),
                Times.Never);
        }

        /// <summary>
        /// UTCID05
        /// Verify ChangeBrandStatusAsync() throws exception
        /// when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public async Task UTCID05_ChangeBrandStatusAsync_RepositoryThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.ChangeBrandStatusAsync(id, Status.Active))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeBrandStatusAsync(id, Status.Active));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

    }
}