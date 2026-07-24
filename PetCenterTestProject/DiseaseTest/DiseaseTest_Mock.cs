using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;
using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

// Dưới đây là các using giả định, bạn hãy bỏ comment và sửa lại cho khớp với cấu trúc thư mục của Project
// using PetCenterAPI.Models;
// using PetCenterAPI.Repository.Interface;
// using PetCenterAPI.Service;
// using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

namespace PetCenterTestProject.DiseaseTest
{
    public class DiseaseTest_Mock
    {
        //=========================================================
        // Mock & Setup
        //=========================================================
        private readonly Mock<IDiseaseRepository> _diseaseRepositoryMock;
        private readonly PetCenterContext _dbContext;
        private readonly DiseaseService _service;

        public DiseaseTest_Mock()
        {
            _diseaseRepositoryMock = new Mock<IDiseaseRepository>();

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PetCenterContext(options);

            _service = new DiseaseService(_diseaseRepositoryMock.Object, _dbContext);
        }

        private async Task ClearDatabaseAsync()
        {
            _dbContext.Diseases.RemoveRange(_dbContext.Diseases);
            await _dbContext.SaveChangesAsync();
        }

        //=========================================================
        // Helpers
        //=========================================================
        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
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
                Description = "Dummy Description",     // Đã bổ sung để fix lỗi DbUpdateException
                Recommendation = "Dummy Recommendation" // Đã bổ sung để fix lỗi DbUpdateException
            };
        }

        //=====================================================================
        // Function: GetAllDiseasesQuery() (7 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetAllDiseasesQuery_KeywordEmpty_AllSpecies_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.Add(CreateDummyDisease("Parvo", 1));
            await _dbContext.SaveChangesAsync();

            var result = _service.GetAllDiseasesQuery().ToList();

            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID02_GetAllDiseasesQuery_KeywordExists_ShouldReturnMatches()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.AddRange(
                CreateDummyDisease("Parvo", 1),
                CreateDummyDisease("Rabies", 2)
            );
            await _dbContext.SaveChangesAsync();

            var query = _service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Name.Contains("Parvo")).ToList();

            Assert.Single(result);
            Assert.Equal("Parvo", result.First().Name);
        }

        [Fact]
        public async Task UTCID03_GetAllDiseasesQuery_KeywordNotExists_ShouldReturnEmptyList()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.Add(CreateDummyDisease("Parvo", 1));
            await _dbContext.SaveChangesAsync();

            var query = _service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Name.Contains("XYZ123")).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID04_GetAllDiseasesQuery_SpeciesDog_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.AddRange(
                CreateDummyDisease("Dog Disease", 1),
                CreateDummyDisease("Cat Disease", 2)
            );
            await _dbContext.SaveChangesAsync();

            var query = _service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 1).ToList();

            Assert.Single(result);
            Assert.Equal("Dog Disease", result.First().Name);
        }

        [Fact]
        public async Task UTCID05_GetAllDiseasesQuery_SpeciesCat_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.Add(CreateDummyDisease("Cat Disease", 2));
            await _dbContext.SaveChangesAsync();

            var query = _service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 2).ToList();

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID06_GetAllDiseasesQuery_SpeciesBoth_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            _dbContext.Diseases.Add(CreateDummyDisease("Both Disease", 3));
            await _dbContext.SaveChangesAsync();

            var query = _service.GetAllDiseasesQuery();
            var result = query.Where(d => d.Species == 3).ToList();

            Assert.NotEmpty(result);
        }

        [Fact]
        public void UTCID07_GetAllDiseasesQuery_DatabaseThrowsException_ShouldThrow()
        {
            var mockRepo = new Mock<IDiseaseRepository>();
            var options = new DbContextOptionsBuilder<PetCenterContext>().Options;
            var badContext = new PetCenterContext(options);

            var service = new DiseaseService(mockRepo.Object, badContext);

            Assert.Throws<InvalidOperationException>(() => service.GetAllDiseasesQuery().ToList());
        }

        //=====================================================================
        // Function: GetDiseaseByIdAsync() (4 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetDiseaseById_ExistingGuid_ShouldReturnDetails()
        {
            var id = Guid.NewGuid();
            var disease = CreateDummyDisease("Rabies", 1);
            disease.DiseaseId = id;

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(disease);

            var result = await _service.GetDiseaseByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Rabies", result!.Name);
        }

        [Fact]
        public async Task UTCID02_GetDiseaseById_NonExistingGuid_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync((Disease?)null);

            var result = await _service.GetDiseaseByIdAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetDiseaseById_DeletedOrInactive_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync((Disease?)null);

            var result = await _service.GetDiseaseByIdAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID04_GetDiseaseById_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetDiseaseByIdAsync(id));
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=====================================================================
        // Function: AddDiseaseAsync() (9 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_AddDisease_DiseaseNameEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Name = "";

            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Name"));
        }

        [Fact]
        public async Task UTCID02_AddDisease_SpeciesDog_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Species = 1;

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Species == 1)), Times.Once);
        }

        [Fact]
        public async Task UTCID03_AddDisease_SpeciesCat_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Species = 2;

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Species == 2)), Times.Once);
        }

        [Fact]
        public async Task UTCID04_AddDisease_SpeciesBoth_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Species = 3;

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Species == 3)), Times.Once);
        }

        [Fact]
        public async Task UTCID05_AddDisease_DescriptionEmpty_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Description = "";

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Description == "")), Times.Once);
        }

        [Fact]
        public async Task UTCID06_AddDisease_DescriptionValid_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Description = "Valid Description";

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Description == "Valid Description")), Times.Once);
        }

        [Fact]
        public async Task UTCID07_AddDisease_RecommendationEmpty_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Recommendation = "";

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Recommendation == "")), Times.Once);
        }

        [Fact]
        public async Task UTCID08_AddDisease_RecommendationValid_ShouldSuccess()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Recommendation = "Valid Recommendation";

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.AddDiseaseAsync(dto);

            Assert.True(result);
            _diseaseRepositoryMock.Verify(x => x.AddDiseaseAsync(It.Is<Disease>(d => d.Recommendation == "Valid Recommendation")), Times.Once);
        }

        [Fact]
        public async Task UTCID09_AddDisease_RepositoryThrowsException_ShouldThrow()
        {
            var dto = CreateValidDiseaseDTO();

            _diseaseRepositoryMock.Setup(x => x.AddDiseaseAsync(It.IsAny<Disease>()))
                .ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddDiseaseAsync(dto));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: UpdateDiseaseAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_UpdateDiseaseAsync_NameEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDiseaseDTO();
            dto.Name = "";
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Name"));
        }

        [Fact]
        public async Task UTCID02_UpdateDiseaseAsync_DiseaseNotFound_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();
            var dto = CreateValidDiseaseDTO();

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync((Disease?)null);

            var result = await _service.UpdateDiseaseAsync(id, dto);

            Assert.False(result);
            _diseaseRepositoryMock.Verify(x => x.UpdateDiseaseAsync(It.IsAny<Disease>()), Times.Never);
        }

        [Fact]
        public async Task UTCID03_UpdateDiseaseAsync_NormalUpdate_ShouldSuccess()
        {
            var id = Guid.NewGuid();
            var existingDisease = CreateDummyDisease("Old Name", 1);
            existingDisease.DiseaseId = id;

            var dto = CreateValidDiseaseDTO();
            dto.Name = "Updated Name";

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(existingDisease);
            _diseaseRepositoryMock.Setup(x => x.UpdateDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.UpdateDiseaseAsync(id, dto);

            Assert.True(result);
            Assert.Equal("Updated Name", existingDisease.Name);
            _diseaseRepositoryMock.Verify(x => x.UpdateDiseaseAsync(existingDisease), Times.Once);
        }

        [Fact]
        public async Task UTCID04_UpdateDiseaseAsync_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            var dto = CreateValidDiseaseDTO();

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateDiseaseAsync(id, dto));
            Assert.Equal("DB Error", ex.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateDiseaseAsync_SystemDisease_ShouldUpdateNormally()
        {
            var id = Guid.NewGuid();
            var existingDisease = CreateDummyDisease("System Disease", 1);
            existingDisease.DiseaseId = id;
            existingDisease.IsSystem = true;

            var dto = CreateValidDiseaseDTO();

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(existingDisease);

            var result = await _service.UpdateDiseaseAsync(id, dto);

            Assert.True(result);
        }

        //=====================================================================
        // Function: DeleteDiseaseAsync() (4 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_DeleteDiseaseAsync_DiseaseNotFound_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();
            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync((Disease?)null);

            var result = await _service.DeleteDiseaseAsync(id);

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID02_DeleteDiseaseAsync_IsSystemTrue_ShouldThrowException()
        {
            var id = Guid.NewGuid();
            var systemDisease = CreateDummyDisease("System Disease", 1);
            systemDisease.DiseaseId = id;
            systemDisease.IsSystem = true;

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(systemDisease);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteDiseaseAsync(id));

            Assert.Equal("Cannot delete system default diseases.", ex.Message);
            _diseaseRepositoryMock.Verify(x => x.UpdateDiseaseAsync(It.IsAny<Disease>()), Times.Never);
        }

        [Fact]
        public async Task UTCID03_DeleteDiseaseAsync_NormalDisease_ShouldSoftDelete()
        {
            var id = Guid.NewGuid();
            var normalDisease = CreateDummyDisease("Normal Disease", 1);
            normalDisease.DiseaseId = id;
            normalDisease.IsSystem = false;
            normalDisease.IsActive = true;

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(normalDisease);
            _diseaseRepositoryMock.Setup(x => x.UpdateDiseaseAsync(It.IsAny<Disease>())).Returns(Task.CompletedTask);

            var result = await _service.DeleteDiseaseAsync(id);

            Assert.True(result);
            Assert.False(normalDisease.IsActive);
            _diseaseRepositoryMock.Verify(x => x.UpdateDiseaseAsync(normalDisease), Times.Once);
        }

        [Fact]
        public async Task UTCID04_DeleteDiseaseAsync_RepositoryThrowsException_ShouldThrow()
        {
            var id = Guid.NewGuid();
            var normalDisease = CreateDummyDisease("Normal Disease", 1);
            normalDisease.DiseaseId = id;
            normalDisease.IsSystem = false;

            _diseaseRepositoryMock.Setup(x => x.GetDiseaseByIdAsync(id)).ReturnsAsync(normalDisease);
            _diseaseRepositoryMock.Setup(x => x.UpdateDiseaseAsync(It.IsAny<Disease>())).Throws(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteDiseaseAsync(id));
            Assert.Equal("DB Error", ex.Message);
        }
    }
}