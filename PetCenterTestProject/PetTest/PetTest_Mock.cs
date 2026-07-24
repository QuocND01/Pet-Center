using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
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
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterTestProject.PetTest
{
    public class PetTest_Mock
    {
        private readonly Mock<IPetRepository> _petRepositoryMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly PetCenterContext _dbContext;
        private readonly PetService _service;

        public PetTest_Mock()
        {
            _petRepositoryMock = new Mock<IPetRepository>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PetCenterContext(options);

            _service = new PetService(
                _petRepositoryMock.Object,
                _dbContext,
                _cloudinaryServiceMock.Object);
        }

        private async Task ClearDatabaseAsync()
        {
            _dbContext.Pets.RemoveRange(_dbContext.Pets);
            await _dbContext.SaveChangesAsync();
        }

        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        //=====================================================================
        // Function: AddPetAsync() (8 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_AddPetAsync_NameTooLong_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = new string('A', 101), // Cố tình làm sai độ dài
                Species = "Dog",                // Cấp data hợp lệ cho các trường Required
                Breed = "Poodle",
                Gender = "Male"
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("PetName"));
        }

        [Fact]
        public void UTCID02_AddPetAsync_GenderTooLong_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = "Milo",
                Species = "Dog",
                Breed = "Poodle",
                Gender = new string('A', 51) // Cố tình làm sai
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Gender"));
        }

        [Fact]
        public void UTCID03_AddPetAsync_NoteTooLong_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = "Milo",
                Species = "Dog",
                Breed = "Poodle",
                Gender = "Male",
                Note = new string('A', 1001) // Cố tình làm sai
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Note"));
        }

        [Fact]
        public void UTCID04_AddPetAsync_WeightNegative_ShouldFailValidation()
        {
            var dto = new MutatePetDTO { Weight = -1 };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public void UTCID05_AddPetAsync_WeightOverLimit_ShouldFailValidation()
        {
            var dto = new MutatePetDTO { Weight = 10001 };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public async Task UTCID06_AddPetAsync_ValidWithoutImage_ShouldSuccess()
        {
            var customerId = Guid.NewGuid();
            var dto = new MutatePetDTO { PetName = "Milo", Weight = 10m };

            _petRepositoryMock.Setup(x => x.AddPetAsync(It.IsAny<Pet>())).Returns(Task.CompletedTask);

            var result = await _service.AddPetAsync(customerId, dto);

            Assert.True(result);
            _petRepositoryMock.Verify(x => x.AddPetAsync(It.Is<Pet>(p => p.PetName == "Milo" && p.PetAvatar == null)), Times.Once);
        }

        [Fact]
        public async Task UTCID07_AddPetAsync_ValidWithImage_ShouldUploadAndSuccess()
        {
            var customerId = Guid.NewGuid();
            var stream = new MemoryStream(new byte[100]);
            var file = new FormFile(stream, 0, 100, "ImageFile", "avatar.jpg");

            var dto = new MutatePetDTO { PetName = "Milo", ImageFile = file };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "pets"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/img.jpg") });

            var result = await _service.AddPetAsync(customerId, dto);

            Assert.True(result);
            _petRepositoryMock.Verify(x => x.AddPetAsync(It.Is<Pet>(p => p.PetAvatar == "https://demo.com/img.jpg")), Times.Once);
        }

        [Fact]
        public async Task UTCID08_AddPetAsync_RepositoryThrowsException()
        {
            var customerId = Guid.NewGuid();
            var dto = new MutatePetDTO { PetName = "Milo" };

            _petRepositoryMock.Setup(x => x.AddPetAsync(It.IsAny<Pet>()))
                .ThrowsAsync(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddPetAsync(customerId, dto));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: UpdatePetAsync() (7 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_UpdatePetAsync_CustomerRole_ShouldSuccess()
        {
            var petId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId, PetName = "Old", CustomerId = customerId };
            var dto = new MutatePetDTO { PetName = "New" };

            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, customerId)).ReturnsAsync(existingPet);

            var result = await _service.UpdatePetAsync(petId, customerId, dto, isVet: false);

            Assert.True(result);
            Assert.Equal("New", existingPet.PetName);
        }

        [Fact]
        public async Task UTCID02_UpdatePetAsync_VetRole_ShouldSuccess()
        {
            var petId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId, PetName = "Old" };
            var dto = new MutatePetDTO { PetName = "New" };

            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(petId)).ReturnsAsync(existingPet);

            var result = await _service.UpdatePetAsync(petId, Guid.Empty, dto, isVet: true);

            Assert.True(result);
            Assert.Equal("New", existingPet.PetName);
        }

        [Fact]
        public async Task UTCID03_UpdatePetAsync_PetNotFound_ShouldReturnFalse()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

            var result = await _service.UpdatePetAsync(petId, Guid.NewGuid(), new MutatePetDTO(), isVet: false);

            Assert.False(result);
        }

        [Fact]
        public void UTCID04_UpdatePetAsync_NameExceedsLength_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = new string('A', 101), // Cố tình làm sai
                Species = "Dog",
                Breed = "Poodle",
                Gender = "Male"
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("PetName"));
        }

        [Fact]
        public void UTCID05_UpdatePetAsync_WeightOutOfRange_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = "Milo",
                Species = "Dog",
                Breed = "Poodle",
                Gender = "Male",
                Weight = -5 // Cố tình làm sai
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public async Task UTCID06_UpdatePetAsync_NoNewImage_ShouldKeepOldAvatar()
        {
            var petId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId, PetAvatar = "old.jpg" };
            var dto = new MutatePetDTO { PetName = "New" }; // ImageFile = null

            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, customerId)).ReturnsAsync(existingPet);

            await _service.UpdatePetAsync(petId, customerId, dto, isVet: false);

            Assert.Equal("old.jpg", existingPet.PetAvatar);
            _cloudinaryServiceMock.Verify(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UTCID07_UpdatePetAsync_WithNewImage_ShouldUpload()
        {
            var petId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId };
            var file = new FormFile(new MemoryStream(), 0, 10, "Img", "new.jpg");
            var dto = new MutatePetDTO { ImageFile = file };

            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, It.IsAny<Guid>())).ReturnsAsync(existingPet);
            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "pets"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/new.jpg") });

            await _service.UpdatePetAsync(petId, Guid.NewGuid(), dto, isVet: false);

            Assert.Equal("https://demo.com/new.jpg", existingPet.PetAvatar);
        }

        //=====================================================================
        // Function: DeletePetAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_DeletePetAsync_CustomerRole_ShouldSoftDelete()
        {
            var petId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId, IsActive = true };

            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, customerId)).ReturnsAsync(existingPet);

            var result = await _service.DeletePetAsync(petId, customerId, isVet: false);

            Assert.True(result);
            Assert.False(existingPet.IsActive);
        }

        [Fact]
        public async Task UTCID02_DeletePetAsync_CustomerRole_PetNotFound_ShouldReturnFalse()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

            var result = await _service.DeletePetAsync(petId, Guid.NewGuid(), isVet: false);

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID03_DeletePetAsync_VetRole_ShouldSoftDelete()
        {
            var petId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId, IsActive = true };

            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(petId)).ReturnsAsync(existingPet);

            var result = await _service.DeletePetAsync(petId, Guid.Empty, isVet: true);

            Assert.True(result);
            Assert.False(existingPet.IsActive);
        }

        [Fact]
        public async Task UTCID04_DeletePetAsync_VetRole_PetNotFound_ShouldReturnFalse()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(petId)).ReturnsAsync((Pet?)null);

            var result = await _service.DeletePetAsync(petId, Guid.Empty, isVet: true);

            Assert.False(result);
        }

        [Fact]
        public async Task UTCID05_DeletePetAsync_RepositoryThrowsException()
        {
            var petId = Guid.NewGuid();
            var existingPet = new Pet { PetId = petId };

            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, It.IsAny<Guid>())).ReturnsAsync(existingPet);
            _petRepositoryMock.Setup(x => x.UpdatePetAsync(It.IsAny<Pet>())).Throws(new Exception("DB Error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeletePetAsync(petId, Guid.NewGuid(), false));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: GetMyPetsQuery() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetMyPetsQuery_HasActivePets_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            var customerId = Guid.NewGuid();
            _dbContext.Pets.Add(new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, IsActive = true });
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetMyPetsQuery(customerId).ToListAsync();
            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID02_GetMyPetsQuery_HasOnlyInactivePets_ShouldReturnEmpty()
        {
            await ClearDatabaseAsync();
            var customerId = Guid.NewGuid();
            _dbContext.Pets.Add(new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, IsActive = false });
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetMyPetsQuery(customerId).ToListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetMyPetsQuery_NoPets_ShouldReturnEmpty()
        {
            await ClearDatabaseAsync();
            var result = await _service.GetMyPetsQuery(Guid.NewGuid()).ToListAsync();
            Assert.Empty(result);
        }

        //=====================================================================
        // Function: GetPetDetailsAsync() (4 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetPetDetailsAsync_OwnedByCustomer_ShouldReturnDetail()
        {
            var petId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, customerId))
                .ReturnsAsync(new Pet { PetId = petId, PetName = "Milo" });

            var result = await _service.GetPetDetailsAsync(petId, customerId);

            Assert.NotNull(result);
            Assert.Equal("Milo", result!.PetName);
        }

        [Fact]
        public async Task UTCID02_GetPetDetailsAsync_NotOwnedByCustomer_ShouldReturnNull()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(petId, It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

            var result = await _service.GetPetDetailsAsync(petId, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetPetDetailsAsync_InvalidPetId_ShouldReturnNull()
        {
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

            var result = await _service.GetPetDetailsAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID04_GetPetDetailsAsync_RepositoryThrowsException()
        {
            _petRepositoryMock.Setup(x => x.GetPetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetPetDetailsAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        //=====================================================================
        // Function: GetAllPetsForVetQuery() (2 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetAllPetsForVetQuery_SystemHasActivePets_ShouldReturnList()
        {
            await ClearDatabaseAsync();
            var customer = new Customer { CustomerId = Guid.NewGuid(), FullName = "Owner" };
            _dbContext.Pets.Add(new Pet { PetId = Guid.NewGuid(), Customer = customer, IsActive = true });
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetAllPetsForVetQuery().ToListAsync();

            Assert.Single(result);
            Assert.Equal("Owner", result.First().OwnerName);
        }

        [Fact]
        public async Task UTCID02_GetAllPetsForVetQuery_SystemHasNoActivePets_ShouldReturnEmpty()
        {
            await ClearDatabaseAsync();
            var customer = new Customer { CustomerId = Guid.NewGuid(), FullName = "Owner" };
            _dbContext.Pets.Add(new Pet { PetId = Guid.NewGuid(), Customer = customer, IsActive = false }); // Inactive
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetAllPetsForVetQuery().ToListAsync();

            Assert.Empty(result);
        }

        //=====================================================================
        // Function: GetPetDetailForVetAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetPetDetailForVetAsync_ValidPetId_ShouldReturnDetail()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(petId))
                .ReturnsAsync(new Pet { PetId = petId, Customer = new Customer { FullName = "Vet" } });

            var result = await _service.GetPetDetailForVetAsync(petId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UTCID02_GetPetDetailForVetAsync_InvalidPetId_ShouldReturnNull()
        {
            var petId = Guid.NewGuid();
            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(petId)).ReturnsAsync((Pet?)null);

            var result = await _service.GetPetDetailForVetAsync(petId);

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetPetDetailForVetAsync_RepositoryThrowsException()
        {
            _petRepositoryMock.Setup(x => x.GetPetByIdWithOwnerAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetPetDetailForVetAsync(Guid.NewGuid()));
        }
    }
}