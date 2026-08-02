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
using System.ComponentModel.DataAnnotations;
using System.Net;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryAttributeResponseDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterTestProject.CategoryTest
{
    public class CategoryTest_Mock
    {
        //=========================================================
        // Mock
        //=========================================================

        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        private readonly IMapper _mapper;
        private readonly CategoryService _service;


        //=========================================================
        // Constructor
        //=========================================================

        public CategoryTest_Mock()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<CategoryAttributeProfile>();

            }, NullLoggerFactory.Instance);


            // Kiểm tra toàn bộ mapping trước khi chạy test
            mapperConfig.AssertConfigurationIsValid();


            _mapper = mapperConfig.CreateMapper();


            _service = new CategoryService(
                _categoryRepositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);
        }


        //=========================================================
        // DTO Validation
        //=========================================================

        private IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();

            var context = new ValidationContext(model);

            Validator.TryValidateObject(
                model,
                context,
                results,
                true);


            // Validate nested objects
            if (model is CreateCategoryDTO category &&
                category.Attributes != null)
            {
                foreach (var attribute in category.Attributes)
                {
                    var attributeContext = new ValidationContext(attribute);

                    Validator.TryValidateObject(
                        attribute,
                        attributeContext,
                        results,
                        true);
                }
            }

            return results;
        }

        // =========================================================
        // UTCID01
        // GetAllCategory - Active categories exist
        // Expected: Active category list
        // Result: Normal
        // =========================================================

        [Fact]
        public void UTCID01_GetAllCategory_ReturnActiveCategoryList()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Dog Food",
            Status = Status.Active
        },
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Cat Food",
            Status = Status.Active
        }
    }.AsQueryable();

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategory())
                .Returns(categories);

            // Act
            var result = _service.GetAllCategory().ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Contains(
                result,
                x => x.CategoryName == "Dog Food"
            );

            Assert.Contains(
                result,
                x => x.CategoryName == "Cat Food"
            );

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once
            );
        }


        // =========================================================
        // UTCID02
        // GetAllCategory - No categories exist
        // Expected: Empty list
        // Result: Normal
        // =========================================================

        [Fact]
        public void UTCID02_GetAllCategory_ReturnEmptyList_WhenNoCategoryExists()
        {
            // Arrange

            var categories = new List<Category>()
                .AsQueryable();

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategory())
                .Returns(categories);

            // Act

            var result = _service.GetAllCategory().ToList();

            // Assert

            Assert.NotNull(result);

            Assert.Empty(result);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once
            );
        }


        // =========================================================
        // UTCID03
        // GetAllCategory - Repository throws exception
        // Expected: Exception
        // Result: Abnormal
        // =========================================================

        [Fact]
        public void UTCID03_GetAllCategory_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange

            var exception = new Exception(
                "Service Temporarily Unavailable"
            );

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategory())
                .Throws(exception);

            // Act & Assert

            var actualException = Assert.Throws<Exception>(
                () => _service.GetAllCategory()
            );

            Assert.Equal(
                "Service Temporarily Unavailable",
                actualException.Message
            );

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once
            );
        }



        [Fact]
        public async Task UTCID01_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenKeywordEmptyAndStatusAll()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Dog Food",
            Status = Status.Active
        },
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Cat Food",
            Status = Status.Inactive
        }
    };

            var spec = new CategorySpecification
            {
                Search = "",
                Status = null,
                Page = 1,
                PageSize = 10
            };

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ReturnsAsync((categories, 2));

            // Act
            var result = await _service.GetAllCategoryAdminAsync(spec);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(2, result.Data.Count());

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "" &&
                        s.Status == null &&
                        s.Page == 1 &&
                        s.PageSize == 10)),
                Times.Once);
        }


        [Fact]
        public async Task UTCID02_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenKeywordExists()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Dog Food",
            Status = Status.Active
        }
    };

            var spec = new CategorySpecification
            {
                Search = "Dog",
                Status = null,
                Page = 1,
                PageSize = 10
            };

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ReturnsAsync((categories, 1));

            // Act
            var result = await _service.GetAllCategoryAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Single(result.Data);

            Assert.Contains(
                result.Data,
                x => x.CategoryName == "Dog Food");

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "Dog" &&
                        s.Status == null)),
                Times.Once);
        }


        [Fact]
        public async Task UTCID03_GetAllCategoryAdminAsync_ReturnEmptyPagedList_WhenKeywordNotExists()
        {
            // Arrange
            var categories = new List<Category>();

            var spec = new CategorySpecification
            {
                Search = "Bird",
                Status = null,
                Page = 1,
                PageSize = 10
            };

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ReturnsAsync((categories, 0));

            // Act
            var result = await _service.GetAllCategoryAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
            Assert.Empty(result.Data);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "Bird" &&
                        s.Status == null)),
                Times.Once);
        }


        [Fact]
        public async Task UTCID04_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenStatusActive()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Dog Food",
            Status = Status.Active
        },
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Cat Food",
            Status = Status.Active
        }
    };

            var spec = new CategorySpecification
            {
                Search = "",
                Status = Status.Active,
                Page = 1,
                PageSize = 10
            };

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ReturnsAsync((categories, 2));

            // Act
            var result = await _service.GetAllCategoryAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(2, result.Data.Count());

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "" &&
                        s.Status == Status.Active)),
                Times.Once);
        }


        [Fact]
        public async Task UTCID05_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenStatusInactive()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Cat Food",
            Status = Status.Inactive
        }
    };

            var spec = new CategorySpecification
            {
                Search = "",
                Status = Status.Inactive,
                Page = 1,
                PageSize = 10
            };

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ReturnsAsync((categories, 1));

            // Act
            var result = await _service.GetAllCategoryAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Single(result.Data);

            Assert.Contains(
                result.Data,
                x => x.CategoryName == "Cat Food");

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "" &&
                        s.Status == Status.Inactive)),
                Times.Once);
        }


        [Fact]
        public async Task UTCID06_GetAllCategoryAdminAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            var spec = new CategorySpecification
            {
                Search = "",
                Status = null,
                Page = 1,
                PageSize = 10
            };

            var exception = new Exception(
                "Service Temporarily Unavailable");

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(
                    It.IsAny<CategorySpecification>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var actualException =
                await Assert.ThrowsAsync<Exception>(
                    () => _service.GetAllCategoryAdminAsync(spec));

            Assert.Equal(
                "Service Temporarily Unavailable",
                actualException.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(
                    It.Is<CategorySpecification>(s =>
                        s.Search == "" &&
                        s.Status == null)),
                Times.Once);
        }



        [Fact]
        public async Task UTCID01_GetCategoryByIdAsync_ReturnCategoryInformation_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Dog Food",
                Status = Status.Active
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal("Dog Food", result.CategoryName);

            _categoryRepositoryMock.Verify(
                x => x.GetCategoryByIdAsync(categoryId),
                Times.Once);
        }



        [Fact]
        public async Task UTCID02_GetCategoryByIdAsync_ThrowsKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetCategoryByIdAsync(categoryId));

            // Assert
            Assert.Equal(
                "Category not found",
                exception.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetCategoryByIdAsync(categoryId),
                Times.Once);
        }



        [Fact]
        public async Task UTCID03_GetCategoryByIdAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            var exception = new Exception(
                "Service Temporarily Unavailable");

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ThrowsAsync(exception);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<Exception>(
                () => _service.GetCategoryByIdAsync(categoryId));

            Assert.Equal(
                "Service Temporarily Unavailable",
                actualException.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetCategoryByIdAsync(categoryId),
                Times.Once);
        }


        [Fact]
        public void UTCID01_AddCategoryAsync_CategoryNameNull_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = null!,
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void UTCID02_AddCategoryAsync_CategoryNameEmpty_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "",
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void UTCID03_AddCategoryAsync_CategoryNameLengthGreaterThan200_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = new string('A', 201),
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                     "Category name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID04_AddCategoryAsync_CategoryNameLength200_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = new string('A', 200),
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryName == new string('A', 200))),
                Times.Once);
        }

        [Fact]
        public void UTCID05_AddCategoryAsync_CategoryNameContainsSpecialCharacter_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog@Food",
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                     "Category name cannot contain special characters");
        }


        [Fact]
        public async Task UTCID06_AddCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName,null))
                .ReturnsAsync(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Category already exists", ex.Message);

            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public void UTCID07_AddCategoryAsync_DescriptionLengthGreaterThan2000_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2001),
                CategoryLogo = null,
                Attributes = []
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                     "Description cannot exceed 2000 characters");
        }

        [Fact]
        public async Task UTCID08_AddCategoryAsync_DescriptionLength2000_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2000),
                CategoryLogo = null,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryDescription == new string('A', 2000))),
                Times.Once);
        }

        [Fact]
        public async Task UTCID09_AddCategoryAsync_ValidDescription_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = "Premium food for dogs",
                CategoryLogo = null,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryDescription == "Premium food for dogs")),
                Times.Once);
        }

        [Fact]
        public async Task UTCID10_AddCategoryAsync_LogoNull_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = "",
                CategoryLogo = null,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(x =>
                x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never);

            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryLogo == null &&
                    c.PublicId == null)),
                Times.Once);
        }

        [Fact]
        public async Task UTCID11_AddCategoryAsync_UploadImageSuccess_ShouldSuccess()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, stream.Length,
                "CategoryLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://demo.com/category.jpg"),
                    PublicId = "category123"
                });

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(x =>
                x.UploadImageAsync(file, "categories"),
                Times.Once);

            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryLogo == "https://demo.com/category.jpg" &&
                    c.PublicId == "category123")),
                Times.Once);
        }

        [Fact]
        public async Task UTCID12_AddCategoryAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, stream.Length,
                "CategoryLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file,
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Failed to upload category logo", ex.Message);

            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID13_AddCategoryAsync_AttributeNameNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = null
            }
                ]
            };

            Assert.Contains(Validate(dto),
                x => x.ErrorMessage == "Category Attribute name is required");

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Attribute name is required.", ex.Message);
        }

        [Fact]
        public void UTCID14_AddCategoryAsync_AttributeNameGreaterThan200_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = new string('A',201)
            }
                ]
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                     "Category Attribute name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID15_AddCategoryAsync_AttributeNameLength200_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = new string('A',200)
            }
                ]
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryAttributes.Count == 1 &&
                    c.CategoryAttributes.First().AttributeName == new string('A', 200))),
                Times.Once);
        }



        [Fact]
        public void UTCID16_AddCategoryAsync_AttributeNameContainsSpecialCharacter_ShouldReturnValidationError()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = "Color@Red"
            }
                ]
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                     "Category Attribute name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID17_AddCategoryAsync_DuplicateAttributes_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = "Color"
            },
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Color"
            }
                ]
            };

            Assert.Empty(Validate(dto));

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Duplicate attributes: color", ex.Message);

            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID18_AddCategoryAsync_ValidAttributes_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes =
                [
                    new CreateCategoryAttributeDTO
            {
                AttributeName = "Color"
            },
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Weight"
            },
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Age"
            }
                ]
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(x =>
                x.AddCategoryAsync(It.Is<Category>(c =>
                    c.CategoryAttributes.Count == 3 &&
                    c.CategoryAttributes.Any(a => a.AttributeName == "Color") &&
                    c.CategoryAttributes.Any(a => a.AttributeName == "Weight") &&
                    c.CategoryAttributes.Any(a => a.AttributeName == "Age"))),
                Times.Once);
        }

        [Fact]
        public async Task UTCID19_AddCategoryAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = []
            };

            Assert.Empty(Validate(dto));

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(),null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }





























        [Fact]
        public async Task UTCID01_UpdateCategoryAsync_CategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            var id = Guid.NewGuid();
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync((Category?)null);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task UTCID02_UpdateCategoryAsync_ExistingCategoryHasLogo_UploadSuccess_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old Name", PublicId = "old-public-id" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/logo.jpg"), PublicId = "new-public-id" });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };

            await _service.UpdateCategoryAsync(id, dto);

            _cloudinaryServiceMock.Verify(x => x.DeleteImageAsync("old-public-id"), Times.Once);
            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryLogo == "https://demo.com/logo.jpg" && c.PublicId == "new-public-id")), Times.Once);
        }

        [Fact]
        public async Task UTCID03_UpdateCategoryAsync_ExistingAttributeRemoved_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var attrId1 = Guid.NewGuid();
            var category = new Category { 
                CategoryId = id, 
                CategoryName = "Old Name",
                CategoryAttributes = new List<CategoryAttribute> { 
                    new CategoryAttribute { CategoryAttributeId = attrId1, AttributeName = "Color", IsActive = true } 
                }
            };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", Attributes = new List<UpdateCategoryAttributeDTO>() };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryAttributes.First(a => a.CategoryAttributeId == attrId1).IsActive == false)), Times.Once);
        }

        [Fact]
        public async Task UTCID04_UpdateCategoryAsync_ValidAttributes_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var attrId1 = Guid.NewGuid();
            var category = new Category { 
                CategoryId = id, 
                CategoryName = "Old Name",
                CategoryAttributes = new List<CategoryAttribute> { 
                    new CategoryAttribute { CategoryAttributeId = attrId1, AttributeName = "Color", IsActive = true } 
                }
            };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> {
                    new UpdateCategoryAttributeDTO { CategoryAttributeId = attrId1, AttributeName = "Size" }
                } 
            };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryAttributes.First(a => a.CategoryAttributeId == attrId1).AttributeName == "Size")), Times.Once);
        }

        [Fact]
        public async Task UTCID05_UpdateCategoryAsync_NewValidAttributeAdded_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { 
                CategoryId = id, 
                CategoryName = "Old Name",
                CategoryAttributes = new List<CategoryAttribute>()
            };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> {
                    new UpdateCategoryAttributeDTO { AttributeName = "Weight" }
                } 
            };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryAttributes.Any(a => a.AttributeName == "Weight" && a.IsActive == true))), Times.Once);
        }

        [Fact]
        public async Task UTCID06_UpdateCategoryAsync_RepositoryThrowsException()
        {
            var id = Guid.NewGuid();
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        [Fact]
        public void UTCID07_UpdateCategoryDTO_CategoryNameNull_ShouldFailValidation()
        {
            var dto = new UpdateCategoryDTO { CategoryName = null! };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void UTCID08_UpdateCategoryDTO_CategoryNameTooLong_ShouldFailValidation()
        {
            var dto = new UpdateCategoryDTO { CategoryName = new string('A', 201) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID09_UpdateCategoryAsync_CategoryNameLength200_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { CategoryName = new string('A', 200) };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryName == new string('A', 200))), Times.Once);
        }

        [Fact]
        public void UTCID10_UpdateCategoryDTO_CategoryNameSpecialCharacter_ShouldFailValidation()
        {
            var dto = new UpdateCategoryDTO { CategoryName = "Dog@Food" };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID11_UpdateCategoryAsync_CategoryNameValid_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryName == "Dog Food")), Times.Once);
        }

        [Fact]
        public async Task UTCID12_UpdateCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync("Dog Food", id)).ReturnsAsync(true);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Category already exists", ex.Message);
        }

        [Fact]
        public void UTCID13_UpdateCategoryDTO_DescriptionTooLong_ShouldFailValidation()
        {
            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = new string('A', 2001) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Description cannot exceed 2000 characters");
        }

        [Fact]
        public async Task UTCID14_UpdateCategoryAsync_DescriptionLength2000_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = new string('A', 2000) };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryDescription == new string('A', 2000))), Times.Once);
        }

        [Fact]
        public async Task UTCID15_UpdateCategoryAsync_DescriptionValid_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = "Valid description" };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryDescription == "Valid description")), Times.Once);
        }

        [Fact]
        public async Task UTCID16_UpdateCategoryAsync_CategoryLogoExists_UploadSuccess_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/logo.jpg"), PublicId = "new-public-id" });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryLogo == "https://demo.com/logo.jpg" && c.PublicId == "new-public-id")), Times.Once);
        }

        [Fact]
        public async Task UTCID17_UpdateCategoryAsync_CategoryLogoExists_UploadFail_ShouldThrowException()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.BadRequest });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Failed to upload category logo", ex.Message);
        }

        [Fact]
        public void UTCID18_UpdateCategoryAttributeDTO_AttributeNameNull_ShouldFailValidation()
        {
            var dto = new UpdateCategoryAttributeDTO { AttributeName = null! };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name is required");
        }

        [Fact]
        public void UTCID19_UpdateCategoryAttributeDTO_AttributeNameTooLong_ShouldFailValidation()
        {
            var dto = new UpdateCategoryAttributeDTO { AttributeName = new string('A', 201) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID20_UpdateCategoryAsync_AttributeNameLength200_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.CheckCategoryExistAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> {
                    new UpdateCategoryAttributeDTO { AttributeName = new string('A', 200) }
                }
            };

            await _service.UpdateCategoryAsync(id, dto);

            _categoryRepositoryMock.Verify(x => x.UpdateCategoryAsync(It.Is<Category>(c => c.CategoryAttributes.Any(a => a.AttributeName == new string('A', 200)))), Times.Once);
        }

        [Fact]
        public void UTCID21_UpdateCategoryAttributeDTO_AttributeNameSpecialCharacter_ShouldFailValidation()
        {
            var dto = new UpdateCategoryAttributeDTO { AttributeName = "Size@" };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID22_UpdateCategoryAsync_DuplicateAttribute_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Old" };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> {
                    new UpdateCategoryAttributeDTO { AttributeName = "Size" },
                    new UpdateCategoryAttributeDTO { AttributeName = "Size" }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Duplicate attributes: Size", ex.Message);
        }

        [Fact]
        public async Task UTCID01_ChangeCategoryStatusAsync_Active_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Inactive };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.ChangeCategoryStatusAsync(id, Status.Active)).Returns(Task.CompletedTask);

            await _service.ChangeCategoryStatusAsync(id, Status.Active);

            _categoryRepositoryMock.Verify(x => x.ChangeCategoryStatusAsync(id, Status.Active), Times.Once);
        }

        [Fact]
        public async Task UTCID02_ChangeCategoryStatusAsync_Inactive_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Active };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.ChangeCategoryStatusAsync(id, Status.Inactive)).Returns(Task.CompletedTask);

            await _service.ChangeCategoryStatusAsync(id, Status.Inactive);

            _categoryRepositoryMock.Verify(x => x.ChangeCategoryStatusAsync(id, Status.Inactive), Times.Once);
        }

        [Fact]
        public async Task UTCID03_ChangeCategoryStatusAsync_Deleted_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var category = new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Active };
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);
            _categoryRepositoryMock.Setup(x => x.ChangeCategoryStatusAsync(id, Status.Deleted)).Returns(Task.CompletedTask);

            await _service.ChangeCategoryStatusAsync(id, Status.Deleted);

            _categoryRepositoryMock.Verify(x => x.ChangeCategoryStatusAsync(id, Status.Deleted), Times.Once);
        }

        [Fact]
        public async Task UTCID04_ChangeCategoryStatusAsync_CategoryNotFound_ShouldThrowException()
        {
            var id = Guid.NewGuid();
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync((Category?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ChangeCategoryStatusAsync(id, Status.Active));

            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_ChangeCategoryStatusAsync_RepositoryThrowsException()
        {
            var id = Guid.NewGuid();
            
            _categoryRepositoryMock.Setup(x => x.GetCategoryByIdAsync(id)).ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ChangeCategoryStatusAsync(id, Status.Active));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

    }
}
