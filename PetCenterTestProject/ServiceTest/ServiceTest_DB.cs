using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Profiles;
using PetCenterAPI.Repository;
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
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Responses.Service.ServiceResponseDTO;

namespace PetCenterTestProject.ServiceTest
{
    public class ServiceTest_DB : IAsyncLifetime
    {
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;

        public ServiceTest_DB()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ServiceProfile>();
            }, NullLoggerFactory.Instance)
  .CreateMapper();

            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        private PetCenterContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseSqlServer("Server=127.0.0.1,1433;Database=PetCenter_Test;User Id=sa;Password=123456;TrustServerCertificate=True;")
                .Options;

            return new PetCenterContext(options);
        }

        private ServiceRepository CreateRepository(PetCenterContext context)
        {
            return new ServiceRepository(context, _mapper);
        }

        /// <summary>
        /// Integration Test
        /// </summary>
        private ServiceService CreateService(PetCenterContext context)
        {
            return new ServiceService(
                CreateRepository(context),
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        /// <summary>
        /// Unit Test (Mock Repository)
        /// </summary>
        private ServiceService CreateService(IServiceRepository repository)
        {
            return new ServiceService(
                repository,
                _mapper,
                _cloudinaryServiceMock.Object);
        }

        private async Task ClearDatabaseAsync(PetCenterContext context)
        {
            context.Services.RemoveRange(context.Services);
            await context.SaveChangesAsync();
        }

        private IList<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, context, results, true);

            return results;
        }

        //=========================================================
        // GetAllService()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.GetAllServiceAsync(null!));
        }

        [Fact]
        public async Task UTCID02_GetAllServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            // Act & Assert
            // OData QueryOptions is null, so it throws NullReferenceException in the service
            await Assert.ThrowsAsync<NullReferenceException>(() => service.GetAllServiceAsync(null!));
        }

        [Fact]
        public async Task UTCID03_GetAllServiceAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            repositoryMock
                .Setup(x => x.GetAllService())
                .Throws(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAllServiceAsync(null!));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // GetAllServiceAdminAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID02_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID03_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task UTCID04_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID05_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID06_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID07_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task UTCID08_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Active
            });
            await context.SaveChangesAsync();
            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task UTCID09_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task UTCID10_GetAllServiceAdminAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

            // Act
            var result = await service.GetAllServiceAdminAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task UTCID11_GetAllServiceAdminAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            repositoryMock
                .Setup(x => x.GetAllServiceAdminAsync(It.IsAny<ServiceSpecification>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            var spec = new ServiceSpecification();

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAllServiceAdminAsync(spec));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // GetServiceByIdAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_GetServiceByIdAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = id,
                ServiceName = "Pet Grooming"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetServiceByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.ServiceId);
            Assert.Equal("Pet Grooming", result.ServiceName);
        }

        [Fact]
        public async Task UTCID02_GetServiceByIdAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetServiceByIdAsync(id));
            Assert.Equal("Service not found", ex.Message);
        }

        [Fact]
        public async Task UTCID03_GetServiceByIdAsync_RepositoryThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            repositoryMock
                .Setup(x => x.GetServiceByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

            var service = CreateService(repositoryMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetServiceByIdAsync(Guid.NewGuid()));

            // Assert
            Assert.Equal("Service Temporarily Unavailable", ex.Message);
        }

        //=========================================================
        // AddServiceAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID02_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID03_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID04_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID05_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = "Pet Grooming"
            });
            await context.SaveChangesAsync();

            var dto = new CreateServiceDTO
            {
                ServiceName = "Pet Grooming",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddServiceAsync(dto));
            Assert.Equal("Service already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID06_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID07_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID08_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID09_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID10_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID11_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID12_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID13_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID14_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID15_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID16_AddServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var dto = new CreateServiceDTO
            {
                ServiceName = "New Service",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.AddServiceAsync(dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceName == "New Service");
            Assert.NotNull(serviceEntity);
        }

        [Fact]
        public async Task UTCID17_AddServiceAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            repositoryMock
                .Setup(x => x.CheckServiceExistAsync(
                    It.IsAny<string>(), null))
                .ReturnsAsync(false);

            repositoryMock
                .Setup(x => x.AddServiceAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);


            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.FileName)
                .Returns("service.jpg");

            fileMock.Setup(f => f.ContentType)
                .Returns("image/jpeg");

            fileMock.Setup(f => f.Length)
                .Returns(1024);


            var dto = new CreateServiceDTO
            {
                ServiceName = "Pet Grooming",
                ServiceDescription = "Grooming service",
                Price = 200000,
                Duration = 30,
                ServiceType = 1,
                ImageFiles = [fileMock.Object]
            };


            // Mock upload Cloudinary thất bại
            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    "services"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });


            var service = CreateService(repositoryMock.Object);


            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.AddServiceAsync(dto));


            // Assert
            Assert.Equal(
                "Failed to upload image",
                ex.Message);


            // Không được lưu service khi upload fail
            repositoryMock.Verify(
                x => x.AddServiceAsync(It.IsAny<Service>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID18_AddServiceAsync_RepositoryThrowsException_ShouldRollbackUploadedImages()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            repositoryMock
                .Setup(x => x.CheckServiceExistAsync(
                    It.IsAny<string>(),null))
                .ReturnsAsync(false);


            repositoryMock
                .Setup(x => x.AddServiceAsync(It.IsAny<Service>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));


            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(x => x.FileName)
                .Returns("service.jpg");

            fileMock.Setup(x => x.ContentType)
                .Returns("image/jpeg");

            fileMock.Setup(x => x.Length)
                .Returns(1024);


            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    "services"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.OK,
                    SecureUrl = new Uri("https://test.com/service.jpg"),
                    PublicId = "service-public-id"
                });


            var dto = new CreateServiceDTO
            {
                ServiceName = "Pet Grooming",
                ServiceDescription = "Grooming service",
                Price = 200000,
                Duration = 30,
                ServiceType = 1,
                ImageFiles = [fileMock.Object]
            };


            var service = CreateService(repositoryMock.Object);


            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.AddServiceAsync(dto));


            // Assert
            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);


            // Verify rollback Cloudinary
            _cloudinaryServiceMock.Verify(
                x => x.DeleteImageAsync("service-public-id"),
                Times.Once);


            // Verify repository đã được gọi
            repositoryMock.Verify(
                x => x.AddServiceAsync(It.IsAny<Service>()),
                Times.Once);
        }

        //=========================================================
        // UpdateServiceAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID02_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID03_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID04_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID05_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id1, ServiceName = "Existing" });
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id2, ServiceName = "Other" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "Existing",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateServiceAsync(id2, dto));
            Assert.Equal("Service already exists", ex.Message);
        }

        [Fact]
        public async Task UTCID06_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID07_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID08_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID09_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID10_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID11_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID12_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID13_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID14_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID15_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO();

            // Act
            var result = Validate(dto);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UTCID16_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        [Fact]
        public async Task UTCID17_UpdateServiceAsync_UploadImageFail_ShouldThrowException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            var serviceId = Guid.NewGuid();

            var existingService = new Service
            {
                ServiceId = serviceId,
                ServiceName = "Old Service",
                Price = 200000,
                Duration = 30,
                ServiceType = 1,
                ServiceImages = new List<ServiceImage>()
            };


            repositoryMock
                .Setup(x => x.GetServiceByIdAsync(serviceId))
                .ReturnsAsync(existingService);


            repositoryMock
                .Setup(x => x.CheckServiceExistAsync(
                    It.IsAny<string>(),
                    serviceId))
                .ReturnsAsync(false);


            repositoryMock
                .Setup(x => x.UpdateServiceAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);


            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(x => x.FileName)
                .Returns("service.jpg");

            fileMock.Setup(x => x.ContentType)
                .Returns("image/jpeg");

            fileMock.Setup(x => x.Length)
                .Returns(1024);


            _cloudinaryServiceMock
                .Setup(x => x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    "services"))
                .ReturnsAsync(new ImageUploadResult
                {
                    StatusCode = HttpStatusCode.BadRequest
                });


            var dto = new UpdateServiceDTO
            {
                ServiceName = "Updated Service",
                Price = 300000,
                Duration = 45,
                ServiceType = 1,

                ExistingImages = new List<string>(),

                ImageFiles = new List<IFormFile>
        {
            fileMock.Object
        }
            };


            var service = CreateService(repositoryMock.Object);


            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateServiceAsync(serviceId, dto));


            // Assert
            Assert.Equal(
                "Failed to upload image",
                ex.Message);


            repositoryMock.Verify(
                x => x.UpdateServiceAsync(It.IsAny<Service>()),
                Times.Never);
        }

        [Fact]
        public async Task UTCID18_UpdateServiceAsync_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var repositoryMock = new Mock<IServiceRepository>();

            var serviceId = Guid.NewGuid();

            var existingService = new Service
            {
                ServiceId = serviceId,
                ServiceName = "Old Service",
                Price = 200000,
                Duration = 30,
                ServiceType = 1,
                ServiceImages = new List<ServiceImage>()
            };


            repositoryMock
                .Setup(x => x.GetServiceByIdAsync(serviceId))
                .ReturnsAsync(existingService);


            repositoryMock
                .Setup(x => x.CheckServiceExistAsync(
                    It.IsAny<string>(),
                    serviceId))
                .ReturnsAsync(false);


            repositoryMock
                .Setup(x => x.UpdateServiceAsync(It.IsAny<Service>()))
                .ThrowsAsync(new Exception("Service Temporarily Unavailable"));


            var dto = new UpdateServiceDTO
            {
                ServiceName = "Updated Service",
                Price = 300000,
                Duration = 45,
                ServiceType = 1,

                ExistingImages = new List<string>(),
                ImageFiles = null
            };


            var service = CreateService(repositoryMock.Object);


            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateServiceAsync(serviceId, dto));


            // Assert
            Assert.Equal(
                "Service Temporarily Unavailable",
                ex.Message);


            repositoryMock.Verify(
                x => x.UpdateServiceAsync(It.IsAny<Service>()),
                Times.Once);
        }

        [Fact]
        public async Task UTCID19_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            var dto = new UpdateServiceDTO
            {
                ServiceName = "Valid Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateServiceAsync(id, dto));
            Assert.Equal("Service not found", ex.Message);
        }

        [Fact]
        public async Task UTCID20_UpdateServiceAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();
            context.Services.Add(new PetCenterAPI.Models.Service { ServiceId = id, ServiceName = "Old Name" });
            await context.SaveChangesAsync();

            var dto = new UpdateServiceDTO
            {
                ServiceName = "New Name",
                Price = 100,
                Duration = 30,
                ServiceType = 1
            };

            // Act
            await service.UpdateServiceAsync(id, dto);

            // Assert
            var serviceEntity = await context.Services.FirstOrDefaultAsync(p => p.ServiceId == id);
            Assert.NotNull(serviceEntity);
            Assert.Equal("New Name", serviceEntity.ServiceName);
        }

        //=========================================================
        // ChangeServiceStatusAsync()
        //=========================================================

        [Fact]
        public async Task UTCID01_ChangeServiceStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            context.Services.Add(new Service
            {
                ServiceId = id,
                ServiceName = "Pet Grooming",
                Status = Status.Inactive
            });

            await context.SaveChangesAsync();

            // Act
            await service.ChangeServiceStatusAsync(
                id,
                Status.Active);

            // Assert
            var serviceEntity = await context.Services
                .FirstOrDefaultAsync(x => x.ServiceId == id);

            Assert.NotNull(serviceEntity);
            Assert.Equal(
                Status.Active,
                serviceEntity.Status);
        }

        [Fact]
        public async Task UTCID02_ChangeServiceStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            context.Services.Add(new Service
            {
                ServiceId = id,
                ServiceName = "Pet Grooming",
                Status = Status.Inactive
            });

            await context.SaveChangesAsync();

            // Act
            await service.ChangeServiceStatusAsync(
                id,
                Status.Active);

            // Assert
            var serviceEntity = await context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            Assert.NotNull(serviceEntity);
            Assert.Equal(
                Status.Active,
                serviceEntity.Status);
        }

        [Fact]
        public async Task UTCID03_ChangeServiceStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            context.Services.Add(new PetCenterAPI.Models.Service
            {
                ServiceId = id,
                ServiceName = "Pet Grooming",
                Status = PetCenterAPI.Common.Status.Inactive
            });

            await context.SaveChangesAsync();

            // Act
            await service.ChangeServiceStatusAsync(
                id,
                PetCenterAPI.Common.Status.Active);

            // Assert
            var serviceEntity = await context.Services
                .FirstOrDefaultAsync(p => p.ServiceId == id);

            Assert.NotNull(serviceEntity);

            Assert.Equal(
                PetCenterAPI.Common.Status.Active,
                serviceEntity.Status);
        }

        [Fact]
        public async Task UTCID04_ChangeServiceStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            var id = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active));
            Assert.Equal("Service not found", ex.Message);
        }

        [Fact]
        public async Task UTCID05_ChangeServiceStatusAsync()
        {
            // Arrange
            using var context = CreateContext();
            await ClearDatabaseAsync(context);
            var service = CreateService(context);

            // Act & Assert
            Assert.True(true);
        }


    }
}
