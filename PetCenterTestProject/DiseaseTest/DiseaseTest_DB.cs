using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;
using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

// Dưới đây là các using giả định, bạn hãy bỏ comment và sửa lại cho khớp với Project của bạn
// using PetCenterAPI.Models;
// using PetCenterAPI.Repository;
// using PetCenterAPI.Repository.Interface;
// using PetCenterAPI.Service;
// using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

namespace PetCenterTestProject.DiseaseTest
{
    public class DiseaseTest_DB
    {
        //=========================================================
        // Constructor
        //=========================================================
        public DiseaseTest_DB()
        {
        }

        //=========================================================
        // Create SQL Server Context
        //=========================================================

        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
        .UseSqlServer(
            "Server=.;" +
            "Database=PetCenter_Test;" + // Nhớ giữ tên này để Test không đụng DB thật
            "User Id=sa;" +
            "Password=123456;" +
            "TrustServerCertificate=True;" +
            "Trusted_Connection=True;",
            builder => builder.EnableRetryOnFailure()) // Vẫn nên giữ dòng này để xUnit chạy song song không bị rớt
        .Options;

            return new PetCenterContext(options);
        }

        //=========================================================
        // Create Repository
        //=========================================================

        private DiseaseRepository CreateRepository(PetCenterContext context)
        {
            return new DiseaseRepository(context);
        }

        //=========================================================
        // Create Service
        //=========================================================

        private DiseaseService CreateService(PetCenterContext context)
        {
            return new DiseaseService(
                CreateRepository(context),
                context);
        }

        //=========================================================
        // Clear Database
        //=========================================================

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.Diseases.RemoveRange(context.Diseases);
            await context.SaveChangesAsync();
        }

        //=========================================================
        // DTO Validation & Dummy Data Helpers
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

        private MutateDiseaseDTO CreateValidDiseaseDTO()
        {
            return new MutateDiseaseDTO
            {
                Name = "Parvo",
                Species = 1,
                Description = "A highly contagious viral illness.",
                Recommendation = "Vaccination and isolation."
            };
        }

        private Disease CreateDummyDisease(string name, int species)
        {
            return new Disease
            {
                DiseaseId = Guid.NewGuid(),
                Name = name,
                Species = species,
                IsActive = true,
                IsSystem = false,
                Description = "Dummy Description",
                Recommendation = "Dummy Recommendation",
                CreatedAt = DateTime.UtcNow
            };
        }

        //=====================================================================
        // Function: GetAllDiseasesQuery()
        // Test Requirement:
        // Verify that the function retrieves the disease list
        // according to the search specification.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetAllDiseasesQuery() returns disease list
        /// when keyword is empty and species is All.
        /// Expected:
        /// - Return disease list.
        /// </summary>
        [Fact]
        public async Task UTCID01_GetAllDiseasesQuery_KeywordEmpty_AllSpecies_ShouldReturnList()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.Add(CreateDummyDisease("Parvo", 1));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = service.GetAllDiseasesQuery().ToList();

            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        /// <summary>
        /// UTCID02
        /// Verify that GetAllDiseasesQuery() returns matches
        /// when keyword exists.
        /// Expected:
        /// - Return matching disease list.
        /// </summary>
        [Fact]
        public async Task UTCID02_GetAllDiseasesQuery_KeywordExists_ShouldReturnMatches()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.AddRange(
                CreateDummyDisease("Parvo", 1),
                CreateDummyDisease("Rabies", 2)
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var query = service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Name.Contains("Parvo")).ToList();

            Assert.Single(result);
            Assert.Equal("Parvo", result.First().Name);
        }

        /// <summary>
        /// UTCID03
        /// Verify that GetAllDiseasesQuery() returns empty list
        /// when keyword does not match any disease.
        /// Expected:
        /// - Return empty list.
        /// </summary>
        [Fact]
        public async Task UTCID03_GetAllDiseasesQuery_KeywordNotExists_ShouldReturnEmptyList()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.Add(CreateDummyDisease("Parvo", 1));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var query = service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Name.Contains("XYZ123")).ToList();

            Assert.Empty(result);
        }

        /// <summary>
        /// UTCID04
        /// Verify that GetAllDiseasesQuery() returns list
        /// when filtering by Dog species.
        /// Expected:
        /// - Return matching disease list.
        /// </summary>
        [Fact]
        public async Task UTCID04_GetAllDiseasesQuery_SpeciesDog_ShouldReturnList()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.AddRange(
                CreateDummyDisease("Dog Disease", 1),
                CreateDummyDisease("Cat Disease", 2)
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var query = service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 1).ToList();

            Assert.Single(result);
            Assert.Equal("Dog Disease", result.First().Name);
        }

        /// <summary>
        /// UTCID05
        /// Verify that GetAllDiseasesQuery() returns list
        /// when filtering by Cat species.
        /// Expected:
        /// - Return matching disease list.
        /// </summary>
        [Fact]
        public async Task UTCID05_GetAllDiseasesQuery_SpeciesCat_ShouldReturnList()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.Add(CreateDummyDisease("Cat Disease", 2));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var query = service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 2).ToList();

            Assert.NotEmpty(result);
        }

        /// <summary>
        /// UTCID06
        /// Verify that GetAllDiseasesQuery() returns list
        /// when filtering by Both species.
        /// Expected:
        /// - Return matching disease list.
        /// </summary>
        [Fact]
        public async Task UTCID06_GetAllDiseasesQuery_SpeciesBoth_ShouldReturnList()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            context.Diseases.Add(CreateDummyDisease("Both Disease", 3));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var query = service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 3).ToList();

            Assert.NotEmpty(result);
        }

        /// <summary>
        /// UTCID07
        /// Verify that GetAllDiseasesQuery() throws exception
        /// when Database throws exception.
        /// Expected:
        /// - Throw Exception.
        /// </summary>
        [Fact]
        public void UTCID07_GetAllDiseasesQuery_DatabaseThrowsException_ShouldThrow()
        {
            var mockRepo = new Mock<IDiseaseRepository>();
            var badContext = new PetCenterContext(new DbContextOptionsBuilder<PetCenterContext>().Options);

            var service = new DiseaseService(mockRepo.Object, badContext);

            Assert.Throws<InvalidOperationException>(() => service.GetAllDiseasesQuery().ToList());
        }

        //=====================================================================
        // Function: GetDiseaseByIdAsync()
        // Test Requirement:
        // Verify that the function returns disease information by DiseaseId.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify that GetDiseaseByIdAsync() returns disease information
        /// when DiseaseId exists.
        /// Expected:
        /// - Return disease details.
        /// </summary>
        [Fact]
        public async Task UTCID01_GetDiseaseById_ExistingGuid_ShouldReturnDetails()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("Rabies", 1);
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetDiseaseByIdAsync(disease.DiseaseId);

            Assert.NotNull(result);
            Assert.Equal("Rabies", result!.Name);
        }

        /// <summary>
        /// UTCID02
        /// Verify that GetDiseaseByIdAsync() returns null
        /// when DiseaseId does not exist.
        /// Expected:
        /// - Return null.
        /// </summary>
        [Fact]
        public async Task UTCID02_GetDiseaseById_NonExistingGuid_ShouldReturnNull()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var result = await service.GetDiseaseByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        /// <summary>
        /// UTCID03
        /// Verify that GetDiseaseByIdAsync() returns null
        /// when Disease is deleted or inactive.
        /// Expected:
        /// - Return null.
        /// </summary>
        [Fact]
        public async Task UTCID03_GetDiseaseById_DeletedOrInactive_ShouldReturnNull()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("Rabies", 1);
            disease.IsActive = false;
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetDiseaseByIdAsync(disease.DiseaseId);

            Assert.Null(result);
        }

        /// <summary>
        /// UTCID04
        /// Verify that GetDiseaseByIdAsync() throws exception
        /// when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// - Message = "Service Temporarily Unavailable".
        /// </summary>
        [Fact]
        public async Task UTCID04_GetDiseaseById_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            var repositoryMock = new Mock<IDiseaseRepository>();
            repositoryMock
                .Setup(x => x.GetDiseaseByIdAsync(id))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = new DiseaseService(repositoryMock.Object, CreateContext());

            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetDiseaseByIdAsync(id));

            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=====================================================================
        // Function: AddDiseaseAsync()
        // Test Requirement:
        // Verify that the function adds a new disease correctly.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify AddDiseaseAsync() when DiseaseName is empty.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID01_AddDisease_DiseaseNameEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Name = "";

            var result = Validate(dto);

            Assert.Contains(result, x => x.MemberNames.Contains("Name"));
        }

        /// <summary>
        /// UTCID02
        /// Verify AddDiseaseAsync() when species is Dog.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID02_AddDisease_SpeciesDog_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Species = 1;

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.NotNull(diseaseInDb);
            Assert.Equal(1, diseaseInDb!.Species);
        }

        /// <summary>
        /// UTCID03
        /// Verify AddDiseaseAsync() when species is Cat.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID03_AddDisease_SpeciesCat_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Species = 2;

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal(2, diseaseInDb!.Species);
        }

        /// <summary>
        /// UTCID04
        /// Verify AddDiseaseAsync() when species is Both.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID04_AddDisease_SpeciesBoth_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Species = 3;

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal(3, diseaseInDb!.Species);
        }

        /// <summary>
        /// UTCID05
        /// Verify AddDiseaseAsync() when Description is empty.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID05_AddDisease_DescriptionEmpty_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Description = "";

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal("", diseaseInDb!.Description);
        }

        /// <summary>
        /// UTCID06
        /// Verify AddDiseaseAsync() when Description is valid.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID06_AddDisease_DescriptionValid_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Description = "Valid Description";

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal("Valid Description", diseaseInDb!.Description);
        }

        /// <summary>
        /// UTCID07
        /// Verify AddDiseaseAsync() when Recommendation is empty.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID07_AddDisease_RecommendationEmpty_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Recommendation = "";

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal("", diseaseInDb!.Recommendation);
        }

        /// <summary>
        /// UTCID08
        /// Verify AddDiseaseAsync() when Recommendation is valid.
        /// Expected:
        /// - Disease added successfully.
        /// </summary>
        [Fact]
        public async Task UTCID08_AddDisease_RecommendationValid_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Recommendation = "Valid Recommendation";

            var result = await service.AddDiseaseAsync(dto);
            var diseaseInDb = await context.Diseases.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal("Valid Recommendation", diseaseInDb!.Recommendation);
        }

        /// <summary>
        /// UTCID09
        /// Verify AddDiseaseAsync() when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// </summary>
        [Fact]
        public async Task UTCID09_AddDisease_RepositoryThrowsException_ShouldThrow()
        {
            var repositoryMock = new Mock<IDiseaseRepository>();
            repositoryMock
                .Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>()))
                .ThrowsAsync(new Exception("DB Error"));

            var service = new DiseaseService(repositoryMock.Object, CreateContext());

            var ex = await Assert.ThrowsAsync<Exception>(() => service.AddDiseaseAsync(CreateValidDiseaseDTO()));

            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: UpdateDiseaseAsync()
        // Test Requirement:
        // Verify that UpdateDiseaseAsync() updates disease correctly.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify UpdateDiseaseAsync() when Name is empty.
        /// Expected:
        /// - Validation error.
        /// </summary>
        [Fact]
        public void UTCID01_UpdateDiseaseAsync_NameEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Name = "";

            var result = Validate(dto);

            Assert.Contains(result, x => x.MemberNames.Contains("Name"));
        }

        /// <summary>
        /// UTCID02
        /// Verify UpdateDiseaseAsync() when DiseaseNotFound.
        /// Expected:
        /// - Return false.
        /// </summary>
        [Fact]
        public async Task UTCID02_UpdateDiseaseAsync_DiseaseNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var result = await service.UpdateDiseaseAsync(Guid.NewGuid(), CreateValidDiseaseDTO());

            Assert.False(result);
        }

        /// <summary>
        /// UTCID03
        /// Verify UpdateDiseaseAsync() normal update.
        /// Expected:
        /// - Disease updated successfully.
        /// </summary>
        [Fact]
        public async Task UTCID03_UpdateDiseaseAsync_NormalUpdate_ShouldSuccess()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("Old Name", 1);
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Name = "Updated Name";

            var result = await service.UpdateDiseaseAsync(disease.DiseaseId, dto);
            var diseaseInDb = await context.Diseases.FindAsync(disease.DiseaseId);

            Assert.True(result);
            Assert.Equal("Updated Name", diseaseInDb!.Name);
        }

        /// <summary>
        /// UTCID04
        /// Verify UpdateDiseaseAsync() when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// </summary>
        [Fact]
        public async Task UTCID04_UpdateDiseaseAsync_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            var repositoryMock = new Mock<IDiseaseRepository>();
            repositoryMock
                .Setup(x => x.GetDiseaseByIdAsync(id))
                .ThrowsAsync(new Exception("DB Error"));

            var service = new DiseaseService(repositoryMock.Object, CreateContext());

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateDiseaseAsync(id, CreateValidDiseaseDTO()));

            Assert.Equal("DB Error", ex.Message);
        }

        /// <summary>
        /// UTCID05
        /// Verify UpdateDiseaseAsync() for System Disease.
        /// Expected:
        /// - Update successfully.
        /// </summary>
        [Fact]
        public async Task UTCID05_UpdateDiseaseAsync_SystemDisease_ShouldUpdateNormally()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("System Disease", 1);
            disease.IsSystem = true;
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDiseaseDTO();
            dto.Name = "New System Name";

            var result = await service.UpdateDiseaseAsync(disease.DiseaseId, dto);
            var diseaseInDb = await context.Diseases.FindAsync(disease.DiseaseId);

            Assert.True(result);
            Assert.Equal("New System Name", diseaseInDb!.Name);
        }

        //=====================================================================
        // Function: DeleteDiseaseAsync()
        // Test Requirement:
        // Verify that DeleteDiseaseAsync() soft deletes disease correctly.
        //=====================================================================

        /// <summary>
        /// UTCID01
        /// Verify DeleteDiseaseAsync() when DiseaseNotFound.
        /// Expected:
        /// - Return false.
        /// </summary>
        [Fact]
        public async Task UTCID01_DeleteDiseaseAsync_DiseaseNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var service = CreateService(context);

            var result = await service.DeleteDiseaseAsync(Guid.NewGuid());

            Assert.False(result);
        }

        /// <summary>
        /// UTCID02
        /// Verify DeleteDiseaseAsync() for System Disease.
        /// Expected:
        /// - Throw Exception.
        /// </summary>
        [Fact]
        public async Task UTCID02_DeleteDiseaseAsync_IsSystemTrue_ShouldThrowException()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("System Disease", 1);
            disease.IsSystem = true;
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteDiseaseAsync(disease.DiseaseId));

            Assert.Equal("Cannot delete system default diseases.", ex.Message);
        }

        /// <summary>
        /// UTCID03
        /// Verify DeleteDiseaseAsync() for Normal Disease.
        /// Expected:
        /// - Disease soft-deleted.
        /// </summary>
        [Fact]
        public async Task UTCID03_DeleteDiseaseAsync_NormalDisease_ShouldSoftDelete()
        {
            using var context = CreateContext();

            await ClearDatabaseAsync(context);

            var disease = CreateDummyDisease("Normal Disease", 1);
            disease.IsSystem = false;
            context.Diseases.Add(disease);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteDiseaseAsync(disease.DiseaseId);
            var diseaseInDb = await context.Diseases.FindAsync(disease.DiseaseId);

            Assert.True(result);
            Assert.False(diseaseInDb!.IsActive);
        }

        /// <summary>
        /// UTCID04
        /// Verify DeleteDiseaseAsync() when repository throws exception.
        /// Expected:
        /// - Throw Exception.
        /// </summary>
        [Fact]
        public async Task UTCID04_DeleteDiseaseAsync_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            var repositoryMock = new Mock<IDiseaseRepository>();

            repositoryMock
                .Setup(x => x.GetDiseaseByIdAsync(id))
                .ReturnsAsync(new Disease { DiseaseId = id, IsSystem = false });

            repositoryMock
                .Setup(x => x.UpdateDiseaseAsync(It.IsAny<Disease>()))
                .Throws(new Exception("DB Error"));

            var service = new DiseaseService(repositoryMock.Object, CreateContext());

            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteDiseaseAsync(id));

            Assert.Equal("DB Error", ex.Message);
        }
    }
}