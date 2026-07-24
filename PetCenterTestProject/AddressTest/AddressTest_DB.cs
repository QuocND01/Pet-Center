using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.AddressRequestDTO;

namespace PetCenterTestProject.AddressTest
{
    public class AddressTest_DB
    {
        //=========================================================
        // Constructor
        //=========================================================
        public AddressTest_DB()
        {
        }

        //=========================================================
        // Create InMemory Context
        //=========================================================
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PetCenterContext(options);
        }

        //=========================================================
        // Create Repository & Service
        //=========================================================
        private AddressRepository CreateRepository(PetCenterContext context)
        {
            return new AddressRepository(context);
        }

        private AddressService CreateService(PetCenterContext context)
        {
            return new AddressService(CreateRepository(context));
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.Addresses.RemoveRange(context.Addresses);
            await context.SaveChangesAsync();
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

        private MutateAddressDTO CreateValidDTO()
        {
            return new MutateAddressDTO
            {
                Province = "Can Tho",
                District = "Ninh Kieu",
                Ward = "Xuan Khanh",
                AddressDetails = "FPT University",
                IsDefault = false
            };
        }

        // Helper sinh entity hợp lệ để nạp vào InMemory DB không bị lỗi Required
        private Address CreateValidAddressEntity(Guid customerId, bool isDefault = false)
        {
            return new Address
            {
                AddressId = Guid.NewGuid(),
                CustomerId = customerId,
                Province = "Can Tho",
                District = "Ninh Kieu",
                Ward = "Xuan Khanh",
                AddressDetails = "FPT University",
                IsDefault = isDefault,
                IsActive = true
            };
        }

        //=====================================================================
        // Function: AddAddressAsync() (9 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_AddAddressAsync_ProvinceEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID02_AddAddressAsync_ProvinceTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID03_AddAddressAsync_DistrictEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID04_AddAddressAsync_DistrictTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID05_AddAddressAsync_AddressDetailsEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public void UTCID06_AddAddressAsync_AddressDetailsTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = new string('A', 301);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public async Task UTCID07_AddAddressAsync_NoActiveAddress_ShouldAutoForceDefault()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var customerId = Guid.NewGuid();
            var dto = CreateValidDTO();
            dto.IsDefault = false;

            var result = await service.AddAddressAsync(customerId, dto);
            var addressInDb = await context.Addresses.FirstOrDefaultAsync(a => a.CustomerId == customerId);

            Assert.True(result);
            Assert.NotNull(addressInDb);
            Assert.True(addressInDb!.IsDefault);
        }

        [Fact]
        public async Task UTCID08_AddAddressAsync_HasActiveAddress_ShouldSaveNormally()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            context.Addresses.Add(CreateValidAddressEntity(customerId, true));
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDTO();
            dto.AddressDetails = "New Address";
            dto.IsDefault = false;

            var result = await service.AddAddressAsync(customerId, dto);
            var addressesInDb = await context.Addresses.Where(a => a.CustomerId == customerId).ToListAsync();

            Assert.True(result);
            Assert.Equal(2, addressesInDb.Count);
            Assert.False(addressesInDb.First(a => a.AddressDetails == "New Address").IsDefault);
        }

        [Fact]
        public async Task UTCID09_AddAddressAsync_HasActive_SetToDefault_ShouldResetOthers()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var oldAddress = CreateValidAddressEntity(customerId, true);
            oldAddress.AddressDetails = "Old";
            context.Addresses.Add(oldAddress);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDTO();
            dto.AddressDetails = "New";
            dto.IsDefault = true;

            var result = await service.AddAddressAsync(customerId, dto);

            var addressesInDb = await context.Addresses.Where(a => a.CustomerId == customerId).ToListAsync();
            var updatedOld = addressesInDb.First(a => a.AddressDetails == "Old");
            var insertedNew = addressesInDb.First(a => a.AddressDetails == "New");

            Assert.True(result);
            Assert.True(insertedNew.IsDefault);
            Assert.False(updatedOld.IsDefault);
        }

        //=====================================================================
        // Function: UpdateAddressAsync() (10 UTCs)
        //=====================================================================

        [Fact]
        public void UTCID01_UpdateAddressAsync_ProvinceEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID02_UpdateAddressAsync_ProvinceTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.Province = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("Province"));
        }

        [Fact]
        public void UTCID03_UpdateAddressAsync_DistrictEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID04_UpdateAddressAsync_DistrictTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.District = new string('A', 101);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("District"));
        }

        [Fact]
        public void UTCID05_UpdateAddressAsync_AddressDetailsEmpty_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = "";
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public void UTCID06_UpdateAddressAsync_AddressDetailsTooLong_ShouldFailValidation()
        {
            var dto = CreateValidDTO(); dto.AddressDetails = new string('A', 301);
            Assert.Contains(Validate(dto), x => x.MemberNames.Contains("AddressDetails"));
        }

        [Fact]
        public async Task UTCID07_UpdateAddressAsync_AddressNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.UpdateAddressAsync(Guid.NewGuid(), Guid.NewGuid(), CreateValidDTO());
            Assert.False(result);
        }

        [Fact]
        public async Task UTCID08_UpdateAddressAsync_NormalUpdate_ShouldSuccess()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var existingAddress = CreateValidAddressEntity(customerId, false);
            context.Addresses.Add(existingAddress);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDTO();
            dto.AddressDetails = "Updated Details";
            dto.IsDefault = false;

            var result = await service.UpdateAddressAsync(customerId, existingAddress.AddressId, dto);
            var addressInDb = await context.Addresses.FindAsync(existingAddress.AddressId);

            Assert.True(result);
            Assert.Equal("Updated Details", addressInDb!.AddressDetails);
        }

        [Fact]
        public async Task UTCID09_UpdateAddressAsync_SetToDefault_ShouldResetOthers()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var addr1 = CreateValidAddressEntity(customerId, true);
            var addr2 = CreateValidAddressEntity(customerId, false);

            context.Addresses.AddRange(addr1, addr2);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = CreateValidDTO();
            dto.AddressDetails = "Addr 2 Updated";
            dto.IsDefault = true;

            var result = await service.UpdateAddressAsync(customerId, addr2.AddressId, dto);

            var dbAddr1 = await context.Addresses.FindAsync(addr1.AddressId);
            var dbAddr2 = await context.Addresses.FindAsync(addr2.AddressId);

            Assert.True(result);
            Assert.True(dbAddr2!.IsDefault);
            Assert.False(dbAddr1!.IsDefault);
        }

        [Fact]
        public async Task UTCID10_UpdateAddressAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IAddressRepository>();
            repoMock.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

            var service = new AddressService(repoMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateAddressAsync(Guid.NewGuid(), Guid.NewGuid(), CreateValidDTO()));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: DeleteAddressAsync() (5 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_DeleteAddressAsync_EmptyGuid_ShouldReturnFalse()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.DeleteAddressAsync(Guid.NewGuid(), Guid.Empty);
            Assert.False(result);
        }

        [Fact]
        public async Task UTCID02_DeleteAddressAsync_NormalAddress_ShouldSoftDelete()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var address = CreateValidAddressEntity(customerId, false);
            context.Addresses.Add(address);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.DeleteAddressAsync(customerId, address.AddressId);

            var addressInDb = await context.Addresses.FindAsync(address.AddressId);

            Assert.True(result);
            Assert.False(addressInDb!.IsActive);
        }

        [Fact]
        public async Task UTCID03_DeleteAddressAsync_DefaultAddress_ShouldBlockDeletion()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var address = CreateValidAddressEntity(customerId, true);
            context.Addresses.Add(address);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.DeleteAddressAsync(customerId, address.AddressId);

            var addressInDb = await context.Addresses.FindAsync(address.AddressId);

            Assert.False(result);
            Assert.True(addressInDb!.IsActive);
        }

        [Fact]
        public async Task UTCID04_DeleteAddressAsync_AddressNotFound_ShouldReturnFalse()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.DeleteAddressAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task UTCID05_DeleteAddressAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IAddressRepository>();
            repoMock.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

            var service = new AddressService(repoMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAddressAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal("DB Error", ex.Message);
        }

        //=====================================================================
        // Function: GetCustomerAddressesAsync() (3 UTCs)
        //=====================================================================

        [Fact]
        public async Task UTCID01_GetCustomerAddressesAsync_HasAddresses_ShouldReturnList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var addr1 = CreateValidAddressEntity(customerId, true);
            var addr2 = CreateValidAddressEntity(customerId, false);
            var addr3 = CreateValidAddressEntity(Guid.NewGuid(), true);

            context.Addresses.AddRange(addr1, addr2, addr3);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetCustomerAddressesAsync(customerId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UTCID02_GetCustomerAddressesAsync_NoAddresses_ShouldReturnEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var result = await service.GetCustomerAddressesAsync(Guid.NewGuid());

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetCustomerAddressesAsync_RepositoryThrowsException()
        {
            var repoMock = new Mock<IAddressRepository>();
            repoMock.Setup(x => x.GetAddressesByCustomerIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("DB Error"));

            var service = new AddressService(repoMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetCustomerAddressesAsync(Guid.NewGuid()));
            Assert.Equal("DB Error", ex.Message);
        }
    }
}