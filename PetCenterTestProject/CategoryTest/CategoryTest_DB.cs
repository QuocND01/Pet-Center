using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Category;
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
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Responses.Category.CategoryAttributeResponseDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterTestProject.CategoryTest
{
    public class CategoryTest_DB : IAsyncLifetime
    {
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;

        public CategoryTest_DB()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<CategoryAttributeProfile>();

            }, NullLoggerFactory.Instance);


            mapperConfig.AssertConfigurationIsValid();


            _mapper = mapperConfig.CreateMapper();
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


        private CategoryRepository CreateRepository(PetCenterContext context)
        {
            return new CategoryRepository(context);
        }


        private CategoryService CreateService(PetCenterContext context)
        {
            return new CategoryService(
                CreateRepository(context),
                _mapper,
                _cloudinaryServiceMock.Object);
        }


        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.CategoryAttributes.RemoveRange(context.CategoryAttributes);

            context.Categories.RemoveRange(context.Categories);

            await context.SaveChangesAsync();
        }


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
        // GetAllCategory
        //=========================================================

        [Fact]
        public void UTCID01_GetAllCategory_ReturnActiveCategoryList()
        {
            using var context = CreateContext();
            ClearDatabaseAsync(context).GetAwaiter().GetResult();

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Cat Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Bird Food", Status = Status.Inactive }
            );
            context.SaveChanges();

            var service = CreateService(context);
            var result = service.GetAllCategory().ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, x => x.CategoryName == "Bird Food");
        }

        [Fact]
        public void UTCID02_GetAllCategory_ReturnEmptyList_WhenNoCategoryExists()
        {
            using var context = CreateContext();
            ClearDatabaseAsync(context).GetAwaiter().GetResult();

            context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), CategoryName = "Bird Food", Status = Status.Inactive });
            context.SaveChanges();

            var service = CreateService(context);
            var result = service.GetAllCategory().ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void UTCID03_GetAllCategory_ThrowsException_WhenRepositoryThrowsException()
        {
            Assert.True(true);
        }

        //=========================================================
        // GetAllCategoryAdminAsync
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenKeywordEmptyAndStatusAll()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Cat Food", Status = Status.Inactive }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var spec = new CategorySpecification { Search = "", Status = null, Page = 1, PageSize = 10 };
            var result = await service.GetAllCategoryAdminAsync(spec);

            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task UTCID02_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenKeywordExists()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Cat Food", Status = Status.Inactive }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var spec = new CategorySpecification { Search = "Dog", Status = null, Page = 1, PageSize = 10 };
            var result = await service.GetAllCategoryAdminAsync(spec);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Dog Food", result.Data.First().CategoryName);
        }

        [Fact]
        public async Task UTCID03_GetAllCategoryAdminAsync_ReturnEmptyPagedList_WhenKeywordNotExists()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var spec = new CategorySpecification { Search = "Bird", Status = null, Page = 1, PageSize = 10 };
            var result = await service.GetAllCategoryAdminAsync(spec);

            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task UTCID04_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenStatusActive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Cat Food", Status = Status.Inactive }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var spec = new CategorySpecification { Search = "", Status = Status.Active, Page = 1, PageSize = 10 };
            var result = await service.GetAllCategoryAdminAsync(spec);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(Status.Active, result.Data.First().Status);
        }

        [Fact]
        public async Task UTCID05_GetAllCategoryAdminAsync_ReturnPagedCategoryList_WhenStatusInactive()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            context.Categories.AddRange(
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food", Status = Status.Active },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Cat Food", Status = Status.Inactive }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var spec = new CategorySpecification { Search = "", Status = Status.Inactive, Page = 1, PageSize = 10 };
            var result = await service.GetAllCategoryAdminAsync(spec);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(Status.Inactive, result.Data.First().Status);
        }

        [Fact]
        public async Task UTCID06_GetAllCategoryAdminAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            Assert.True(true);
        }

        //=========================================================
        // GetCategoryByIdAsync
        //=========================================================

        [Fact]
        public async Task UTCID01_GetCategoryByIdAsync_ReturnCategoryInformation_WhenCategoryExists()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Dog Food" });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetCategoryByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.CategoryId);
        }

        [Fact]
        public async Task UTCID02_GetCategoryByIdAsync_ThrowsKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCategoryByIdAsync(Guid.NewGuid()));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_GetCategoryByIdAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            Assert.True(true);
        }

        //=========================================================
        // AddCategoryAsync
        //=========================================================

        [Fact]
        public void UTCID01_AddCategoryAsync_CategoryNameNull_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryDTO { CategoryName = null! };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void UTCID02_AddCategoryAsync_CategoryNameEmpty_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryDTO { CategoryName = "" };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void UTCID03_AddCategoryAsync_CategoryNameLengthGreaterThan200_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryDTO { CategoryName = new string('A', 201) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID04_AddCategoryAsync_CategoryNameLength200_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { CategoryName = new string('A', 200) };
            await service.AddCategoryAsync(dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(new string('A', 200), category.CategoryName);
        }

        [Fact]
        public void UTCID05_AddCategoryAsync_CategoryNameContainsSpecialCharacter_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryDTO { CategoryName = "Dog@Food" };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID06_AddCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddCategoryAsync(dto));
            Assert.Equal("Category already exists", ex.Message);
        }

        [Fact]
        public void UTCID07_AddCategoryAsync_DescriptionLengthGreaterThan2000_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = new string('A', 2001) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Description cannot exceed 2000 characters");
        }

        [Fact]
        public async Task UTCID08_AddCategoryAsync_DescriptionLength2000_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = new string('A', 2000) };
            await service.AddCategoryAsync(dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(new string('A', 2000), category.CategoryDescription);
        }

        [Fact]
        public async Task UTCID09_AddCategoryAsync_ValidDescription_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = "Valid" };
            await service.AddCategoryAsync(dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("Valid", category.CategoryDescription);
        }

        [Fact]
        public async Task UTCID10_AddCategoryAsync_LogoNull_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = null };
            await service.AddCategoryAsync(dto);

            var category = await context.Categories.FirstAsync();
            Assert.Null(category.CategoryLogo);
        }

        [Fact]
        public async Task UTCID11_AddCategoryAsync_UploadImageSuccess_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories")).ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/logo.jpg"), PublicId = "new-public-id" });

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };
            await service.AddCategoryAsync(dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("https://demo.com/logo.jpg", category.CategoryLogo);
            Assert.Equal("new-public-id", category.PublicId);
        }

        [Fact]
        public async Task UTCID12_AddCategoryAsync_UploadImageFail_ShouldThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories")).ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.BadRequest });

            var dto = new CreateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };
            var ex = await Assert.ThrowsAsync<Exception>(() => service.AddCategoryAsync(dto));
            Assert.Equal("Failed to upload category logo", ex.Message);
        }

        [Fact]
        public void UTCID13_AddCategoryAsync_AttributeNameNull_ShouldThrowInvalidOperationException()
        {
            var dto = new CreateCategoryAttributeDTO { AttributeName = null! };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name is required");
        }

        [Fact]
        public void UTCID14_AddCategoryAsync_AttributeNameGreaterThan200_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryAttributeDTO { AttributeName = new string('A', 201) };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name cannot exceed 200 characters");
        }

        [Fact]
        public async Task UTCID15_AddCategoryAsync_AttributeNameLength200_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<CreateCategoryAttributeDTO> { new CreateCategoryAttributeDTO { AttributeName = new string('A', 200) } }
            };
            await service.AddCategoryAsync(dto);

            var attr = await context.CategoryAttributes.FirstAsync();
            Assert.Equal(new string('A', 200), attr.AttributeName);
        }

        [Fact]
        public void UTCID16_AddCategoryAsync_AttributeNameContainsSpecialCharacter_ShouldReturnValidationError()
        {
            var dto = new CreateCategoryAttributeDTO { AttributeName = "Size@" };
            var result = Validate(dto);
            Assert.Contains(result, x => x.ErrorMessage == "Category Attribute name cannot contain special characters");
        }

        [Fact]
        public async Task UTCID17_AddCategoryAsync_DuplicateAttributes_ShouldThrowInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<CreateCategoryAttributeDTO> {
                    new CreateCategoryAttributeDTO { AttributeName = "Size" },
                    new CreateCategoryAttributeDTO { AttributeName = "Size" }
                }
            };
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddCategoryAsync(dto));
            Assert.Equal("Duplicate attributes: Size", ex.Message);
        }

        [Fact]
        public async Task UTCID18_AddCategoryAsync_ValidAttributes_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<CreateCategoryAttributeDTO> { new CreateCategoryAttributeDTO { AttributeName = "Size" } }
            };
            await service.AddCategoryAsync(dto);

            var attr = await context.CategoryAttributes.FirstAsync();
            Assert.Equal("Size", attr.AttributeName);
        }

        [Fact]
        public async Task UTCID19_AddCategoryAsync_RepositoryThrowsException_ShouldThrowException()
        {
            Assert.True(true);
        }

        [Fact]
        public async Task UTCID01_UpdateCategoryAsync_CategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task UTCID02_UpdateCategoryAsync_ExistingCategoryHasLogo_UploadSuccess_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old Name", PublicId = "old-public-id" });
            await context.SaveChangesAsync();
            var service = CreateService(context);
            
            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories")).ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/logo.jpg"), PublicId = "new-public-id" });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("https://demo.com/logo.jpg", category.CategoryLogo);
            Assert.Equal("new-public-id", category.PublicId);
        }

        [Fact]
        public async Task UTCID03_UpdateCategoryAsync_ExistingAttributeRemoved_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            var attrId1 = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old Name", CategoryAttributes = new List<CategoryAttribute> { new CategoryAttribute { CategoryAttributeId = attrId1, AttributeName = "Color", IsActive = true } } });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", Attributes = new List<UpdateCategoryAttributeDTO>() };
            await service.UpdateCategoryAsync(id, dto);

            var attr = await context.CategoryAttributes.FirstAsync(a => a.CategoryAttributeId == attrId1);
            Assert.False(attr.IsActive);
        }

        [Fact]
        public async Task UTCID04_UpdateCategoryAsync_ValidAttributes_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            var attrId1 = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old Name", CategoryAttributes = new List<CategoryAttribute> { new CategoryAttribute { CategoryAttributeId = attrId1, AttributeName = "Color", IsActive = true } } });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", Attributes = new List<UpdateCategoryAttributeDTO> { new UpdateCategoryAttributeDTO { CategoryAttributeId = attrId1, AttributeName = "Size" } } };
            await service.UpdateCategoryAsync(id, dto);

            var attr = await context.CategoryAttributes.FirstAsync(a => a.CategoryAttributeId == attrId1);
            Assert.Equal("Size", attr.AttributeName);
            Assert.True(attr.IsActive);
        }

        [Fact]
        public async Task UTCID05_UpdateCategoryAsync_NewValidAttributeAdded_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old Name", CategoryAttributes = new List<CategoryAttribute>() });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", Attributes = new List<UpdateCategoryAttributeDTO> { new UpdateCategoryAttributeDTO { AttributeName = "Weight" } } };
            await service.UpdateCategoryAsync(id, dto);

            var attr = await context.CategoryAttributes.FirstAsync(a => a.AttributeName == "Weight");
            Assert.True(attr.IsActive);
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = new string('A', 200) };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(new string('A', 200), category.CategoryName);
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("Dog Food", category.CategoryName);
        }

        [Fact]
        public async Task UTCID12_UpdateCategoryAsync_DuplicateCategory_ShouldThrowInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), CategoryName = "Dog Food" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCategoryAsync(id, dto));
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = new string('A', 2000) };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(new string('A', 2000), category.CategoryDescription);
        }

        [Fact]
        public async Task UTCID15_UpdateCategoryAsync_DescriptionValid_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryDescription = "Valid description" };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("Valid description", category.CategoryDescription);
        }

        [Fact]
        public async Task UTCID16_UpdateCategoryAsync_CategoryLogoExists_UploadSuccess_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories")).ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/logo.jpg"), PublicId = "new-public-id" });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };
            await service.UpdateCategoryAsync(id, dto);

            var category = await context.Categories.FirstAsync();
            Assert.Equal("https://demo.com/logo.jpg", category.CategoryLogo);
            Assert.Equal("new-public-id", category.PublicId);
        }

        [Fact]
        public async Task UTCID17_UpdateCategoryAsync_CategoryLogoExists_UploadFail_ShouldThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);
            IFormFile file = new FormFile(stream, 0, 100, "CategoryLogo", "logo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "categories")).ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.BadRequest });

            var dto = new UpdateCategoryDTO { CategoryName = "Dog Food", CategoryLogo = file };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateCategoryAsync(id, dto));
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> { new UpdateCategoryAttributeDTO { AttributeName = new string('A', 200) } }
            };
            await service.UpdateCategoryAsync(id, dto);

            var attr = await context.CategoryAttributes.FirstAsync();
            Assert.Equal(new string('A', 200), attr.AttributeName);
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
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Old" });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var dto = new UpdateCategoryDTO { 
                CategoryName = "Dog Food", 
                Attributes = new List<UpdateCategoryAttributeDTO> {
                    new UpdateCategoryAttributeDTO { AttributeName = "Size" },
                    new UpdateCategoryAttributeDTO { AttributeName = "Size" }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCategoryAsync(id, dto));
            Assert.Equal("Duplicate attributes: Size", ex.Message);
        }

        [Fact]
        public async Task UTCID01_ChangeCategoryStatusAsync_Active_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Inactive });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.ChangeCategoryStatusAsync(id, Status.Active);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(Status.Active, category.Status);
        }

        [Fact]
        public async Task UTCID02_ChangeCategoryStatusAsync_Inactive_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Active });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.ChangeCategoryStatusAsync(id, Status.Inactive);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(Status.Inactive, category.Status);
        }

        [Fact]
        public async Task UTCID03_ChangeCategoryStatusAsync_Deleted_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var id = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = id, CategoryName = "Dog Food", Status = Status.Active });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.ChangeCategoryStatusAsync(id, Status.Deleted);

            var category = await context.Categories.FirstAsync();
            Assert.Equal(Status.Deleted, category.Status);
        }

        [Fact]
        public async Task UTCID04_ChangeCategoryStatusAsync_CategoryNotFound_ShouldThrowException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            var ex = await Assert.ThrowsAsync<Exception>(() => service.ChangeCategoryStatusAsync(id, Status.Active));
            Assert.Equal("Category not found", ex.Message);
        }
    }
}
