using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
using PetCenterAPI.Service;
using Xunit;

namespace PetCenterTestProject.AppointmentTest
{
    [Collection("DatabaseTests")]
    public class AppointmentTest_DB : IDisposable
    {
        private readonly PetCenterContext _context;
        private readonly PetCenterAPI.Service.AppointmentService _service;
        private readonly string _connectionString = "Server=.;Database=PetCenter_Test;User Id=sa;Password=123456;TrustServerCertificate=True;";

        public AppointmentTest_DB()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _context = new PetCenterContext(options);

            ClearDatabaseAsync(_context).Wait();
            _service = CreateService(_context);
        }

        public void Dispose()
        {
            ClearDatabaseAsync(_context).Wait();
            _context.Dispose();
        }

        private PetCenterAPI.Service.AppointmentService CreateService(PetCenterContext context)
        {
            var mapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile<AppointmentProfile>(); }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            var mapper = mapperConfig.CreateMapper();

            var appointmentRepo = new AppointmentRepository(context);
            var petRepo = new PetRepository(context);
            var serviceRepo = new ServiceRepository(context, mapper);
            var scheduleRepo = new ScheduleRepository(context);
            var staffRepo = new StaffRepository(context);
            var paymentRepo = new PaymentRepository(context);

            var logger = NullLogger<PetCenterAPI.Service.AppointmentService>.Instance;

            return new PetCenterAPI.Service.AppointmentService(
                appointmentRepo,
                petRepo,
                serviceRepo,
                mapper,
                logger,
                scheduleRepo,
                staffRepo,
                null,
                null,
                paymentRepo
            );
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

        #region Helper Methods

        private async Task<Customer> EnsureCustomerAsync(string email = "testcustomer@petcenter.com")
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerId = Guid.NewGuid(),
                    FullName = "Test Customer",
                    Email = email,
                    PhoneNumber = "0988776655",
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            return customer;
        }

        private async Task<Staff> EnsureVetStaffAsync(string email = "testvet@petcenter.com")
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == email);
            if (staff == null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Veterinarian");
                if (role == null)
                {
                    role = new Role { RoleId = Guid.NewGuid(), RoleName = "Veterinarian", IsActive = true };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }

                staff = new Staff
                {
                    StaffId = Guid.NewGuid(),
                    FullName = "Vet Doctor Test",
                    Email = email,
                    PhoneNumber = "0912345678",
                    BirthDate = new DateTime(1990, 1, 1),
                    Gender = "Male",
                    HireDate = DateTime.Now,
                    PasswordHash = "hash",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.Staffs.Add(staff);

                var vetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    StaffId = staff.StaffId,
                    ExperienceYears = 5,
                    Description = "Expert Vet",
                    LicenseNumber = "VET-LIC-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    IsActive = true
                };
                _context.VetProfiles.Add(vetProfile);

                await _context.SaveChangesAsync();
            }
            return staff;
        }

        private async Task<Pet> EnsurePetAsync(Guid customerId, string petName = "Buddy")
        {
            var pet = new Pet
            {
                PetId = Guid.NewGuid(),
                CustomerId = customerId,
                PetName = petName,
                Species = "Dog",
                Breed = "Golden Retriever",
                Gender = "Male",
                Weight = 12.5m,
                IsActive = true
            };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        private async Task<Service> EnsureServiceAsync(string serviceName = "Grooming Service", decimal price = 100000m, int duration = 60)
        {
            var service = new Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = serviceName,
                Price = price,
                Duration = duration,
                Status = PetCenterAPI.Common.Status.Active,
                ServiceType = 1
            };
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        private async Task EnsureGlobalWorkSchedulesAsync()
        {
            for (byte day = 1; day <= 7; day++)
            {
                if (!await _context.GlobalWorkSchedules.AnyAsync(g => g.DayOfWeek == day))
                {
                    _context.GlobalWorkSchedules.Add(new GlobalWorkSchedule
                    {
                        GlobalScheduleId = Guid.NewGuid(),
                        DayOfWeek = day,
                        IsWorking = true,
                        StartTime = new TimeOnly(8, 0, 0),
                        EndTime = new TimeOnly(18, 0, 0)
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Test Cases

        [Fact]
        public async Task UTCID01_BookAppointmentAsync_ValidRequest_Success()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();

            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(9); // Tomorrow 9:00 AM

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId },
                Note = "Routine Checkup"
            };

            var result = await _service.BookAppointmentAsync(request);

            Assert.NotNull(result);
            Assert.Equal(customer.CustomerId, result.CustomerId);
            Assert.Equal(vet.StaffId, result.StaffId);
            Assert.Equal(100000m, result.Total);
        }

        [Fact]
        public async Task UTCID02_BookAppointmentAsync_ServiceIdsNullOrEmpty_ThrowsException()
        {
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                AppointmentStart = DateTime.Now.AddDays(1),
                ServiceIds = new List<Guid>()
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.BookAppointmentAsync(request));
            Assert.Contains("Please select at least one service", ex.Message);
        }

        [Fact]
        public async Task UTCID03_BookAppointmentAsync_PastStart_ThrowsException()
        {
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                AppointmentStart = DateTime.Now.AddHours(-2),
                ServiceIds = new List<Guid> { Guid.NewGuid() }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.BookAppointmentAsync(request));
            Assert.Contains("Appointment time must be in the future", ex.Message);
        }

        [Fact]
        public async Task UTCID04_BookAppointmentAsync_NonExistingService_ThrowsException()
        {
            await ClearDatabaseAsync(_context);
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();

            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                AppointmentStart = DateTime.Now.AddDays(1),
                ServiceIds = new List<Guid> { Guid.NewGuid() }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.BookAppointmentAsync(request));
            Assert.Contains("One or more services do not exist", ex.Message);
        }

        [Fact]
        public async Task UTCID05_GetBookingDataAsync_ValidCustomer_ReturnsBookingDataResponseDTO()
        {
            await ClearDatabaseAsync(_context);
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var result = await _service.GetBookingDataAsync(customer.CustomerId);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Services);
            Assert.NotEmpty(result.Pets);
            Assert.NotEmpty(result.Staffs);
        }

        [Fact]
        public async Task UTCID06_GetMyAppointmentsAsync_ExistingCustomer_ReturnsList()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(10);
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            await _service.BookAppointmentAsync(request);

            var list = await _service.GetMyAppointmentsAsync(customer.CustomerId);

            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task UTCID07_GetAllAppointmentsAsync_ReturnsAllAppointments()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(11);
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            await _service.BookAppointmentAsync(request);

            var list = await _service.GetAllAppointmentsAsync();

            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task UTCID08_GetAppointmentDetailAsync_ExistingId_ReturnsDetail()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(13);
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            var booked = await _service.BookAppointmentAsync(request);

            var detail = await _service.GetAppointmentDetailAsync(booked.AppointmentId);

            Assert.NotNull(detail);
            Assert.Equal(booked.AppointmentId, detail.AppointmentId);
        }

        [Fact]
        public async Task UTCID09_GetAppointmentDetailAsync_NonExistingId_ThrowsException()
        {
            await ClearDatabaseAsync(_context);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetAppointmentDetailAsync(Guid.NewGuid()));
            Assert.Contains("Appointment not found", ex.Message);
        }

        [Fact]
        public async Task UTCID10_CancelAppointmentAsync_ReservedAppointment_SuccessfullyCancelled()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(14);
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            var booked = await _service.BookAppointmentAsync(request);

            await _service.CancelAppointmentAsync(booked.AppointmentId, customer.CustomerId);

            var detail = await _service.GetAppointmentDetailAsync(booked.AppointmentId);
            Assert.Equal(0, detail.Status); // Status 0 = Cancelled
        }

        [Fact]
        public async Task UTCID11_ForwardAppointmentStatusAsync_ReservedToConfirmed_AdvancesStatus()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var customer = await EnsureCustomerAsync();
            var vet = await EnsureVetStaffAsync();
            var pet = await EnsurePetAsync(customer.CustomerId);
            var service = await EnsureServiceAsync();

            var start = DateTime.Today.AddDays(1).AddHours(15);
            var request = new BookAppointmentRequestDTO
            {
                CustomerId = customer.CustomerId,
                StaffId = vet.StaffId,
                PetId = pet.PetId,
                AppointmentStart = start,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            var booked = await _service.BookAppointmentAsync(request);

            await _service.ForwardAppointmentStatusAsync(booked.AppointmentId, vet.StaffId);

            var detail = await _service.GetAppointmentDetailAsync(booked.AppointmentId);
            Assert.Equal(2, detail.Status); // Status 2 = Confirmed
        }

        [Fact]
        public async Task UTCID12_GetAvailableSlotsAsync_ValidDate_ReturnsAvailableSlots()
        {
            await ClearDatabaseAsync(_context);
            await EnsureGlobalWorkSchedulesAsync();
            var vet = await EnsureVetStaffAsync();
            var service = await EnsureServiceAsync();

            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var req = new GetAvailableSlotsRequestDTO
            {
                StaffId = vet.StaffId,
                Date = date,
                ServiceIds = new List<Guid> { service.ServiceId }
            };
            var slots = await _service.GetAvailableSlotsAsync(req);

            Assert.NotNull(slots);
            Assert.NotEmpty(slots);
        }

        #endregion
    }
}
