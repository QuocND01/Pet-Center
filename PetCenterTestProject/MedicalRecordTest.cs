using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using Xunit;
using static PetCenterAPI.DTOs.Requests.MedicalRecord.MedicalRecordRequestDTO;
using static PetCenterAPI.DTOs.Responses.MedicalRecord.MedicalRecordResponseDTO;

namespace PetCenterTestProject
{
    public class MedicalRecordTest
    {
        private readonly Mock<IMedicalRecordRepository> _repoMock;
        private readonly MedicalRecordService _service;

        public MedicalRecordTest()
        {
            _repoMock = new Mock<IMedicalRecordRepository>();
            _service = new MedicalRecordService(_repoMock.Object);
        }

        // Helper: Create a standard medical record with hierarchy
        private MedicalRecord CreateTestRecord(Guid id, Guid appointmentId, Guid customerId, int status)
        {
            return new MedicalRecord
            {
                RecordId = id,
                AppointmentId = appointmentId,
                DiseaseId = Guid.NewGuid(),
                DiseaseNameSnapshot = "Flu",
                Diagnosis = "General Flu",
                Treatment = "Rest",
                Note = "No food restriction",
                CreatedAt = DateTime.UtcNow,
                Status = status,
                Appointment = new Appointment
                {
                    AppointmentId = appointmentId,
                    CustomerId = customerId,
                    AppointmentStart = DateTime.UtcNow.AddHours(-1),
                    AppointmentEnd = DateTime.UtcNow,
                    Customer = new Customer { CustomerId = customerId, FullName = "Customer A" },
                    Staff = new Staff { StaffId = Guid.NewGuid(), FullName = "Vet Dr. A" },
                    AppointmentSnapshot = new AppointmentSnapshot
                    {
                        AppointmentId = appointmentId,
                        Species = "Dog",
                        Breed = "Golden Retriever",
                        VetName = "Vet Dr. A"
                    }
                },
                PrescriptionItems = new List<PrescriptionItem>()
            };
        }

        // =========================================================================
        // SECTION 1: GetAllAsync() Unit Tests (UTCID01 - UTCID08)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetAllAsync_SearchNullStatusNull_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1)
            };
            _repoMock.Setup(repo => repo.GetAllAsync(null, null, 1, 10))
                .ReturnsAsync((records, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(null, null, 1, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            var item = items.First();
            Assert.Equal(records[0].RecordId, item.RecordId);
            Assert.Equal("Drafted", item.StatusName);
            Assert.Equal("Dog", item.PetSpecies);
            Assert.Equal("Golden Retriever", item.PetBreed);
            Assert.Equal("Vet Dr. A", item.VetName);
            Assert.Equal("Customer A", item.CustomerName);
        }

        [Fact]
        public async Task UTCID02_GetAllAsync_SearchMatches_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2)
            };
            _repoMock.Setup(repo => repo.GetAllAsync("Flu", null, 1, 10))
                .ReturnsAsync((records, 1));

            // Act
            var (items, total) = await _service.GetAllAsync("Flu", null, 1, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Completed", items.First().StatusName);
        }

        [Fact]
        public async Task UTCID03_GetAllAsync_SearchNoMatch_ReturnsEmpty()
        {
            // Arrange
            _repoMock.Setup(repo => repo.GetAllAsync("Nonexistent", null, 1, 10))
                .ReturnsAsync((new List<MedicalRecord>(), 0));

            // Act
            var (items, total) = await _service.GetAllAsync("Nonexistent", null, 1, 10);

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task UTCID04_GetAllAsync_StatusDrafted_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1)
            };
            _repoMock.Setup(repo => repo.GetAllAsync(null, 1, 1, 10))
                .ReturnsAsync((records, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(null, 1, 1, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, items.First().Status);
        }

        [Fact]
        public async Task UTCID05_GetAllAsync_StatusCompleted_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2)
            };
            _repoMock.Setup(repo => repo.GetAllAsync(null, 2, 1, 10))
                .ReturnsAsync((records, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(null, 2, 1, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(2, items.First().Status);
        }

        [Fact]
        public async Task UTCID06_GetAllAsync_StatusCancelled_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3)
            };
            _repoMock.Setup(repo => repo.GetAllAsync(null, 3, 1, 10))
                .ReturnsAsync((records, 1));

            // Act
            var (items, total) = await _service.GetAllAsync(null, 3, 1, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(3, items.First().Status);
        }

        [Fact]
        public async Task UTCID07_GetAllAsync_Pagination_ReturnsItems()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1)
            };
            _repoMock.Setup(repo => repo.GetAllAsync(null, null, 2, 5))
                .ReturnsAsync((records, 10));

            // Act
            var (items, total) = await _service.GetAllAsync(null, null, 2, 5);

            // Assert
            Assert.Single(items);
            Assert.Equal(10, total);
        }

        [Fact]
        public async Task UTCID08_GetAllAsync_ThrowsException()
        {
            // Arrange
            _repoMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync(null, null, 1, 10));
        }

        // =========================================================================
        // SECTION 2: GetByCustomerIdAsync() Unit Tests (UTCID01 - UTCID06)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByCustomerIdAsync_HasCompletedRecords_ReturnsItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), customerId, 2)
            };
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, null))
                .ReturnsAsync(records);

            // Act
            var result = await _service.GetByCustomerIdAsync(customerId, null);

            // Assert
            Assert.Single(result);
            Assert.Equal(customerId, result.First().CustomerId);
        }

        [Fact]
        public async Task UTCID02_GetByCustomerIdAsync_HasOnlyDraftedOrCancelled_ReturnsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            // Since service maps exactly whatever repo returns, the repository should return an empty list
            // to fulfill the condition that no completed records are retrieved.
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, null))
                .ReturnsAsync(new List<MedicalRecord>());

            // Act
            var result = await _service.GetByCustomerIdAsync(customerId, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID03_GetByCustomerIdAsync_NoRecords_ReturnsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, null))
                .ReturnsAsync(new List<MedicalRecord>());

            // Act
            var result = await _service.GetByCustomerIdAsync(customerId, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID04_GetByCustomerIdAsync_SearchMatches_ReturnsItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var records = new List<MedicalRecord>
            {
                CreateTestRecord(Guid.NewGuid(), Guid.NewGuid(), customerId, 2)
            };
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, "Flu"))
                .ReturnsAsync(records);

            // Act
            var result = await _service.GetByCustomerIdAsync(customerId, "Flu");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task UTCID05_GetByCustomerIdAsync_SearchNoMatch_ReturnsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, "Nonexistent"))
                .ReturnsAsync(new List<MedicalRecord>());

            // Act
            var result = await _service.GetByCustomerIdAsync(customerId, "Nonexistent");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID06_GetByCustomerIdAsync_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByCustomerIdAsync(customerId, "Flu"));
        }

        // =========================================================================
        // SECTION 3: GetByIdAsync() Unit Tests (UTCID01 - UTCID07)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetByIdAsync_CompleteDetails_ReturnsMappedDTO()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var apptId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var record = CreateTestRecord(recordId, apptId, custId, 1);
            record.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                RecordId = recordId,
                MedicineName = "Medicine A",
                Quantity = 10
            });

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
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
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord
            {
                RecordId = recordId,
                AppointmentId = Guid.NewGuid(),
                Appointment = null, // Null
                PrescriptionItems = new List<PrescriptionItem>()
            };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
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
            // Arrange
            var recordId = Guid.NewGuid();
            var record = CreateTestRecord(recordId, Guid.NewGuid(), Guid.NewGuid(), 1);
            record.Appointment.AppointmentSnapshot = null; // Null snapshot
            record.Appointment.Staff = new Staff { FullName = "Dr. Staff Vet" };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dr. Staff Vet", result.VetName); // Falling back to staff name
        }

        [Fact]
        public async Task UTCID04_GetByIdAsync_NullSnapshotNullStaff_ReturnsVetNameFallback()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = CreateTestRecord(recordId, Guid.NewGuid(), Guid.NewGuid(), 1);
            record.Appointment.AppointmentSnapshot = null;
            record.Appointment.Staff = null;

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("-", result.VetName); // Both null
        }

        [Fact]
        public async Task UTCID05_GetByIdAsync_EmptyPrescriptionItems_ReturnsEmptyList()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = CreateTestRecord(recordId, Guid.NewGuid(), Guid.NewGuid(), 1);
            record.PrescriptionItems = new List<PrescriptionItem>(); // Empty

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.PrescriptionItems);
        }

        [Fact]
        public async Task UTCID06_GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync((MedicalRecord?)null);

            // Act
            var result = await _service.GetByIdAsync(recordId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UTCID07_GetByIdAsync_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(recordId));
        }

        // =========================================================================
        // SECTION 4: GetCompletedAppointmentsAsync() Unit Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_GetCompletedAppointmentsAsync_HasSnapshot_ReturnsMappedDetails()
        {
            // Arrange
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    AppointmentStart = DateTime.UtcNow,
                    Customer = new Customer { FullName = "Customer 1" },
                    AppointmentSnapshot = new AppointmentSnapshot
                    {
                        Species = "Dog",
                        Breed = "Husky",
                        VetName = "Vet 1"
                    }
                }
            };
            _repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _service.GetCompletedAppointmentsAsync();

            // Assert
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
            // Arrange
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    AppointmentStart = DateTime.UtcNow,
                    Customer = new Customer { FullName = "Customer 1" },
                    AppointmentSnapshot = null,
                    Pet = new Pet { Species = "Cat", Breed = "Maine Coon" }
                }
            };
            _repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _service.GetCompletedAppointmentsAsync();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Cat", item.PetSpecies); // Maps species string from Pet
            Assert.Equal("Maine Coon", item.PetBreed);
        }

        [Fact]
        public async Task UTCID03_GetCompletedAppointmentsAsync_NullSnapshotNullPet_ReturnsFallback()
        {
            // Arrange
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    AppointmentStart = DateTime.UtcNow,
                    Customer = null,
                    AppointmentSnapshot = null,
                    Pet = null
                }
            };
            _repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _service.GetCompletedAppointmentsAsync();

            // Assert
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
            // Arrange
            _repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ReturnsAsync(new List<Appointment>());

            // Act
            var result = await _service.GetCompletedAppointmentsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UTCID05_GetCompletedAppointmentsAsync_ThrowsException()
        {
            // Arrange
            _repoMock.Setup(repo => repo.GetCompletedAppointmentsAsync()).ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetCompletedAppointmentsAsync());
        }

        // =========================================================================
        // SECTION 5: CreateAsync() Unit Tests (UTCID01 - UTCID05)
        // =========================================================================

        [Fact]
        public async Task UTCID01_CreateAsync_DiseaseIdValid_CreatesWithDiseaseName()
        {
            // Arrange
            var diseaseId = Guid.NewGuid();
            var disease = new Disease { DiseaseId = diseaseId, Name = "Parvovirus" };
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid(), DiseaseId = diseaseId };

            _repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync(disease);

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _repoMock.Verify(repo => repo.AddAsync(It.Is<MedicalRecord>(r =>
                r.DiseaseId == diseaseId &&
                r.DiseaseNameSnapshot == "Parvovirus" &&
                r.Status == 1 // Drafted
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID02_CreateAsync_DiseaseIdNotFound_CreatesWithUnknownDisease()
        {
            // Arrange
            var diseaseId = Guid.NewGuid();
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid(), DiseaseId = diseaseId };

            _repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync((Disease?)null);

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _repoMock.Verify(repo => repo.AddAsync(It.Is<MedicalRecord>(r =>
                r.DiseaseNameSnapshot == "Unknown Disease"
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID03_CreateAsync_DiseaseIdNullCustomProvided_CreatesWithCustomName()
        {
            // Arrange
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid(), DiseaseId = null, CustomDiseaseName = "Ear Infection" };

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _repoMock.Verify(repo => repo.AddAsync(It.Is<MedicalRecord>(r =>
                r.DiseaseNameSnapshot == "Ear Infection"
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID04_CreateAsync_DiseaseIdNullCustomNull_CreatesWithDefault()
        {
            // Arrange
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid(), DiseaseId = null, CustomDiseaseName = null };

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _repoMock.Verify(repo => repo.AddAsync(It.Is<MedicalRecord>(r =>
                r.DiseaseNameSnapshot == "General Checkup"
            )), Times.Once);
        }

        [Fact]
        public async Task UTCID05_CreateAsync_ThrowsException()
        {
            // Arrange
            var dto = new CreateMedicalRecordDTO { AppointmentId = Guid.NewGuid() };
            _repoMock.Setup(repo => repo.AddAsync(It.IsAny<MedicalRecord>())).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
        }

        // =========================================================================
        // SECTION 6: UpdateAsync() Unit Tests (UTCID01 - UTCID08)
        // =========================================================================

        [Fact]
        public async Task UTCID01_UpdateAsync_StatusDraftedDiseaseExists_UpdatesWithDiseaseName()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 }; // Drafted
            var disease = new Disease { DiseaseId = diseaseId, Name = "Allergy" };
            var dto = new UpdateMedicalRecordDTO { DiseaseId = diseaseId, Diagnosis = "Updated Diag", Treatment = "Updated Treat", Note = "Updated Note" };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            _repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync(disease);

            // Act
            await _service.UpdateAsync(recordId, dto);

            // Assert
            Assert.Equal(diseaseId, record.DiseaseId);
            Assert.Equal("Allergy", record.DiseaseNameSnapshot);
            Assert.Equal("Updated Diag", record.Diagnosis);
            Assert.Equal("Updated Treat", record.Treatment);
            Assert.Equal("Updated Note", record.Note);

            _repoMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        }

        [Fact]
        public async Task UTCID02_UpdateAsync_StatusDraftedDiseaseNotFound_UpdatesWithUnknownDisease()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var diseaseId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };
            var dto = new UpdateMedicalRecordDTO { DiseaseId = diseaseId };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            _repoMock.Setup(repo => repo.GetDiseaseByIdAsync(diseaseId)).ReturnsAsync((Disease?)null);

            // Act
            await _service.UpdateAsync(recordId, dto);

            // Assert
            Assert.Equal("Unknown Disease", record.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID03_UpdateAsync_StatusDraftedCustomProvided_UpdatesWithCustomName()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };
            var dto = new UpdateMedicalRecordDTO { DiseaseId = null, CustomDiseaseName = "Eye Infection" };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            await _service.UpdateAsync(recordId, dto);

            // Assert
            Assert.Equal("Eye Infection", record.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID04_UpdateAsync_StatusDraftedCustomNull_UpdatesWithDefault()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };
            var dto = new UpdateMedicalRecordDTO { DiseaseId = null, CustomDiseaseName = null };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            await _service.UpdateAsync(recordId, dto);

            // Assert
            Assert.Equal("General Checkup", record.DiseaseNameSnapshot);
        }

        [Fact]
        public async Task UTCID05_UpdateAsync_RecordNotFound_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var dto = new UpdateMedicalRecordDTO();
            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync((MedicalRecord?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(recordId, dto));
            Assert.Equal("Medical record not found", ex.Message);
        }

        [Fact]
        public async Task UTCID06_UpdateAsync_StatusCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 2 }; // Completed
            var dto = new UpdateMedicalRecordDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(recordId, dto));
            Assert.Equal("Cannot update a medical record that is not in Drafted status", ex.Message);
        }

        [Fact]
        public async Task UTCID07_UpdateAsync_StatusCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 3 }; // Cancelled
            var dto = new UpdateMedicalRecordDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(recordId, dto));
            Assert.Equal("Cannot update a medical record that is not in Drafted status", ex.Message);
        }

        [Fact]
        public async Task UTCID08_UpdateAsync_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };
            var dto = new UpdateMedicalRecordDTO();

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            _repoMock.Setup(repo => repo.UpdateAsync(record)).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(recordId, dto));
        }

        // =========================================================================
        // SECTION 7: ChangeStatusAsync() Unit Tests (UTCID01 - UTCID07)
        // =========================================================================

        [Fact]
        public async Task UTCID01_ChangeStatusAsync_DraftedToCompleted_Success()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 }; // Drafted

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            await _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed);

            // Assert
            _repoMock.Verify(repo => repo.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed), Times.Once);
        }

        [Fact]
        public async Task UTCID02_ChangeStatusAsync_DraftedToCancelled_Success()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 }; // Drafted

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            await _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Cancelled);

            // Assert
            _repoMock.Verify(repo => repo.ChangeStatusAsync(recordId, MedicalRecordStatus.Cancelled), Times.Once);
        }

        [Fact]
        public async Task UTCID03_ChangeStatusAsync_DraftedToDrafted_Success()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 }; // Drafted

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act
            await _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted);

            // Assert
            _repoMock.Verify(repo => repo.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted), Times.Once);
        }

        [Fact]
        public async Task UTCID04_ChangeStatusAsync_RecordNotFound_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync((MedicalRecord?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed));
            Assert.Equal("Medical record not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_ChangeStatusAsync_CompletedToDrafted_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 2 }; // Completed

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted));
            Assert.Equal("Cannot change the status of a completed medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID06_ChangeStatusAsync_CancelledToDrafted_ThrowsInvalidOperationException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 3 }; // Cancelled

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Drafted));
            Assert.Equal("Cannot change the status of a cancelled medical record", ex.Message);
        }

        [Fact]
        public async Task UTCID07_ChangeStatusAsync_ThrowsException()
        {
            // Arrange
            var recordId = Guid.NewGuid();
            var record = new MedicalRecord { RecordId = recordId, Status = 1 };

            _repoMock.Setup(repo => repo.GetByIdAsync(recordId)).ReturnsAsync(record);
            _repoMock.Setup(repo => repo.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed)).ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ChangeStatusAsync(recordId, MedicalRecordStatus.Completed));
        }
    }
}
