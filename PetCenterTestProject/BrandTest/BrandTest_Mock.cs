using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Responses.Brand.BrandResposeDTO;

namespace PetCenterTestProject.BrandTest
{
    public class BrandTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================

        private readonly Mock<IBrandRepository> _brandRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        private readonly IMapper _mapper;
        private readonly BrandService _service;

        //=========================================================
        // Constructor
        //=========================================================

        public BrandTest_Mock()
        {
            _brandRepositoryMock = new Mock<IBrandRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BrandProfile>();
            }, NullLoggerFactory.Instance)
            .CreateMapper();

            _service = new BrandService(
                _brandRepositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        //=========================================================
        // DTO Validation
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
            BrandDescription = "Premium pet food",
            BrandLogo = "logo1.jpg",
            Status = Status.Active
        },
        new Brand
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            BrandDescription = "Dog food",
            BrandLogo = "logo2.jpg",
            Status = Status.Active
        }
    }.AsQueryable();

            _brandRepositoryMock
                .Setup(x => x.GetAllBrand())
                .Returns(brands);

            // Act
            var result = _service.GetAllBrand().ToList();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(brands.First().BrandId, result.First().BrandId);
            Assert.Equal(brands.First().BrandName, result.First().BrandName);
            Assert.Equal(brands.First().BrandDescription, result.First().BrandDescription);
            Assert.Equal(brands.First().BrandLogo, result.First().BrandLogo);
        }


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


        [Fact]
        public void UTCID03_GetAllBrand_RepositoryThrowsException()
        {
            // Arrange
            _brandRepositoryMock
                .Setup(x => x.GetAllBrand())
                .Throws(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = Assert.Throws<Exception>(() =>
                _service.GetAllBrand().ToList());

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        [Fact]
        public async Task UTCID01_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenKeywordEmpty()
        {
            // Arrange
            var brands = new List<Brand>
    {
        new()
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        },
        new()
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            Status = Status.Inactive
        }
    };

            var spec = new BrandSpecification
            {
                Search = "",
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ReturnsAsync((brands, 2));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task UTCID02_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenKeywordExists()
        {
            // Arrange
            var brands = new List<Brand>
    {
        new()
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal Canin",
            Status = Status.Active
        }
    };

            var spec = new BrandSpecification
            {
                Search = "Royal",
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Royal Canin", result.Data.First().BrandName);
            Assert.Equal(1, result.TotalCount);
        }


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
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ReturnsAsync((new List<Brand>(), 0));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }


        [Fact]
        public async Task UTCID04_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenStatusActive()
        {
            // Arrange
            var brands = new List<Brand>
    {
        new()
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Royal",
            Status = Status.Active
        }
    };

            var spec = new BrandSpecification
            {
                Status = Status.Active,
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(Status.Active, result.Data.First().Status);
        }


        [Fact]
        public async Task UTCID05_GetAllBrandAdminAsync_ReturnPagedBrandList_WhenStatusInactive()
        {
            // Arrange
            var brands = new List<Brand>
    {
        new()
        {
            BrandId = Guid.NewGuid(),
            BrandName = "Pedigree",
            Status = Status.Inactive
        }
    };

            var spec = new BrandSpecification
            {
                Status = Status.Inactive,
                Page = 1,
                PageSize = 10
            };

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ReturnsAsync((brands, 1));

            // Act
            var result = await _service.GetAllBrandAdminAsync(spec);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(Status.Inactive, result.Data.First().Status);
        }


        [Fact]
        public async Task UTCID06_GetAllBrandAdminAsync_RepositoryThrowsException()
        {
            // Arrange
            var spec = new BrandSpecification();

            _brandRepositoryMock
                .Setup(x => x.GetAllBrandAdminAsync(spec))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAllBrandAdminAsync(spec));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        [Fact]
        public async Task UTCID01_GetBrandByIdAsync_ExistingGuid_ShouldReturnBrand()
        {
            // Arrange
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal Canin",
                BrandDescription = "Premium pet food",
                BrandLogo = "logo.jpg",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            // Act
            var result = await _service.GetBrandByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.BrandId);
            Assert.Equal("Royal Canin", result.BrandName);
            Assert.Equal("Premium pet food", result.BrandDescription);
            Assert.Equal("logo.jpg", result.BrandLogo);
            Assert.Equal(Status.Active, result.Status);
        }


        [Fact]
        public async Task UTCID02_GetBrandByIdAsync_NonExistingGuid_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            // Act
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetBrandByIdAsync(id));

            // Assert
            Assert.Equal("Brand not found.", ex.Message);
        }


        [Fact]
        public async Task UTCID03_GetBrandByIdAsync_RepositoryThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetBrandByIdAsync(id));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        [Fact]
        public void UTCID01_CreateBrandDTO_BrandNameNull_ShouldFailValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = null!
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Brand name is required");
        }


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

        [Fact]
        public void UTCID04_CreateBrandDTO_SpecialCharacter_ShouldFailValidation()
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
        public void UTCID05_CreateBrandDTO_DescriptionTooLong_ShouldFailValidation()
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


        [Fact]
        public async Task UTCID06_AddBrandAsync_DuplicateBrand_ShouldThrowInvalidOperationException()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync("Royal", null))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddBrandAsync(dto));

            Assert.Equal("Brand already exists", ex.Message);
        }


        [Fact]
        public async Task UTCID07_AddBrandAsync_UploadImageSuccess_ShouldSuccess()
        {
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, stream.Length, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://demo.com/logo.jpg"),
                    PublicId = "logo123"
                });

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.Is<Brand>(b =>
                    b.BrandName == "Royal" &&
                    b.BrandLogo == "https://demo.com/logo.jpg" &&
                    b.PublicId == "logo123")),
                Times.Once);
        }


        [Fact]
        public async Task UTCID08_AddBrandAsync_UploadImageFail_ShouldThrowException()
        {
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, stream.Length, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddBrandAsync(dto));

            Assert.Equal("Failed to upload brand logo", ex.Message);
        }

        [Fact]
        public async Task UTCID09_AddBrandAsync_RepositoryThrowsException()
        {
            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            _brandRepositoryMock
                .Setup(x => x.AddBrandAsync(It.IsAny<Brand>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal"
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddBrandAsync(dto));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        [Fact]
        public async Task UTCID10_AddBrandAsync_InvalidExtension_ShouldThrowException()
        {
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.gif")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/gif"
            };

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddBrandAsync(dto));

            Assert.Equal("Only JPG, JPEG, PNG, and WEBP images are allowed.", ex.Message);
        }


        [Fact]
        public void UTCID11_CreateBrandDTO_BrandNameLength200_ShouldPassValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }


        [Fact]
        public void UTCID12_CreateBrandDTO_DescriptionLength2000_ShouldPassValidation()
        {
            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }


        [Fact]
        public async Task UTCID13_AddBrandAsync_ValidData_ShouldSuccess()
        {
            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = "Premium pet food"
            };

            await _service.AddBrandAsync(dto);

            _brandRepositoryMock.Verify(
                x => x.AddBrandAsync(It.Is<Brand>(b =>
                    b.BrandName == "Royal" &&
                    b.BrandDescription == "Premium pet food")),
                Times.Once);
        }


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

        [Fact]
        public void UTCID03_UpdateBrandDTO_BrandNameTooLong_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = new string('A', 201)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Brand name cannot exceed 200 characters");
        }

        [Fact]
        public void UTCID04_UpdateBrandDTO_SpecialCharacter_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal@Canin"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Brand name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID05_UpdateBrandAsync_BrandNotFound_ShouldThrowKeyNotFoundException()
        {
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal"
            };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Brand not found", ex.Message);
        }


        [Fact]
        public async Task UTCID06_UpdateBrandAsync_DuplicateBrand_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(new Brand
                {
                    BrandId = id,
                    BrandName = "Old"
                });

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync("Royal", id))
                .ReturnsAsync(true);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Brand already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID07_UpdateBrandAsync_UploadImageSuccess_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), id))
                .ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://demo.com/logo.jpg"),
                    PublicId = "logo123"
                });

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            await _service.UpdateBrandAsync(id, dto);

            _brandRepositoryMock.Verify(x =>
                x.UpdateBrandAsync(It.Is<Brand>(b =>
                    b.BrandLogo == "https://demo.com/logo.jpg" &&
                    b.PublicId == "logo123")),
                Times.Once);
        }


        [Fact]
        public async Task UTCID08_UpdateBrandAsync_UploadImageFail_ShouldThrowException()
        {
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(new Brand());

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), id))
                .ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Failed to upload brand logo", ex.Message);
        }

        [Fact]
        public async Task UTCID09_UpdateBrandAsync_RepositoryThrowsException()
        {
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal"
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateBrandAsync(id, dto));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        [Fact]
        public void UTCID10_UpdateBrandDTO_DescriptionTooLong_ShouldFailValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2001)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Description cannot exceed 2000 characters");
        }


        [Fact]
        public void UTCID11_UpdateBrandDTO_BrandNameLength200_ShouldPassValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }


        [Fact]
        public void UTCID12_UpdateBrandDTO_DescriptionLength2000_ShouldPassValidation()
        {
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }


        [Fact]
        public async Task UTCID13_UpdateBrandAsync_DeleteOldLogo_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old",
                PublicId = "old-public-id"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), id))
                .ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "BrandLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://demo.com/logo.jpg"),
                    PublicId = "new-public-id"
                });

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandLogo = file
            };

            await _service.UpdateBrandAsync(id, dto);

            _cloudinaryServiceMock.Verify(
                x => x.DeleteImageAsync("old-public-id"),
                Times.Once);
        }


        [Fact]
        public async Task UTCID14_UpdateBrandAsync_ValidDescription_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), id))
                .ReturnsAsync(false);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = "Premium pet food."
            };

            await _service.UpdateBrandAsync(id, dto);

            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.Is<Brand>(b =>
                    b.BrandDescription == "Premium pet food.")),
                Times.Once);
        }


        [Fact]
        public async Task UTCID15_UpdateBrandAsync_ValidData_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Old"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.CheckBrandExistAsync(It.IsAny<string>(), id))
                .ReturnsAsync(false);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Premium pet food."
            };

            await _service.UpdateBrandAsync(id, dto);

            _brandRepositoryMock.Verify(
                x => x.UpdateBrandAsync(It.Is<Brand>(b =>
                    b.BrandName == "Royal Canin" &&
                    b.BrandDescription == "Premium pet food.")),
                Times.Once);
        }



        [Fact]
        public async Task UTCID01_ChangeBrandStatusAsync_Active_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                Status = Status.Inactive
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.ChangeBrandStatusAsync(id, Status.Active))
                .Returns(Task.CompletedTask);

            await _service.ChangeBrandStatusAsync(id, Status.Active);

            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Active),
                Times.Once);
        }


        [Fact]
        public async Task UTCID02_ChangeBrandStatusAsync_Inactive_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.ChangeBrandStatusAsync(id, Status.Inactive))
                .Returns(Task.CompletedTask);

            await _service.ChangeBrandStatusAsync(id, Status.Inactive);

            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Inactive),
                Times.Once);
        }


        [Fact]
        public async Task UTCID03_ChangeBrandStatusAsync_Deleted_ShouldSuccess()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                Status = Status.Active
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.ChangeBrandStatusAsync(id, Status.Deleted))
                .Returns(Task.CompletedTask);

            await _service.ChangeBrandStatusAsync(id, Status.Deleted);

            _brandRepositoryMock.Verify(
                x => x.ChangeBrandStatusAsync(id, Status.Deleted),
                Times.Once);
        }


        [Fact]
        public async Task UTCID04_ChangeBrandStatusAsync_BrandNotFound_ShouldThrowException()
        {
            var id = Guid.NewGuid();

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync((Brand?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeBrandStatusAsync(id, Status.Active));

            Assert.Equal("Brand not found", ex.Message);
        }


        [Fact]
        public async Task UTCID05_ChangeBrandStatusAsync_RepositoryThrowsException()
        {
            var id = Guid.NewGuid();

            var brand = new Brand
            {
                BrandId = id,
                BrandName = "Royal"
            };

            _brandRepositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ReturnsAsync(brand);

            _brandRepositoryMock
                .Setup(x => x.ChangeBrandStatusAsync(id, Status.Active))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeBrandStatusAsync(id, Status.Active));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

    }
}
