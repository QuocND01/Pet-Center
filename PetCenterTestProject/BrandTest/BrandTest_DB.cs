using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Brand;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static PetCenterAPI.DTOs.Responses.Brand.BrandResposeDTO;

namespace PetCenterTestProject.BrandTest
{
    public class BrandTest_DB
    {
        //=========================================================
        // Mock external service only
        //=========================================================

        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;

        //=========================================================
        // Constructor
        //=========================================================

        public BrandTest_DB()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BrandProfile>();
            }, NullLoggerFactory.Instance)
            .CreateMapper();
        }

        //=========================================================
        // Create SQL Server Context
        //=========================================================

        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;")
                .Options;

            return new PetCenterContext(options);
        }


        //=========================================================
        // Create Repository
        //=========================================================

        private BrandRepository CreateRepository(PetCenterContext context)
        {
            return new BrandRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================

        private BrandService CreateService(PetCenterContext context)
        {
            return new BrandService(
                CreateRepository(context),
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        //=========================================================
        // Clear Database
        //=========================================================

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.Brands.RemoveRange(context.Brands);
            await context.SaveChangesAsync();
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

        //=========================================================
        // GetAllBrand()
        // UTCID01 - Active brands exist
        // Expected: Return active brand list
        //=========================================================

        [Fact]
        public void UTCID01_GetAllBrand_ReturnActiveBrandList()
        {
            using var context = CreateContext();

            ClearDatabaseAsync(context).GetAwaiter().GetResult();

            context.Brands.AddRange(
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
                },
                new Brand
                {
                    BrandId = Guid.NewGuid(),
                    BrandName = "Whiskas",
                    Status = Status.Inactive
                });

            context.SaveChanges();

            var service = CreateService(context);

            var result = service.GetAllBrand().ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, x => x.BrandName == "Whiskas");
            Assert.All(result, x => Assert.NotEqual(Guid.Empty, x.BrandId));
        }

        //=========================================================
        // GetAllBrand()
        // UTCID02 - No active brands exist
        // Expected: Return empty list
        //=========================================================

        [Fact]
        public void UTCID02_GetAllBrand_ReturnEmptyList()
        {
            using var context = CreateContext();

            ClearDatabaseAsync(context).GetAwaiter().GetResult();

            context.Brands.AddRange(
                new Brand
                {
                    BrandName = "Royal",
                    Status = Status.Inactive
                },
                new Brand
                {
                    BrandName = "Pedigree",
                    Status = Status.Inactive
                });

            context.SaveChanges();

            var service = CreateService(context);

            var result = service.GetAllBrand().ToList();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //=========================================================
        // GetAllBrand()
        // UTCID03 - Repository throws exception
        // Expected:
        // - Throw Exception
        // - Message = Service Temporarily Unavailable
        //=========================================================

        [Fact]
        public void UTCID03_GetAllBrand_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IBrandRepository>();

            repositoryMock
                .Setup(x => x.GetAllBrand())
                .Throws(new Exception("Service Temporarily Unavailable"));

            var service = new BrandService(
                repositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);

            // Act
            var ex = Assert.Throws<Exception>(() =>
                service.GetAllBrand().ToList());

            // Assert
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.AddRange(
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
                });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var spec = new BrandSpecification
            {
                Search = "",
                Page = 1,
                PageSize = 10
            };

            var result = await service.GetAllBrandAdminAsync(spec);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Data.Count());
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.AddRange(
                new Brand
                {
                    BrandName = "Royal Canin",
                    Status = Status.Active
                },
                new Brand
                {
                    BrandName = "Pedigree",
                    Status = Status.Active
                });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var spec = new BrandSpecification
            {
                Search = "Royal"
            };

            var result = await service.GetAllBrandAdminAsync(spec);

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.Add(new Brand
            {
                BrandName = "Royal Canin",
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var spec = new BrandSpecification
            {
                Search = "ABCXYZ"
            };

            var result = await service.GetAllBrandAdminAsync(spec);

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.AddRange(
                new Brand
                {
                    BrandName = "Royal",
                    Status = Status.Active
                },
                new Brand
                {
                    BrandName = "Pedigree",
                    Status = Status.Inactive
                });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var spec = new BrandSpecification
            {
                Status = Status.Active
            };

            var result = await service.GetAllBrandAdminAsync(spec);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.AddRange(
                new Brand
                {
                    BrandName = "Royal",
                    Status = Status.Active
                },
                new Brand
                {
                    BrandName = "Pedigree",
                    Status = Status.Inactive
                });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var spec = new BrandSpecification
            {
                Status = Status.Inactive
            };

            var result = await service.GetAllBrandAdminAsync(spec);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
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
            var repoMock = new Mock<IBrandRepository>();

            repoMock
                .Setup(x => x.GetAllBrandAdminAsync(It.IsAny<BrandSpecification>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = new BrandService(
                repoMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);

            var spec = new BrandSpecification();

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAllBrandAdminAsync(spec));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }


        //=====================================================================
        // Function: GetBrandByIdAsync()
        // Test Requirement:
        // Verify that the function returns brand information by BrandId.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetBrandByIdAsync() returns brand information
        /// when BrandId exists.
        /// Expected:
        /// - Return brand information.
        /// </summary>
        [Fact]
        public async Task UTCID01_GetBrandByIdAsync_ReturnBrand_WhenBrandExists()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = "Royal Canin",
                BrandDescription = "Pet Food",
                Status = Status.Active
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetBrandByIdAsync(brand.BrandId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(brand.BrandId, result.BrandId);
            Assert.Equal("Royal Canin", result.BrandName);
            Assert.Equal("Pet Food", result.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var id = Guid.NewGuid();

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetBrandByIdAsync(id));

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
            var repoMock = new Mock<IBrandRepository>();

            repoMock
                .Setup(x => x.GetBrandByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = new BrandService(
                repoMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetBrandByIdAsync(Guid.NewGuid()));

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Pet Food"
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal("Royal Canin", brand.BrandName);
            Assert.Equal("Pet Food", brand.BrandDescription);
        }



        [Fact]
        public async Task UTCID06_AddBrandAsync_BrandAlreadyExists_ShouldThrowException()
        {
            // Arrange

            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Brands.Add(new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = "Royal Canin",
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddBrandAsync(dto));

            Assert.Equal("Brand already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID07_AddBrandAsync_UploadImageSuccess_ShouldSaveImageInformation()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "BrandLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    PublicId = "brand/logo",
                    SecureUrl = new Uri("https://demo.com/logo.jpg")
                });

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandLogo = file
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal("Royal Canin", brand.BrandName);
            Assert.Equal("brand/logo", brand.PublicId);
            Assert.Equal("https://demo.com/logo.jpg", brand.BrandLogo);
        }

        [Fact]
        public async Task UTCID08_AddBrandAsync_UploadImageFail_ShouldThrowException()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "BrandLogo",
                "logo.jpg")
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

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandLogo = file
            };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.AddBrandAsync(dto));

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal(new string('A', 2000), brand.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal(new string('A', 200), brand.BrandName);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = new string('A', 200),
                BrandDescription = ""
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal(new string('A', 200), brand.BrandName);
            Assert.Equal("", brand.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new CreateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "High quality pet food."
            };

            await service.AddBrandAsync(dto);

            var brand = await context.Brands.FirstAsync();

            Assert.Equal("Royal Canin", brand.BrandName);
            Assert.Equal("High quality pet food.", brand.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal"
            };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateBrandAsync(Guid.NewGuid(), dto));

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.AddRange(
                new Brand
                {
                    BrandId = id,
                    BrandName = "Old Brand",
                    Status = Status.Active
                },
                new Brand
                {
                    BrandId = Guid.NewGuid(),
                    BrandName = "Royal Canin",
                    Status = Status.Active
                });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateBrandAsync(id, dto));

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                PublicId = ""
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "BrandLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    PublicId = "logo123",
                    SecureUrl = new Uri("https://demo.com/logo.jpg")
                });

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Updated",
                BrandLogo = file
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.NotNull(brand);
            Assert.Equal("Royal Updated", brand!.BrandName);
            Assert.Equal("logo123", brand.PublicId);
            Assert.Equal("https://demo.com/logo.jpg", brand.BrandLogo);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Royal"
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "BrandLogo",
                "logo.jpg")
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
                service.UpdateBrandAsync(id, dto));

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
            var repositoryMock = new Mock<IBrandRepository>();
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin"
            };

            repositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = new BrandService(
              repositoryMock.Object,
              _mapper,
              _cloudinaryServiceMock.Object);

            // Act

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateBrandAsync(id, dto));

            // Assert

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
            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2001)
            };

            var result = Validate(dto);

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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Old"
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = new string('A', 200)
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.NotNull(brand);
            Assert.Equal(new string('A', 200), brand!.BrandName);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Old"
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = new string('A', 2000)
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.Equal(new string('A', 2000), brand!.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Royal",
                PublicId = "old-public-id"
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "BrandLogo",
                "logo.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(file, "brands"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    PublicId = "new-public-id",
                    SecureUrl = new Uri("https://demo.com/logo.jpg")
                });

            _cloudinaryServiceMock
                .Setup(x => x.DeleteImageAsync("old-public-id"))
                .ReturnsAsync(new DeletionResult());

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Updated",
                BrandLogo = file
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.Equal("Royal Updated", brand!.BrandName);
            Assert.Equal("new-public-id", brand.PublicId);
            Assert.Equal("https://demo.com/logo.jpg", brand.BrandLogo);

            _cloudinaryServiceMock.Verify(
                x => x.DeleteImageAsync("old-public-id"),
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Old"
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal",
                BrandDescription = "High quality pet food."
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.Equal("Royal", brand!.BrandName);
            Assert.Equal("High quality pet food.", brand.BrandDescription);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var id = Guid.NewGuid();

            context.Brands.Add(new Brand
            {
                BrandId = id,
                BrandName = "Old Brand",
                BrandDescription = "Old Description",
                Status = Status.Active
            });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateBrandDTO
            {
                BrandName = "Royal Canin",
                BrandDescription = "Premium pet food."
            };

            await service.UpdateBrandAsync(id, dto);

            var brand = await context.Brands.FindAsync(id);

            Assert.NotNull(brand);
            Assert.Equal("Royal Canin", brand!.BrandName);
            Assert.Equal("Premium pet food.", brand.BrandDescription);
            Assert.Equal(Status.Active, brand.Status);
        }

        /// <summary>
        /// UTCID01
        /// Verify ChangeBrandStatusAsync() changes status to Active.
        /// Expected:
        /// - Brand status changed successfully.
        /// </summary>
        [Fact]
        public async Task UTCID01_ChangeBrandStatusAsync_Active_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = "Royal",
                Status = Status.Inactive
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            await service.ChangeBrandStatusAsync(brand.BrandId, Status.Active);

            var updated = await context.Brands.FindAsync(brand.BrandId);

            Assert.NotNull(updated);
            Assert.Equal(Status.Active, updated!.Status);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = "Royal Canin",
                Status = Status.Active
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            await service.ChangeBrandStatusAsync(brand.BrandId, Status.Inactive);

            var updated = await context.Brands.FindAsync(brand.BrandId);

            Assert.NotNull(updated);
            Assert.Equal(Status.Inactive, updated!.Status);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var brand = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = "Royal Canin",
                Status = Status.Active
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            await service.ChangeBrandStatusAsync(brand.BrandId, Status.Deleted);

            var updated = await context.Brands.FindAsync(brand.BrandId);

            Assert.NotNull(updated);
            Assert.Equal(Status.Deleted, updated!.Status);
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
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ChangeBrandStatusAsync(Guid.NewGuid(), Status.Active));

            Assert.Equal("Brand not found", ex.Message);
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

            var repositoryMock = new Mock<IBrandRepository>();

            repositoryMock
                .Setup(x => x.GetBrandByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = new BrandService(
                repositoryMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ChangeBrandStatusAsync(id, Status.Active));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }
    }
}