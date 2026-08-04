using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;
using static PetCenterAPI.DTOs.Requests.PrescriptionItem.PrescriptionItemRequestDTO;

namespace PetCenterTestProject.PrescriptionItemTest
{
    [Collection("DatabaseTests")]
    public class PrescriptionItemTest_DB
    {
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=.;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "TrustServerCertificate=True;",
                    builder => builder.EnableRetryOnFailure())
                .Options;

            return new PetCenterContext(options);
        }

        private PrescriptionItemRepository CreateRepository(PetCenterContext context)
        {
            return new PrescriptionItemRepository(context);
        }

        private PrescriptionItemService CreateService(PetCenterContext context)
        {
            return new PrescriptionItemService(CreateRepository(context));
        }

                                                private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.ChangeTracker.Clear();

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [StaffRoles];");

            context.PrescriptionItems.RemoveRange(context.PrescriptionItems);
            context.MedicalRecords.RemoveRange(context.MedicalRecords);
            context.AppointmentSnapshots.RemoveRange(context.AppointmentSnapshots);
            context.AppointmentServices.RemoveRange(context.AppointmentServices);
            context.Appointments.RemoveRange(context.Appointments);
            context.Pets.RemoveRange(context.Pets);
            context.Diseases.RemoveRange(context.Diseases);
            context.VetFeedbacks.RemoveRange(context.VetFeedbacks);
            context.VetProfiles.RemoveRange(context.VetProfiles);
            context.ScheduleExceptions.RemoveRange(context.ScheduleExceptions);
            context.GlobalWorkSchedules.RemoveRange(context.GlobalWorkSchedules);
            context.CartDetails.RemoveRange(context.CartDetails);
            context.Carts.RemoveRange(context.Carts);
            context.OtpCodes.RemoveRange(context.OtpCodes);
            context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
            context.Vouchers.RemoveRange(context.Vouchers);
            context.FeedbackImages.RemoveRange(context.FeedbackImages);
            context.ProductFeedbacks.RemoveRange(context.ProductFeedbacks);
            context.OrderProductSnapshots.RemoveRange(context.OrderProductSnapshots);
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Payments.RemoveRange(context.Payments);
            context.Orders.RemoveRange(context.Orders);
            context.Addresses.RemoveRange(context.Addresses);
            context.Customers.RemoveRange(context.Customers);
            context.InventoryTransactions.RemoveRange(context.InventoryTransactions);
            context.ImportProductSnapshots.RemoveRange(context.ImportProductSnapshots);
            context.ImportStockDetails.RemoveRange(context.ImportStockDetails);
            context.ImportStocks.RemoveRange(context.ImportStocks);
            context.Suppliers.RemoveRange(context.Suppliers);
            context.Staffs.RemoveRange(context.Staffs);
            context.Inventories.RemoveRange(context.Inventories);
            context.ProductImages.RemoveRange(context.ProductImages);
            context.ProductAttributes.RemoveRange(context.ProductAttributes);
            context.Products.RemoveRange(context.Products);
            context.CategoryAttributes.RemoveRange(context.CategoryAttributes);
            context.Categories.RemoveRange(context.Categories);
            context.Brands.RemoveRange(context.Brands);
            context.ServiceImages.RemoveRange(context.ServiceImages);
            context.Services.RemoveRange(context.Services);

            await context.SaveChangesAsync();
        }

        private async Task EnsureCustomerExistsAsync(PetCenterContext context, Guid customerId)
        {
            if (!await context.Customers.AnyAsync(c => c.CustomerId == customerId))
            {
                context.Customers.Add(new Customer
                {
                    CustomerId = customerId,
                    FullName = "Test Customer",
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsurePetExistsAsync(PetCenterContext context, Guid petId, Guid customerId)
        {
            await EnsureCustomerExistsAsync(context, customerId);
            if (!await context.Pets.AnyAsync(p => p.PetId == petId))
            {
                context.Pets.Add(new Pet
                {
                    PetId = petId,
                    CustomerId = customerId,
                    PetName = "Test Pet",
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureMedicalRecordExistsAsync(PetCenterContext context, Guid recordId, int status = (int)MedicalRecordStatus.Drafted)
        {
            var customerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var petId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            await EnsurePetExistsAsync(context, petId, customerId);

            if (!await context.MedicalRecords.AnyAsync(m => m.RecordId == recordId))
            {
                context.MedicalRecords.Add(new MedicalRecord
                {
                    RecordId = recordId,
                    PetId = petId,
                    DiseaseNameSnapshot = "Flu",
                    Diagnosis = "Flu Test",
                    Treatment = "Pills",
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        // =========================================================================
        // SECTION 1: GetByRecordIdAsync() Integration Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByRecordIdAsync_HasItems_ReturnsList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId);

            var item = new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                RecordId = recordId,
                MedicineName = "Paracetamol",
                Quantity = 10,
                Dosage = "1 pill",
                Duration = "3 days",
                Note = "After meal"
            };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetByRecordIdAsync(recordId);

            Assert.Single(result);
            var resultItem = result.First();
            Assert.Equal("Paracetamol", resultItem.MedicineName);
            Assert.Equal(10, resultItem.Quantity);
        }

        [Fact]
        public async Task UTCID02_GetByRecordIdAsync_NoItems_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId);

            var service = CreateService(context);
            var result = await service.GetByRecordIdAsync(recordId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetByRecordIdAsync_ThrowsException()
        {
            var recordId = Guid.NewGuid();
            var repoMock = new Mock<IPrescriptionItemRepository>();
            repoMock.Setup(repo => repo.GetByRecordIdAsync(recordId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new PrescriptionItemService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetByRecordIdAsync(recordId));
        }

        // =========================================================================
        // SECTION 2: GetByIdAsync() Integration Tests (UTCID01 - UTCID03)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByIdAsync_Exists_ReturnsDTO()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                PrescriptionItemId = itemId,
                RecordId = recordId,
                MedicineName = "Amoxicillin",
                Quantity = 5,
                Dosage = "1 pill",
                Duration = "5 days"
            };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Equal(itemId, result.PrescriptionItemId);
            Assert.Equal("Amoxicillin", result.MedicineName);
        }

        [Fact]
        public async Task UTCID02_GetByIdAsync_NotFound_ReturnsNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID03_GetByIdAsync_ThrowsException()
        {
            var itemId = Guid.NewGuid();
            var repoMock = new Mock<IPrescriptionItemRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ThrowsAsync(new Exception("Database connection failed"));

            var service = new PrescriptionItemService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetByIdAsync(itemId));
        }

        // =========================================================================
        // SECTION 3: CreateAsync() Integration Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateAsync_StatusDrafted_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Drafted);

            var dto = new CreatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", RecordId = recordId, MedicineName = "Ibuprofen", Quantity = 2 };

            var service = CreateService(context);
            await service.CreateAsync(dto);

            var createdItem = await context.PrescriptionItems.FirstOrDefaultAsync(p => p.RecordId == recordId);
            Assert.NotNull(createdItem);
            Assert.Equal("Ibuprofen", createdItem.MedicineName);
            Assert.Equal(2, createdItem.Quantity);
        }

        [Fact]
        public async Task UTCID02_CreateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Completed);

            var dto = new CreatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", RecordId = recordId, MedicineName = "Ibuprofen", Quantity = 2 };

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID03_CreateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Cancelled);

            var dto = new CreatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", RecordId = recordId, MedicineName = "Ibuprofen", Quantity = 2 };

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_CreateAsync_RecordNotFound_ThrowsException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", RecordId = recordId, MedicineName = "Ibuprofen", Quantity = 2 };

            var service = CreateService(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task UTCID05_CreateAsync_ThrowsException()
        {
            var recordId = Guid.NewGuid();
            var dto = new CreatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", RecordId = recordId };
            var repoMock = new Mock<IPrescriptionItemRepository>();
            repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            repoMock.Setup(repo => repo.AddAsync(It.IsAny<PrescriptionItem>())).ThrowsAsync(new Exception("Database save failed"));

            var service = new PrescriptionItemService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(dto));
        }

        // =========================================================================
        // SECTION 4: UpdateAsync() Integration Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateAsync_StatusDrafted_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Drafted);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var dto = new UpdatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", MedicineName = "New Med", Quantity = 2 };

            var service = CreateService(context);
            await service.UpdateAsync(itemId, dto);

            var updatedItem = await context.PrescriptionItems.FindAsync(itemId);
            Assert.NotNull(updatedItem);
            Assert.Equal("New Med", updatedItem.MedicineName);
            Assert.Equal(2, updatedItem.Quantity);
        }

        [Fact]
        public async Task UTCID02_UpdateAsync_NotFound_ThrowsException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var dto = new UpdatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", MedicineName = "New Med", Quantity = 2 };

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(Guid.NewGuid(), dto));
            Assert.Equal("Prescription item not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_UpdateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Completed);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var dto = new UpdatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", MedicineName = "New Med", Quantity = 2 };

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(itemId, dto));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_UpdateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Cancelled);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var dto = new UpdatePrescriptionItemDTO
            {
                Duration = "5 days",
                Dosage = "1 tablet", MedicineName = "New Med", Quantity = 2 };

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(itemId, dto));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID05_UpdateAsync_ThrowsException()
        {
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId };
            var dto = new UpdatePrescriptionItemDTO();

            var repoMock = new Mock<IPrescriptionItemRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            repoMock.Setup(repo => repo.UpdateAsync(item)).ThrowsAsync(new Exception("Database save failed"));

            var service = new PrescriptionItemService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(itemId, dto));
        }

        // =========================================================================
        // SECTION 5: DeleteAsync() Integration Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_DeleteAsync_StatusDrafted_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Drafted);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            await service.DeleteAsync(itemId);

            var deletedItem = await context.PrescriptionItems.AsNoTracking().FirstOrDefaultAsync(p => p.PrescriptionItemId == itemId);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task UTCID02_DeleteAsync_NotFound_ThrowsException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(Guid.NewGuid()));
            Assert.Equal("Prescription item not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_DeleteAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Completed);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(itemId));
            Assert.Equal("Cannot modify prescription items of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID04_DeleteAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            await EnsureMedicalRecordExistsAsync(context, recordId, (int)MedicalRecordStatus.Cancelled);

            var itemId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId, MedicineName = "Old Med", Quantity = 1 };
            context.PrescriptionItems.Add(item);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(itemId));
            Assert.Equal("Cannot modify prescription items of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID05_DeleteAsync_ThrowsException()
        {
            var itemId = Guid.NewGuid();
            var recordId = Guid.NewGuid();
            var item = new PrescriptionItem
            {
                Duration = "5 days",
                Dosage = "1 tablet", PrescriptionItemId = itemId, RecordId = recordId };

            var repoMock = new Mock<IPrescriptionItemRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
            repoMock.Setup(repo => repo.GetRecordStatusAsync(recordId)).ReturnsAsync((int)MedicalRecordStatus.Drafted);
            repoMock.Setup(repo => repo.DeleteAsync(itemId)).ThrowsAsync(new Exception("Database save failed"));

            var service = new PrescriptionItemService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(itemId));
        }
    }
}
