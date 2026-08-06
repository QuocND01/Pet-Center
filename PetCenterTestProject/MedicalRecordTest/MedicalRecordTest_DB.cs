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
using PetCenterTestProject;
using Xunit;
using static PetCenterAPI.DTOs.Requests.MedicalRecord.MedicalRecordRequestDTO;
using static PetCenterAPI.DTOs.Responses.MedicalRecord.MedicalRecordResponseDTO;

namespace PetCenterTestProject.MedicalRecordTest
{
    [Collection("DatabaseTests")]
    public class MedicalRecordTest_DB
    {
        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(
                    "Server=127.0.0.1,1433;" +
                    "Database=PetCenter_Test;" +
                    "User Id=sa;" +
                    "Password=123456;" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;",
                    builder => builder.EnableRetryOnFailure())
                .Options;

            return new PetCenterContext(options);
        }

        private MedicalRecordRepository CreateRepository(PetCenterContext context)
        {
            return new MedicalRecordRepository(context);
        }

        private MedicalRecordService CreateService(PetCenterContext context)
        {
            return new MedicalRecordService(CreateRepository(context));
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            await TestDatabaseCleaner.ClearAllAsync(context);
        }

        private async Task EnsureStaffExistsAsync(PetCenterContext context, Guid staffId, string fullName = "Vet Dr. A")
        {
            if (!await context.Staffs.AnyAsync(s => s.StaffId == staffId))
            {
                context.Staffs.Add(new Staff
                {
                    StaffId = staffId,
                    FullName = fullName,
                    PhoneNumber = "0123456789",
                    Email = $"{staffId}@petcenter.com",
                    PasswordHash = "hashedpassword",
                    Gender = "Male",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    HireDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureCustomerExistsAsync(PetCenterContext context, Guid customerId, string fullName = "Customer A")
        {
            if (!await context.Customers.AnyAsync(c => c.CustomerId == customerId))
            {
                context.Customers.Add(new Customer
                {
                    CustomerId = customerId,
                    FullName = fullName,
                    Email = $"{customerId}@customer.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsurePetExistsAsync(PetCenterContext context, Guid petId, Guid customerId, string species = "Dog", string breed = "Golden Retriever")
        {
            await EnsureCustomerExistsAsync(context, customerId);
            if (!await context.Pets.AnyAsync(p => p.PetId == petId))
            {
                context.Pets.Add(new Pet
                {
                    PetId = petId,
                    CustomerId = customerId,
                    PetName = "Fido",
                    Species = species,
                    Breed = breed,
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureDiseaseExistsAsync(PetCenterContext context, Guid diseaseId, string name = "Flu")
        {
            if (!await context.Diseases.AnyAsync(d => d.DiseaseId == diseaseId))
            {
                context.Diseases.Add(new Disease
                {
                    DiseaseId = diseaseId,
                    Name = name,
                    Description = "Flu description",
                    Recommendation = "Rest",
                    Species = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureAppointmentExistsAsync(PetCenterContext context, Guid appointmentId, Guid customerId, Guid petId, Guid staffId, int status = 4, string species = "Dog", string breed = "Golden Retriever", string vetName = "Vet Dr. A")
        {
            await EnsureCustomerExistsAsync(context, customerId);
            await EnsurePetExistsAsync(context, petId, customerId, species, breed);
            await EnsureStaffExistsAsync(context, staffId, vetName);

            if (!await context.Appointments.AnyAsync(a => a.AppointmentId == appointmentId))
            {
                var appointment = new Appointment
                {
                    AppointmentId = appointmentId,
                    CustomerId = customerId,
                    PetId = petId,
                    StaffId = staffId,
                    AppointmentStart = DateTime.UtcNow.AddHours(-1),
                    AppointmentEnd = DateTime.UtcNow,
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                };
                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();

                context.AppointmentSnapshots.Add(new AppointmentSnapshot
                {
                    AppointmentSnapshotId = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    Species = species,
                    Breed = breed,
                    Gender = "Male",
                    Weight = 10,
                    Rating = 5,
                    VetName = vetName
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task<MedicalRecord> CreateAndSaveTestRecordAsync(
            PetCenterContext context, Guid id, Guid appointmentId, Guid customerId, Guid petId, Guid staffId, int status)
        {
            await EnsureAppointmentExistsAsync(context, appointmentId, customerId, petId, staffId, 4, "Dog", "Golden Retriever", "Vet Dr. A");
            
            var record = new MedicalRecord
            {
                RecordId = id,
                AppointmentId = appointmentId,
                PetId = petId,
                DiseaseNameSnapshot = "Flu",
                Diagnosis = "General Flu",
                Treatment = "Rest",
                Note = "No food restriction",
                CreatedAt = DateTime.UtcNow,
                Status = status
            };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();
            return record;
        }

        // =========================================================================
        // SECTION 1: GetAllAsync() Integration Tests (UTCID01 - UTCID08)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetAllAsync_SearchNullStatusNull_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync(null, null, 1, 10);

            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal(recordId, item.RecordId);
            Assert.Equal("Drafted", item.StatusName);
            Assert.Equal("Dog", item.PetSpecies);
            Assert.Equal("Golden Retriever", item.PetBreed);
            Assert.Equal("Vet Dr. A", item.VetName);
            Assert.Equal("Customer A", item.CustomerName);
        }

        [Fact]
        public async Task UTCID02_GetAllAsync_SearchMatches_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 2);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync("Flu", null, 1, 10);

            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Completed", items.First().StatusName);
        }

        [Fact]
        public async Task UTCID03_GetAllAsync_SearchNoMatch_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync("Nonexistent", null, 1, 10);

            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task UTCID04_GetAllAsync_StatusDrafted_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync(null, 1, 1, 10);

            Assert.Single(items);
            Assert.Equal(1, items.First().Status);
        }

        [Fact]
        public async Task UTCID05_GetAllAsync_StatusCompleted_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 2);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync(null, 2, 1, 10);

            Assert.Single(items);
            Assert.Equal(2, items.First().Status);
        }

        [Fact]
        public async Task UTCID06_GetAllAsync_StatusCancelled_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 3);

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync(null, 3, 1, 10);

            Assert.Single(items);
            Assert.Equal(3, items.First().Status);
        }

        [Fact]
        public async Task UTCID07_GetAllAsync_Pagination_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            for (int i = 0; i < 6; i++)
            {
                await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);
            }

            var service = CreateService(context);
            var (items, total) = await service.GetAllAsync(null, null, 2, 5);

            Assert.Single(items);
            Assert.Equal(6, total);
        }

        [Fact]
        public async Task UTCID08_GetAllAsync_ThrowsException()
        {
            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetAllAsync(null, null, 1, 10));
        }

        // =========================================================================
        // SECTION 2: GetByCustomerIdAsync() Integration Tests (UTCID01 - UTCID06)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByCustomerIdAsync_HasCompletedRecords_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), customerId, petId, staffId, 2);

            var service = CreateService(context);
            var result = await service.GetByCustomerIdAsync(customerId, null);

            Assert.Single(result);
            Assert.Equal(customerId, result.First().CustomerId);
        }

        [Fact]
        public async Task UTCID02_GetByCustomerIdAsync_HasOnlyDraftedOrCancelled_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), customerId, petId, staffId, 1); // Drafted
            await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), customerId, petId, staffId, 3); // Cancelled

            var service = CreateService(context);
            var result = await service.GetByCustomerIdAsync(customerId, null);

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetByCustomerIdAsync_NoRecords_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();

            var service = CreateService(context);
            var result = await service.GetByCustomerIdAsync(customerId, null);

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID04_GetByCustomerIdAsync_SearchMatches_ReturnsItems()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), customerId, petId, staffId, 2);

            var service = CreateService(context);
            var result = await service.GetByCustomerIdAsync(customerId, "Flu");

            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID05_GetByCustomerIdAsync_SearchNoMatch_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var customerId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, Guid.NewGuid(), Guid.NewGuid(), customerId, petId, staffId, 2);

            var service = CreateService(context);
            var result = await service.GetByCustomerIdAsync(customerId, "Nonexistent");

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID06_GetByCustomerIdAsync_ThrowsException()
        {
            var customerId = Guid.NewGuid();
            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetByCustomerIdAsync(customerId, "Flu"));
        }

        // =========================================================================
        // SECTION 3: GetByIdAsync() Integration Tests (UTCID01 - UTCID07)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByIdAsync_CompleteDetails_ReturnsMappedDTO()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                RecordId = recordId,
                MedicineName = "Medicine A",
                Dosage = "1 pill",
                Duration = "5 days",
                Quantity = 10
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetByIdAsync(recordId);

            Assert.NotNull(result);
            Assert.Equal(recordId, result.RecordId);
            Assert.Equal("Dog", result.PetSpecies);
            Assert.Equal("Vet Dr. A", result.VetName);
            Assert.Equal("Customer A", result.CustomerName);
            Assert.Single(result.PrescriptionItems);
            Assert.Equal("Medicine A", result.PrescriptionItems.First().MedicineName);
        }

        [Fact]
        public async Task UTCID02_GetByIdAsync_NullAppointment_ReturnsFallbackValues()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var custId = Guid.NewGuid();

            await EnsurePetExistsAsync(context, petId, custId);

            var record = new MedicalRecord
            {
                RecordId = recordId,
                PetId = petId,
                AppointmentId = null,
                DiseaseNameSnapshot = "Flu",
                Diagnosis = "General Flu",
                Treatment = "Rest",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetByIdAsync(recordId);

            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, result.CustomerId);
            Assert.Equal("-", result.CustomerName);
            Assert.Equal("-", result.PetSpecies);
            Assert.Equal("-", result.VetName);
            Assert.Empty(result.PrescriptionItems);
        }

        [Fact]
        public async Task UTCID03_GetByIdAsync_NullSnapshotWithStaff_ReturnsVetNameFromStaff()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            // Set snapshot to null and update staff name
            var snapshot = await context.AppointmentSnapshots.FirstOrDefaultAsync(s => s.AppointmentId == apptId);
            if (snapshot != null) context.AppointmentSnapshots.Remove(snapshot);

            var staff = await context.Staffs.FindAsync(staffId);
            if (staff != null) staff.FullName = "Dr. Staff Vet";
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetByIdAsync(recordId);

            Assert.NotNull(result);
            Assert.Equal("Dr. Staff Vet", result.VetName);
        }

        [Fact]
        public async Task UTCID04_GetByIdAsync_NullSnapshotNullStaff_ReturnsVetNameFallback()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            // Set snapshot and staff to null
            var snapshot = await context.AppointmentSnapshots.FirstOrDefaultAsync(s => s.AppointmentId == apptId);
            if (snapshot != null) context.AppointmentSnapshots.Remove(snapshot);

            // We must set StaffId in appointment to a different staff or bypass FK. But since StaffId is required,
            // we will simulate the null staff at repository level using mock in the mock test.
            // In DB, if StaffId is required, it can never be null in DB records due to foreign keys. 
            // So we mock this specific test case or use a mock repo to test the mapping logic.
            var repoMock = new Mock<IMedicalRecordRepository>();
            var record = new MedicalRecord
            {
                RecordId = recordId,
                PetId = petId,
                AppointmentId = apptId,
                Appointment = new Appointment
                {
                    AppointmentId = apptId,
                    CustomerId = custId,
                    Customer = new Customer { FullName = "Customer A" },
                    AppointmentSnapshot = null,
                    Staff = null
                }
            };
            repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            var service = new MedicalRecordService(repoMock.Object);
            var result = await service.GetByIdAsync(recordId);

            Assert.NotNull(result);
            Assert.Equal("-", result.VetName);
        }

        [Fact]
        public async Task UTCID05_GetByIdAsync_EmptyPrescriptionItems_ReturnsEmptyList()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            var result = await service.GetByIdAsync(recordId);

            Assert.NotNull(result);
            Assert.Empty(result.PrescriptionItems);
        }

        [Fact]
        public async Task UTCID06_GetByIdAsync_NotFound_ReturnsNull()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID07_GetByIdAsync_ThrowsException()
        {
            var recordId = Guid.NewGuid();
            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ThrowsAsync(new Exception("Database save failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetByIdAsync(recordId));
        }

        // =========================================================================
        // SECTION 4: GetCompletedAppointmentsAsync() Integration Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetCompletedAppointmentsAsync_HasSnapshot_ReturnsMappedDetails()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await EnsureAppointmentExistsAsync(context, apptId, custId, petId, staffId, 3, "Dog", "Husky", "Vet 1");

            var customer = await context.Customers.FindAsync(custId);
            customer.FullName = "Customer 1";
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetCompletedAppointmentsAsync();

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Dog", item.PetSpecies);
            Assert.Equal("Husky", item.PetBreed);
            Assert.Equal("Vet 1", item.VetName);
            Assert.Equal("Customer 1", item.CustomerName);
        }

        [Fact]
        public async Task UTCID02_GetCompletedAppointmentsAsync_NullSnapshotHasPet_ReturnsPetFallback()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await EnsureAppointmentExistsAsync(context, apptId, custId, petId, staffId, 3, "Cat", "Maine Coon", "Vet 1");

            // Remove snapshot
            var snapshot = await context.AppointmentSnapshots.FirstOrDefaultAsync(s => s.AppointmentId == apptId);
            if (snapshot != null) context.AppointmentSnapshots.Remove(snapshot);

            var customer = await context.Customers.FindAsync(custId);
            customer.FullName = "Customer 1";
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetCompletedAppointmentsAsync();

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Cat", item.PetSpecies);
            Assert.Equal("Maine Coon", item.PetBreed);
        }

        [Fact]
        public async Task UTCID03_GetCompletedAppointmentsAsync_NullSnapshotNullPet_ReturnsFallback()
        {
            // Null fields that are non-nullable in database can be mocked or bypassed.
            // In DB, Appointment requires Pet and Customer. So we use mock repo to test mapping fallback logic.
            var repoMock = new Mock<IMedicalRecordRepository>();
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    AppointmentStart = DateTime.UtcNow,
                    Customer = null!,
                    AppointmentSnapshot = null,
                    Pet = null!
                }
            };
            repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ReturnsAsync(appointments);

            var service = new MedicalRecordService(repoMock.Object);
            var result = await service.GetCompletedAppointmentsAsync();

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("-", item.PetSpecies);
            Assert.Equal("-", item.PetBreed);
            Assert.Equal("-", item.CustomerName);
            Assert.Equal("-", item.VetName);
        }

        [Fact]
        public async Task UTCID04_GetCompletedAppointmentsAsync_NoAppointments_ReturnsEmpty()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var result = await service.GetCompletedAppointmentsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID05_GetCompletedAppointmentsAsync_ThrowsException()
        {
            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ThrowsAsync(new Exception("Database connection failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.GetCompletedAppointmentsAsync());
        }

        // =========================================================================
        // SECTION 5: CreateAsync() Integration Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateAsync_DiseaseIdValid_CreatesWithDiseaseName()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();

            await EnsureAppointmentExistsAsync(context, apptId, custId, petId, staffId, 4);
            await EnsureDiseaseExistsAsync(context, diseaseId, "Parvovirus");

            var dto = new CreateMedicalRecordDTO 
            { 
                AppointmentId = apptId, 
                DiseaseId = diseaseId,
                Diagnosis = "Initial Diagnosis",
                Treatment = "Initial Treatment"
            };

            var service = CreateService(context);
            await service.CreateAsync(dto);

            var created = await context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == apptId);
            Assert.NotNull(created);
            Assert.Equal(diseaseId, created.DiseaseId);
            Assert.Equal("Parvovirus", created.DiseaseNameSnapshot);
            Assert.Equal(1, created.Status); // Drafted
        }

        [Fact]
        public async Task UTCID02_CreateAsync_DiseaseIdNotFound_CreatesWithUnknownDisease()
        {
            var apptId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();

            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync((Disease?)null);

            var service = new MedicalRecordService(repoMock.Object);
            var dto = new CreateMedicalRecordDTO
            {
                AppointmentId = apptId,
                DiseaseId = diseaseId,
                Diagnosis = "Initial Diagnosis",
                Treatment = "Initial Treatment"
            };

            await service.CreateAsync(dto);

            repoMock.Verify(repo => repo.AddAsync(It.Is<MedicalRecord>(r =>
                r.DiseaseId == diseaseId &&
                r.DiseaseNameSnapshot == "Unknown Disease"
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID03_CreateAsync_DiseaseIdNullCustomProvided_CreatesWithCustomName()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await EnsureAppointmentExistsAsync(context, apptId, custId, petId, staffId, 4);

            var dto = new CreateMedicalRecordDTO 
            { 
                AppointmentId = apptId, 
                DiseaseId = null, 
                CustomDiseaseName = "Ear Infection",
                Diagnosis = "Initial Diagnosis",
                Treatment = "Initial Treatment"
            };

            var service = CreateService(context);
            await service.CreateAsync(dto);

            var created = await context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == apptId);
            Assert.NotNull(created);
            Assert.Equal("Ear Infection", created.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID04_CreateAsync_DiseaseIdNullCustomNull_CreatesWithDefault()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await EnsureAppointmentExistsAsync(context, apptId, custId, petId, staffId, 4);

            var dto = new CreateMedicalRecordDTO 
            { 
                AppointmentId = apptId, 
                DiseaseId = null, 
                CustomDiseaseName = null,
                Diagnosis = "Initial Diagnosis",
                Treatment = "Initial Treatment"
            };

            var service = CreateService(context);
            await service.CreateAsync(dto);

            var created = await context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == apptId);
            Assert.NotNull(created);
            Assert.Equal("General Checkup", created.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID05_CreateAsync_ThrowsException()
        {
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid() };
            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.AddAsync(It.IsAny<MedicalRecord>())).ThrowsAsync(new Exception("Database save failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(dto));
        }

        // =========================================================================
        // SECTION 6: UpdateAsync() Integration Tests (UTCID01 - UTCID08)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateAsync_StatusDraftedDiseaseExists_UpdatesWithDiseaseName()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1); // Status Drafted
            await EnsureDiseaseExistsAsync(context, diseaseId, "Allergy");

            var dto = new UpdateMedicalRecordDTO
            {
                DiseaseId = diseaseId,
                Diagnosis = "Updated Diag",
                Treatment = "Updated Treat",
                Note = "Updated Note"
            };

            var service = CreateService(context);
            await service.UpdateAsync(recordId, dto);

            var updated = await context.MedicalRecords.FindAsync(recordId);
            Assert.NotNull(updated);
            Assert.Equal(diseaseId, updated.DiseaseId);
            Assert.Equal("Allergy", updated.DiseaseNameSnapshot);
            Assert.Equal("Updated Diag", updated.Diagnosis);
            Assert.Equal("Updated Treat", updated.Treatment);
            Assert.Equal("Updated Note", updated.Note);
        }

        [Fact]
        public async Task UTCID02_UpdateAsync_StatusDraftedDiseaseNotFound_UpdatesWithUnknownDisease()
        {
            var recordId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 }; // Drafted
            var dto = new UpdateMedicalRecordDTO 
            { 
                DiseaseId = diseaseId,
                Diagnosis = "Updated Diag",
                Treatment = "Updated Treat"
            };

            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync((Disease?)null);

            var service = new MedicalRecordService(repoMock.Object);
            await service.UpdateAsync(recordId, dto);

            Assert.Equal("Unknown Disease", record.DiseaseNameSnapshot);
            repoMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        }

        [Fact]
        public async Task UTCID03_UpdateAsync_StatusDraftedCustomProvided_UpdatesWithCustomName()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var dto = new UpdateMedicalRecordDTO 
            { 
                DiseaseId = null, 
                CustomDiseaseName = "Eye Infection",
                Diagnosis = "Updated Diag",
                Treatment = "Updated Treat"
            };

            var service = CreateService(context);
            await service.UpdateAsync(recordId, dto);

            var updated = await context.MedicalRecords.FindAsync(recordId);
            Assert.NotNull(updated);
            Assert.Equal("Eye Infection", updated.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID04_UpdateAsync_StatusDraftedCustomNull_UpdatesWithDefault()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var dto = new UpdateMedicalRecordDTO 
            { 
                DiseaseId = null, 
                CustomDiseaseName = null,
                Diagnosis = "Updated Diag",
                Treatment = "Updated Treat"
            };

            var service = CreateService(context);
            await service.UpdateAsync(recordId, dto);

            var updated = await context.MedicalRecords.FindAsync(recordId);
            Assert.NotNull(updated);
            Assert.Equal("General Checkup", updated.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID05_UpdateAsync_RecordNotFound_ThrowsException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var dto = new UpdateMedicalRecordDTO();

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(Guid.NewGuid(), dto));
            Assert.Equal("Medical record not found", ex.Message);
        }

        [Fact]
        public async Task UTCID06_UpdateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 2); // Completed

            var dto = new UpdateMedicalRecordDTO();

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(recordId, dto));
            Assert.Equal("Cannot update a medical record that is not in Drafted status", ex.Message);
        }

        [Fact]
        public async Task UTCID07_UpdateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 3); // Cancelled

            var dto = new UpdateMedicalRecordDTO();

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(recordId, dto));
            Assert.Equal("Cannot update a medical record that is not in Drafted status", ex.Message);
        }

        [Fact]
        public async Task UTCID08_UpdateAsync_ThrowsException()
        {
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };
            var dto = new UpdateMedicalRecordDTO();

            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            repoMock.Setup(repo => repo.UpdateAsync(record)).ThrowsAsync(new Exception("Database save failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(recordId, dto));
        }

        // =========================================================================
        // SECTION 7: ChangeStatusAsync() Integration Tests (UTCID01 - UTCID07)
        // =========================================================================

        [Fact]
        public async Task UTCID01_ChangeStatusAsync_DraftedToCompleted_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            await service.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed);

            var updated = await context.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(r => r.RecordId == recordId);
            Assert.NotNull(updated);
            Assert.Equal(2, updated.Status);
        }

        [Fact]
        public async Task UTCID02_ChangeStatusAsync_DraftedToCancelled_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            await service.ChangeStatusAsync(recordId, MedicalRecordStatus.Cancelled);

            var updated = await context.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(r => r.RecordId == recordId);
            Assert.NotNull(updated);
            Assert.Equal(3, updated.Status);
        }

        [Fact]
        public async Task UTCID03_ChangeStatusAsync_DraftedToDrafted_Success()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 1);

            var service = CreateService(context);
            await service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted);

            var updated = await context.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(r => r.RecordId == recordId);
            Assert.NotNull(updated);
            Assert.Equal(1, updated.Status);
        }

        [Fact]
        public async Task UTCID04_ChangeStatusAsync_RecordNotFound_ThrowsException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<Exception>(() => service.ChangeStatusAsync(Guid.NewGuid(), MedicalRecordStatus.Completed));
            Assert.Equal("Medical record not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_ChangeStatusAsync_CompletedToDrafted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 2); // Completed

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted));
            Assert.Equal("Cannot change the status of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID06_ChangeStatusAsync_CancelledToDrafted_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();
            await ClearDatabaseAsync(context);

            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var petId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            await CreateAndSaveTestRecordAsync(context, recordId, apptId, custId, petId, staffId, 3); // Cancelled

            var service = CreateService(context);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted));
            Assert.Equal("Cannot change the status of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID07_ChangeStatusAsync_ThrowsException()
        {
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };

            var repoMock = new Mock<IMedicalRecordRepository>();
            repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            repoMock.Setup(repo => repo.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed)).ThrowsAsync(new Exception("Database save failed"));

            var service = new MedicalRecordService(repoMock.Object);
            await Assert.ThrowsAsync<Exception>(() => service.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed));
        }
    }
}
