using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using PetCenterAPI.Service.Interface;
using Xunit;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterTestProject.PetTest
{
    public class PetTest_DB
    {
        //=========================================================
        // Mock external service only
        //=========================================================
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        //=========================================================
        // Constructor
        //=========================================================
        public PetTest_DB()
        {
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        }

        //=========================================================
        // Create SQL Server Context
        //=========================================================
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;Database=PetCenter_Test;User Id=sa;Password=123456;TrustServerCertificate=True;",
                    builder => builder.EnableRetryOnFailure()) // <--- THÊM DÒNG NÀY
                .Options;

            return new PetCenterContext(options);
        }

        //=========================================================
        // Create Repository & Service
        //=========================================================
        private PetRepository CreateRepository(PetCenterContext context)
        {
            return new PetRepository(context);
        }

        private PetService CreateService(PetCenterContext context)
        {
            return new PetService(
                CreateRepository(context),
                context,
                _cloudinaryServiceMock.Object);
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.Pets.RemoveRange(context.Pets);
            await context.SaveChangesAsync();
        }

        //=========================================================
        // DTO Validation Helper
        //=========================================================
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
                PetName = new string('A', 101),
                Species = "Dog",
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
                Gender = new string('A', 51)
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
                Note = new string('A', 1001)
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Note"));
        }

        [Fact]
        public void UTCID04_AddPetAsync_WeightNegative_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = "Milo",
                Species = "Dog",
                Breed = "Poodle",
                Gender = "Male",
                Weight = -1
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public void UTCID05_AddPetAsync_WeightOverLimit_ShouldFailValidation()
        {
            var dto = new MutatePetDTO
            {
                PetName = "Milo",
                Species = "Dog",
                Breed = "Poodle",
                Gender = "Male",
                Weight = 10001
            };
            var result = Validate(dto);
            Assert.Contains(result, x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public async Task UTCID06_AddPetAsync_ValidWithoutImage_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var customerId = Guid.NewGuid();
            var dto = new MutatePetDTO { PetName = "Milo", Species = "Dog", Breed = "Poodle", Gender = "Male", Weight = 10m };

            var result = await service.AddPetAsync(customerId, dto);
            var petInDb = await context.Pets.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.NotNull(petInDb);
            Assert.Equal("Milo", petInDb!.PetName);
            Assert.Null(petInDb.PetAvatar);
        }

        [Fact]
        public async Task UTCID07_AddPetAsync_ValidWithImage_ShouldUploadAndSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var customerId = Guid.NewGuid();
            var stream = new MemoryStream(new byte[100]);
            var file = new FormFile(stream, 0, 100, "ImageFile", "avatar.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new MutatePetDTO { PetName = "Milo", ImageFile = file };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "pets"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/img.jpg") });

            var result = await service.AddPetAsync(customerId, dto);
            var petInDb = await context.Pets.FirstOrDefaultAsync();

            Assert.True(result);
            Assert.Equal("https://demo.com/img.jpg", petInDb!.PetAvatar);
        }

        [Fact]
        public async Task UTCID08_AddPetAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IPetRepository>();
            repoMock.Setup(x => x.AddPetAsync(It.IsAny<Pet>())).ThrowsAsync(new Exception("DB Error"));

            using var context = CreateContext(); // Dùng tạm context rỗng cho service
            var service = new PetService(repoMock.Object, context, _cloudinaryServiceMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.AddPetAsync(Guid.NewGuid(), new MutatePetDTO { PetName = "Milo" }));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: UpdatePetAsync() (7 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_UpdatePetAsync_CustomerRole_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, PetName = "Old", IsActive = true };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new MutatePetDTO { PetName = "New" };

            var result = await service.UpdatePetAsync(pet.PetId, customerId, dto, isVet: false);
            var petInDb = await context.Pets.FindAsync(pet.PetId);

            Assert.True(result);
            Assert.Equal("New", petInDb!.PetName);
        }

        [Fact]
        public async Task UTCID02_UpdatePetAsync_VetRole_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = Guid.NewGuid(), PetName = "Old" };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new MutatePetDTO { PetName = "New" };

            var result = await service.UpdatePetAsync(pet.PetId, Guid.Empty, dto, isVet: true);
            var petInDb = await context.Pets.FindAsync(pet.PetId);

            Assert.True(result);
            Assert.Equal("New", petInDb!.PetName);
        }

        [Fact]
        public async Task UTCID03_UpdatePetAsync_PetNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.UpdatePetAsync(Guid.NewGuid(), Guid.NewGuid(), new MutatePetDTO(), isVet: false);
            Assert.False(result);
        }

        [Fact]
        public void UTCID04_UpdatePetAsync_NameExceedsLength_ShouldFailValidation()
        {
            var dto = new MutatePetDTO { PetName = new string('A', 101), Species = "Dog", Breed = "Poodle", Gender = "Male" };
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("PetName"));
        }

        [Fact]
        public void UTCID05_UpdatePetAsync_WeightOutOfRange_ShouldFailValidation()
        {
            var dto = new MutatePetDTO { PetName = "Milo", Species = "Dog", Breed = "Poodle", Gender = "Male", Weight = -5 };
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Weight"));
        }

        [Fact]
        public async Task UTCID06_UpdatePetAsync_NoNewImage_ShouldKeepOldAvatar()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, PetAvatar = "old.jpg" };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new MutatePetDTO { PetName = "New" };

            await service.UpdatePetAsync(pet.PetId, customerId, dto, isVet: false);
            var petInDb = await context.Pets.FindAsync(pet.PetId);

            Assert.Equal("old.jpg", petInDb!.PetAvatar);
        }

        [Fact]
        public async Task UTCID07_UpdatePetAsync_WithNewImage_ShouldUpload()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = customerId };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var file = new FormFile(new MemoryStream(), 0, 10, "Img", "new.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
            var dto = new MutatePetDTO { ImageFile = file };

            _cloudinaryServiceMock.Setup(x => x.UploadImageAsync(file, "pets"))
                .ReturnsAsync(new ImageUploadResult { StatusCode = HttpStatusCode.OK, SecureUrl = new Uri("https://demo.com/new.jpg") });

            var service = CreateService(context);
            await service.UpdatePetAsync(pet.PetId, customerId, dto, isVet: false);

            var petInDb = await context.Pets.FindAsync(pet.PetId);
            Assert.Equal("https://demo.com/new.jpg", petInDb!.PetAvatar);
        }

        //=====================================================================
        // Function: DeletePetAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_DeletePetAsync_CustomerRole_ShouldSoftDelete()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, IsActive = true };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.DeletePetAsync(pet.PetId, customerId, isVet: false);
            var petInDb = await context.Pets.FindAsync(pet.PetId);

            Assert.True(result);
            Assert.False(petInDb!.IsActive);
        }

        [Fact]
        public async Task UTCID02_DeletePetAsync_CustomerRole_PetNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.DeletePetAsync(Guid.NewGuid(), Guid.NewGuid(), isVet: false);
            Assert.False(result);
        }

        [Fact]
        public async Task UTCID03_DeletePetAsync_VetRole_ShouldSoftDelete()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = Guid.NewGuid(), IsActive = true };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.DeletePetAsync(pet.PetId, Guid.Empty, isVet: true);
            var petInDb = await context.Pets.FindAsync(pet.PetId);

            Assert.True(result);
            Assert.False(petInDb!.IsActive);
        }

        [Fact]
        public async Task UTCID04_DeletePetAsync_VetRole_PetNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.DeletePetAsync(Guid.NewGuid(), Guid.Empty, isVet: true);
            Assert.False(result);
        }

        [Fact]
        public async Task UTCID05_DeletePetAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IPetRepository>();
            repoMock.Setup(x => x.GetPetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(new Pet());
            repoMock.Setup(x => x.UpdatePetAsync(It.IsAny<Pet>())).ThrowsAsync(new Exception("DB Error"));

            using var context = CreateContext();
            var service = new PetService(repoMock.Object, context, _cloudinaryServiceMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeletePetAsync(Guid.NewGuid(), Guid.NewGuid(), false));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: GetMyPetsQuery() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetMyPetsQuery_HasActivePets_ShouldReturnList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            context.Pets.Add(new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, PetName = "Milo", IsActive = true });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetMyPetsQuery(customerId).ToListAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID02_GetMyPetsQuery_HasOnlyInactivePets_ShouldReturnEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            context.Pets.Add(new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, IsActive = false });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetMyPetsQuery(customerId).ToListAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetMyPetsQuery_NoPets_ShouldReturnEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var result = await service.GetMyPetsQuery(Guid.NewGuid()).ToListAsync();

            Assert.Empty(result);
        }

        //=====================================================================
        // Function: GetPetDetailsAsync() (4 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetPetDetailsAsync_OwnedByCustomer_ShouldReturnDetail()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = customerId, PetName = "Milo" };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetPetDetailsAsync(pet.PetId, customerId);

            Assert.NotNull(result);
            Assert.Equal("Milo", result!.PetName);
        }

        [Fact]
        public async Task UTCID02_GetPetDetailsAsync_NotOwnedByCustomer_ShouldReturnNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var pet = new Pet { PetId = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetPetDetailsAsync(pet.PetId, Guid.NewGuid()); // Wrong CustomerId

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetPetDetailsAsync_InvalidPetId_ShouldReturnNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.GetPetDetailsAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID04_GetPetDetailsAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IPetRepository>();
            repoMock.Setup(x => x.GetPetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

            using var context = CreateContext();
            var service = new PetService(repoMock.Object, context, _cloudinaryServiceMock.Object);

            await Assert.ThrowsAsync<Exception>(() => service.GetPetDetailsAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        //=====================================================================
        // Function: GetAllPetsForVetQuery() (2 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetAllPetsForVetQuery_SystemHasActivePets_ShouldReturnList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = new Customer { CustomerId = Guid.NewGuid(), FullName = "Owner" };
            context.Pets.Add(new Pet { PetId = Guid.NewGuid(), Customer = customer, IsActive = true });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetAllPetsForVetQuery().ToListAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID02_GetAllPetsForVetQuery_SystemHasNoActivePets_ShouldReturnEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customer = new Customer { CustomerId = Guid.NewGuid(), FullName = "Owner" };
            context.Pets.Add(new Pet { PetId = Guid.NewGuid(), Customer = customer, IsActive = false }); // Inactive
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetAllPetsForVetQuery().ToListAsync();

            Assert.Empty(result);
        }

        //=====================================================================
        // Function: GetPetDetailForVetAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetPetDetailForVetAsync_ValidPetId_ShouldReturnDetail()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var pet = new Pet { PetId = Guid.NewGuid(), Customer = new Customer { FullName = "Vet" } };
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetPetDetailForVetAsync(pet.PetId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UTCID02_GetPetDetailForVetAsync_InvalidPetId_ShouldReturnNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.GetPetDetailForVetAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetPetDetailForVetAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IPetRepository>();
            repoMock.Setup(x => x.GetPetByIdWithOwnerAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

            using var context = CreateContext();
            var service = new PetService(repoMock.Object, context, _cloudinaryServiceMock.Object);

            await Assert.ThrowsAsync<Exception>(() => service.GetPetDetailForVetAsync(Guid.NewGuid()));
        }
    }
}