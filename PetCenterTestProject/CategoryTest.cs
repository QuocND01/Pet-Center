using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Protocol.Core.Types;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryAttributeResponseDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterTestProject
{
    public class CategoryTest
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;
        private readonly CategoryService _service;

        public CategoryTest()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<CategoryAttributeProfile>();
            }, NullLoggerFactory.Instance);


            _mapper = config.CreateMapper();


            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();


            _service = new CategoryService(
                _categoryRepositoryMock.Object,
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


        //=========================================================
        // GetAllCategory()
        // UTCID01 - Return active category list
        //=========================================================
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

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once);
        }


        //=========================================================
        // GetAllCategory()
        // UTCID02 - Return empty list
        //=========================================================

        [Fact]
        public void UTCID02_GetAllCategory_ReturnEmptyList()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(x => x.GetAllCategory())
                .Returns(new List<Category>().AsQueryable());

            // Act
            var result = _service.GetAllCategory().ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once);
        }


        //=========================================================
        // GetAllCategory()
        // UTCID03 - Repository throws exception
        //=========================================================

        [Fact]
        public void UTCID03_GetAllCategory_RepositoryThrowsException()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(x => x.GetAllCategory())
                .Throws(new Exception("Service Temporarily Unavailable"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() =>
            {
                _service.GetAllCategory().ToList();
            });

            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategory(),
                Times.Once);
        }



        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID01 - Search keyword empty, Status = All
        // Expected: Return paged category list
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenKeywordEmptyAndStatusAll()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PetCenterContext(options);

            context.Categories.AddRange(
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
                });

            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var spec = new CategorySpecification
            {
                Search = "",
                Status = null,
                Page = 1,
                PageSize = 10
            };

            // Act

            var (items, total) = await repository.GetAllCategoryAdminAsync(spec);

            // Assert

            Assert.NotNull(items);

            Assert.Equal(2, total);

            Assert.Equal(2, items.Count());
        }

        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID02 - Search existing keyword
        // Expected: Return paged category list
        //=========================================================

        [Fact]
        public async Task UTCID02_GetAllCategoryAdminAsync_SearchExistingKeyword_ShouldReturnMatchedCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PetCenterContext(options);

            context.Categories.AddRange(
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
                });

            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var spec = new CategorySpecification
            {
                Search = "Dog",
                Page = 1,
                PageSize = 10
            };

            // Act

            var (items, total) = await repository.GetAllCategoryAdminAsync(spec);

            // Assert

            Assert.Single(items);

            Assert.Equal(1, total);

            Assert.Equal("Dog Food", items.First().CategoryName);
        }

        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID03 - Search non-existing keyword
        // Expected: Return empty paged list
        //=========================================================

        [Fact]
        public async Task UTCID03_GetAllCategoryAdminAsync_SearchNotFound_ShouldReturnEmpty()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PetCenterContext(options);

            context.Categories.Add(
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Dog Food",
                    Status = Status.Active
                });

            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var spec = new CategorySpecification
            {
                Search = "Laptop"
            };

            // Act

            var (items, total) = await repository.GetAllCategoryAdminAsync(spec);

            // Assert

            Assert.Empty(items);

            Assert.Equal(0, total);
        }

        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID04 - Filter Status = Active
        // Expected: Return paged category list
        //=========================================================

        [Fact]
        public async Task UTCID04_GetAllCategoryAdminAsync_StatusActive_ShouldReturnOnlyActive()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PetCenterContext(options);

            context.Categories.AddRange(
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Dog",
                    Status = Status.Active
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Cat",
                    Status = Status.Inactive
                });

            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var spec = new CategorySpecification
            {
                Status = Status.Active
            };

            // Act

            var (items, total) = await repository.GetAllCategoryAdminAsync(spec);

            // Assert

            Assert.Single(items);

            Assert.Equal(Status.Active, items.First().Status);
        }
        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID05 - Filter Status = Inactive
        // Expected: Return paged category list
        //=========================================================

        [Fact]
        public async Task UTCID05_GetAllCategoryAdminAsync_StatusInactive_ShouldReturnOnlyInactive()
        {
            // Arrange

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PetCenterContext(options);

            context.Categories.AddRange(
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Dog Food",
                    Status = Status.Active
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Bird Food",
                    Status = Status.Inactive
                });

            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var spec = new CategorySpecification
            {
                Search = "",
                Status = Status.Inactive,
                Page = 1,
                PageSize = 10
            };

            // Act

            var (items, total) = await repository.GetAllCategoryAdminAsync(spec);

            // Assert

            Assert.Single(items);

            Assert.Equal(1, total);

            Assert.Equal(Status.Inactive, items.First().Status);

            Assert.Equal("Bird Food", items.First().CategoryName);
        }

        //=========================================================
        // GetAllCategoryAdminAsync()
        // UTCID06 - Repository throws exception
        // Expected: Throw exception with message
        // "Service Temporarily Unavailable"
        //=========================================================

        [Fact]
        public async Task UTCID06_GetAllCategoryAdminAsync_RepositoryThrowsException()
        {
            // Arrange
            var spec = new CategorySpecification();

            _categoryRepositoryMock
                .Setup(x => x.GetAllCategoryAdminAsync(spec))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetAllCategoryAdminAsync(spec));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetAllCategoryAdminAsync(spec),
                Times.Once);
        }




        //=========================================================
        // GetCategoryByIdAsync()
        // UTCID01 - Existing Guid - Return category information
        //=========================================================

        [Fact]
        public async Task UTCID01_GetCategoryByIdAsync_ReturnCategoryInformation_WhenCategoryExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryDescription = "Food for dogs",
                Status = Status.Active
            };

            var dto = new ReadCategoryDTO
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryDescription = "Food for dogs"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            // Act
            var result = await _service.GetCategoryByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.CategoryId);
            Assert.Equal("Dog Food", result.CategoryName);
        }


        //=========================================================
        // GetCategoryByIdAsync()
        // UTCID02 - Non-existing Guid - Throw KeyNotFoundException
        //=========================================================

        [Fact]
        public async Task UTCID02_GetCategoryByIdAsync_NonExistingGuid_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetCategoryByIdAsync(id));

            Assert.Equal("Category not found", ex.Message);
        }


        //=========================================================
        // GetCategoryByIdAsync()
        // UTCID03 - Repository throws exception
        //=========================================================

        [Fact]
        public async Task UTCID03_GetCategoryByIdAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _service.GetCategoryByIdAsync(id));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID01 - CategoryName = null -> Validation error
        //=========================================================
        [Fact]
        public void UTCID01_CreateCategoryDTO_CategoryNameNull_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = null!
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name is required");
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID02 - CategoryName = empty -> Validation error
        //=========================================================
        [Fact]
        public void UTCID02_CreateCategoryDTO_CategoryNameEmpty_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = ""
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name is required");
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID03 - CategoryName length > 200 -> Validation error
        //=========================================================
        [Fact]
        public void UTCID03_CreateCategoryDTO_CategoryNameTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = new string('A', 201)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name cannot exceed 200 characters");
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID04 - CategoryName length = 200 -> Add successfully
        //=========================================================
        [Fact]
        public async Task UTCID04_AddCategoryAsync_CategoryNameLength200_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = new string('A', 200),
                Attributes = new()
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID05 - CategoryName contains special characters
        //=========================================================
        [Fact]
        public void UTCID05_CreateCategoryDTO_CategoryNameContainsSpecialCharacter_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog@Food"
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name cannot contain special characters");
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID06 - Duplicate category name
        //=========================================================
        [Fact]
        public async Task UTCID06_AddCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new()
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(true);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Category already exists", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID07 - Upload category logo successfully
        //=========================================================
        [Fact]
        public async Task UTCID07_AddCategoryAsync_UploadImageSuccess_ShouldAddSuccessfully()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                100,
                "CategoryLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file,
                Attributes = new()
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/logo.jpg"),
                    PublicId = "logo123"
                });

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(file, "categories"),
                Times.Once);

            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID08 - CategoryDescription length > 2000 -> Validation error
        //=========================================================
        [Fact]
        public void UTCID08_CreateCategoryDTO_DescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2001)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Description cannot exceed 2000 characters");
        }



        //=========================================================
        // AddCategoryAsync()
        // UTCID09 - CategoryDescription length = 2000 -> Add successfully
        //=========================================================
        [Fact]
        public async Task UTCID09_AddCategoryAsync_DescriptionLength2000_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2000),
                Attributes = new()
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID10 - Valid description -> Category added successfully
        //=========================================================
        [Fact]
        public async Task UTCID10_AddCategoryAsync_ValidDescription_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = "Premium pet food",
                Attributes = new()
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID11 - AttributeName = null -> InvalidOperationException
        //=========================================================
        [Fact]
        public async Task UTCID11_AddCategoryAsync_AttributeNameNull_ShouldThrowException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new()
        {
            new CreateCategoryAttributeDTO
            {
                AttributeName = null
            }
        }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Attribute name is required.", ex.Message);
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID12 - AttributeName length > 200 -> Validation error
        //=========================================================
        [Fact]
        public void UTCID12_CreateCategoryAttributeDTO_AttributeNameTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryAttributeDTO
            {
                AttributeName = new string('A', 201)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category Attribute name cannot exceed 200 characters");
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID13 - AttributeName length = 200 -> Category added successfully
        //=========================================================
        [Fact]
        public async Task UTCID13_AddCategoryAsync_AttributeNameLength200_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new()
        {
            new CreateCategoryAttributeDTO
            {
                AttributeName = new string('A', 200)
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        //=========================================================
        // AddCategoryAsync()
        // UTCID14 - AttributeName contains special characters
        //=========================================================
        [Fact]
        public void UTCID14_CreateCategoryAttributeDTO_AttributeNameContainsSpecialCharacter_ShouldFailValidation()
        {
            // Arrange
            var dto = new CreateCategoryAttributeDTO
            {
                AttributeName = "Color@"
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category Attribute name cannot contain special characters");
        }



        //=========================================================
        // AddCategoryAsync()
        // UTCID15 - Duplicate AttributeName -> InvalidOperationException
        //=========================================================
        [Fact]
        public async Task UTCID15_AddCategoryAsync_DuplicateAttribute_ShouldThrowException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new()
        {
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Color"
            },
            new CreateCategoryAttributeDTO
            {
                AttributeName = "color"
            }
        }
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AddCategoryAsync(dto));

            // Assert
            Assert.StartsWith("Duplicate attributes", ex.Message);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID16 - Valid attributes -> Category added successfully
        //=========================================================
        [Fact]
        public async Task UTCID16_AddCategoryAsync_ValidAttributes_ShouldSuccess()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new()
        {
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Color"
            },
            new CreateCategoryAttributeDTO
            {
                AttributeName = "Weight"
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName,null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID17 - Upload category logo successfully
        //=========================================================
        [Fact]
        public async Task UTCID17_AddCategoryAsync_UploadImageSuccess_ShouldSuccess()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                100,
                "CategoryLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/logo.jpg"),
                    PublicId = "category-logo"
                });

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCategoryAsync(dto);

            // Assert
            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(file, "categories"),
                Times.Once);

            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID18 - Upload category logo failed
        //=========================================================
        [Fact]
        public async Task UTCID18_AddCategoryAsync_UploadImageFailed_ShouldThrowException()
        {
            // Arrange
            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                100,
                "CategoryLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Failed to upload category logo", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }


        //=========================================================
        // AddCategoryAsync()
        // UTCID19 - Repository throws exception
        //=========================================================
        [Fact]
        public async Task UTCID19_AddCategoryAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var dto = new CreateCategoryDTO
            {
                CategoryName = "Dog Food"
            };

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, null))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _service.AddCategoryAsync(dto));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID01 - Category not found
        // Expected: Throw KeyNotFoundException
        //=========================================================

        [Fact]
        public async Task UTCID01_UpdateCategoryAsync_CategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Equal("Category not found", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.GetCategoryByIdAsync(id),
                Times.Once);
        }

        //=========================================================
        // UpdateCategoryAsync()
        // UTCID02 - Update successfully with new logo
        // Expected: Category updated successfully
        //=========================================================

        [Fact]
        public async Task UTCID02_UpdateCategoryAsync_UploadNewLogo_ShouldUpdateSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                PublicId = "old-public-id",
                CategoryAttributes = new List<CategoryAttribute>()
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
      .Setup(x => x.DeleteImageAsync("old-public-id"))
      .ReturnsAsync(new DeletionResult
      {
          Result = "ok"
      });
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/logo.jpg"),
                    PublicId = "new-public-id"
                });

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            Assert.Equal("https://cloudinary.com/logo.jpg", category.CategoryLogo);
            Assert.Equal("new-public-id", category.PublicId);

            _cloudinaryServiceMock.Verify(
                x => x.DeleteImageAsync("old-public-id"),
                Times.Once);

            _cloudinaryServiceMock.Verify(
                x => x.UploadImageAsync(file, "categories"),
                Times.Once);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }



        //=========================================================
        // UpdateCategoryAsync()
        // UTCID03 - Remove existing attribute
        // Expected: Existing attribute becomes inactive
        //=========================================================

        [Fact]
        public async Task UTCID03_UpdateCategoryAsync_RemoveAttribute_ShouldSetInactive()
        {
            // Arrange
            var id = Guid.NewGuid();

            var colorId = Guid.NewGuid();
            var weightId = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryAttributes = new List<CategoryAttribute>
        {
            new()
            {
                CategoryAttributeId = colorId,
                AttributeName = "Color",
                IsActive = true
            },
            new()
            {
                CategoryAttributeId = weightId,
                AttributeName = "Weight",
                IsActive = true
            }
        }
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new List<UpdateCategoryAttributeDTO>
        {
            new()
            {
                CategoryAttributeId = colorId,
                AttributeName = "Color"
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            var removedAttr = category.CategoryAttributes
     .First(x => x.CategoryAttributeId == weightId);

            Assert.False(removedAttr.IsActive);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID04 - Add new attribute
        // Expected: New attribute is added successfully
        //=========================================================

        [Fact]
        public async Task UTCID04_UpdateCategoryAsync_AddNewAttribute_ShouldSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryAttributes = new List<CategoryAttribute>
        {
            new()
            {
                AttributeName="Color",
                IsActive=true
            }
        }
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new List<UpdateCategoryAttributeDTO>
        {
            new()
            {
                AttributeName="Color"
            },
            new()
            {
                AttributeName="Size"
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            Assert.Equal(3, category.CategoryAttributes.Count);

            Assert.Contains(
                category.CategoryAttributes,
                x => x.AttributeName == "Size");

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID05 - Repository throws exception
        // Expected: Service Temporarily Unavailable
        //=========================================================

        [Fact]
        public async Task UTCID05_UpdateCategoryAsync_RepositoryThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.UpdateCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);
        }


        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID06 - CategoryName is null
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID06_UpdateCategoryDTO_CategoryNameNull_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = null!
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name is required");
        }


        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID07 - CategoryName exceeds 200 characters
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID07_UpdateCategoryDTO_CategoryNameTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = new string('A', 201)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name cannot exceed 200 characters");
        }


        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID08 - CategoryName exactly 200 characters
        // Expected: Validation success
        //=========================================================

        [Fact]
        public void UTCID08_UpdateCategoryDTO_CategoryNameMaxLength_ShouldPassValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = new string('A', 200)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.DoesNotContain(result,
                x => x.ErrorMessage ==
                "Category name cannot exceed 200 characters");
        }


        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID09 - CategoryName contains special characters
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID09_UpdateCategoryDTO_CategoryNameContainsSpecialCharacters_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog@Food!"
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category name cannot contain special characters");
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID10 - Duplicate category name
        // Expected: Throw InvalidOperationException
        //=========================================================

        [Fact]
        public async Task UTCID10_UpdateCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Cat Food"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Equal("Category already exists", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }

        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID11 - Description exceeds 2000 characters
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID11_UpdateCategoryDTO_DescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2001)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Description cannot exceed 2000 characters");
        }

        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID12 - Description exactly 2000 characters
        // Expected: Validation success
        //=========================================================

        [Fact]
        public void UTCID12_UpdateCategoryDTO_DescriptionMaxLength_ShouldPassValidation()
        {
            // Arrange
            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = new string('A', 2000)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.DoesNotContain(result,
                x => x.ErrorMessage ==
                "Description cannot exceed 2000 characters");
        }

        //=========================================================
        // UpdateCategoryAsync()
        // UTCID13 - Upload new logo successfully
        // Expected: Category updated successfully
        //=========================================================

        [Fact]
        public async Task UTCID13_UpdateCategoryAsync_UploadLogoSuccessfully_ShouldUpdateCategory()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                PublicId = ""
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://cloudinary.com/category.jpg"),
                    PublicId = "category-public-id"
                });

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            Assert.Equal(
                "https://cloudinary.com/category.jpg",
                category.CategoryLogo);

            Assert.Equal(
                "category-public-id",
                category.PublicId);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }

        //=========================================================
        // UpdateCategoryAsync()
        // UTCID14 - Upload logo failed
        // Expected: Throw Exception
        //=========================================================

        [Fact]
        public async Task UTCID14_UpdateCategoryAsync_UploadLogoFailed_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food"
            };

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryLogo = file
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "categories"))
                .ReturnsAsync((ImageUploadResult)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Equal(
                "Failed to upload category logo",
                ex.Message);
        }

        //=========================================================
        // UpdateCategoryAsync()
        // UTCID15 - Valid description
        // Expected: Category updated successfully
        //=========================================================

        [Fact]
        public async Task UTCID15_UpdateCategoryAsync_ValidDescription_ShouldUpdateSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food"
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                CategoryDescription = "Healthy food for pets."
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }

        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID16 - Attribute name is null
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID16_UpdateCategoryAttributeDTO_AttributeNameNull_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryAttributeDTO
            {
                AttributeName = null
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category Attribute name is required");
        }

        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID17 - Attribute name exceeds 200 characters
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID17_UpdateCategoryAttributeDTO_AttributeNameTooLong_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryAttributeDTO
            {
                AttributeName = new string('A', 201)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category Attribute name cannot exceed 200 characters");
        }

        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID18 - Attribute name exactly 200 characters
        // Expected: Validation success
        //=========================================================

        [Fact]
        public void UTCID18_UpdateCategoryAttributeDTO_AttributeNameMaxLength_ShouldPassValidation()
        {
            // Arrange
            var dto = new UpdateCategoryAttributeDTO
            {
                AttributeName = new string('A', 200)
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.DoesNotContain(result,
                x => x.ErrorMessage ==
                "Category Attribute name cannot exceed 200 characters");
        }


        //=========================================================
        // UpdateCategoryDTO Validation
        // UTCID19 - Attribute name contains special characters
        // Expected: Validation error
        //=========================================================

        [Fact]
        public void UTCID19_UpdateCategoryAttributeDTO_AttributeContainsSpecialCharacters_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateCategoryAttributeDTO
            {
                AttributeName = "Color@123"
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Category Attribute name cannot contain special characters");
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID20 - Duplicate attributes
        // Expected: Throw InvalidOperationException
        //=========================================================

        [Fact]
        public async Task UTCID20_UpdateCategoryAsync_DuplicateAttributes_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryAttributes = new List<CategoryAttribute>()
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new List<UpdateCategoryAttributeDTO>
        {
            new()
            {
                AttributeName = "Color"
            },
            new()
            {
                AttributeName = "Color"
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Contains("Duplicate attributes", ex.Message);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(It.IsAny<Category>()),
                Times.Never);
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID21 - Repository throws exception while updating
        // Expected: Service Temporarily Unavailable
        //=========================================================

        [Fact]
        public async Task UTCID21_UpdateCategoryAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryAttributes = new List<CategoryAttribute>()
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(x => x.UpdateCategoryAsync(It.IsAny<Category>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateCategoryAsync(id, dto));

            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);
        }


        //=========================================================
        // UpdateCategoryAsync()
        // UTCID22 - Valid attributes
        // Expected: Category updated successfully
        //=========================================================

        [Fact]
        public async Task UTCID22_UpdateCategoryAsync_ValidAttributes_ShouldUpdateSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                CategoryAttributes = new List<CategoryAttribute>()
            };

            var dto = new UpdateCategoryDTO
            {
                CategoryName = "Dog Food",
                Attributes = new List<UpdateCategoryAttributeDTO>
        {
            new()
            {
                AttributeName = "Color"
            },
            new()
            {
                AttributeName = "Weight"
            }
        }
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.CheckCategoryExistAsync(dto.CategoryName, id))
                .ReturnsAsync(false);

            // Act
            await _service.UpdateCategoryAsync(id, dto);

            // Assert
            Assert.Equal(2, category.CategoryAttributes.Count);

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategoryAsync(category),
                Times.Once);
        }


        //=========================================================
        // ChangeCategoryStatusAsync()
        // UTCID01 - Change category status to Active
        //=========================================================
        [Fact]
        public async Task UTCID01_ChangeCategoryStatusAsync_ActiveStatus_ShouldChangeSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Dog Food",
                Status = Status.Inactive
            };


            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);


            _categoryRepositoryMock
                .Setup(x => x.ChangeCategoryStatusAsync(id, Status.Active))
                .Returns(Task.CompletedTask);


            // Act
            await _service.ChangeCategoryStatusAsync(
                id,
                Status.Active);


            // Assert
            _categoryRepositoryMock.Verify(
                x => x.ChangeCategoryStatusAsync(id, Status.Active),
                Times.Once);
        }



        //=========================================================
        // UTCID02 - Change category status to Inactive
        //=========================================================
        [Fact]
        public async Task UTCID02_ChangeCategoryStatusAsync_InactiveStatus_ShouldChangeSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();

            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Cat Food",
                Status = Status.Active
            };


            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);


            _categoryRepositoryMock
                .Setup(x => x.ChangeCategoryStatusAsync(id, Status.Inactive))
                .Returns(Task.CompletedTask);



            // Act
            await _service.ChangeCategoryStatusAsync(
                id,
                Status.Inactive);



            // Assert
            _categoryRepositoryMock.Verify(
                x => x.ChangeCategoryStatusAsync(id, Status.Inactive),
                Times.Once);
        }



        //=========================================================
        // UTCID03 - Change category status to Deleted
        //=========================================================
        [Fact]
        public async Task UTCID03_ChangeCategoryStatusAsync_DeletedStatus_ShouldChangeSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();


            var category = new Category
            {
                CategoryId = id,
                CategoryName = "Medicine",
                Status = Status.Active
            };


            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);


            _categoryRepositoryMock
                .Setup(x => x.ChangeCategoryStatusAsync(id, Status.Deleted))
                .Returns(Task.CompletedTask);



            // Act
            await _service.ChangeCategoryStatusAsync(
                id,
                Status.Deleted);



            // Assert
            _categoryRepositoryMock.Verify(
                x => x.ChangeCategoryStatusAsync(id, Status.Deleted),
                Times.Once);
        }



        //=========================================================
        // UTCID04 - Category does not exist
        //=========================================================
        [Fact]
        public async Task UTCID04_ChangeCategoryStatusAsync_NonExistingCategory_ShouldThrowException()
        {
            // Arrange

            var id = Guid.NewGuid();


            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ReturnsAsync((Category)null);



            // Act
            var exception = await Assert.ThrowsAsync<Exception>(
                async () =>
                {
                    await _service.ChangeCategoryStatusAsync(
                        id,
                        Status.Active);
                });



            // Assert
            Assert.Equal(
                "Category not found",
                exception.Message);


            _categoryRepositoryMock.Verify(
                x => x.ChangeCategoryStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Status>()),
                Times.Never);
        }



        //=========================================================
        // UTCID05 - Repository throws exception
        //=========================================================
        [Fact]
        public async Task UTCID05_ChangeCategoryStatusAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange

            var id = Guid.NewGuid();


            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(id))
                .ThrowsAsync(
                    new Exception("Database error"));



            // Act

            var exception = await Assert.ThrowsAsync<Exception>(
                async () =>
                {
                    await _service.ChangeCategoryStatusAsync(
                        id,
                        Status.Active);
                });



            // Assert

            Assert.Equal(
                "Database error",
                exception.Message);


            _categoryRepositoryMock.Verify(
                x => x.ChangeCategoryStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Status>()),
                Times.Never);
        }




    }


}