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
        public class ServiceTest_Mock
        {
            private readonly Mock<IServiceRepository> _serviceRepositoryMock;
            private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
            private readonly IMapper _mapper;
            private readonly ServiceService _service;

            public ServiceTest_Mock()
            {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ServiceProfile>();
            }, NullLoggerFactory.Instance)
       .CreateMapper();

            _serviceRepositoryMock = new Mock<IServiceRepository>();
                _cloudinaryServiceMock = new Mock<ICloudinaryService>();

                _service = new ServiceService(
                    _serviceRepositoryMock.Object,
                    _mapper,
                    _cloudinaryServiceMock.Object);
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
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Price = 100,
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllService())
                    .Returns(services.AsQueryable());

                // Act
                // OData QueryOptions is null, so it throws NullReferenceException in the service
                await Assert.ThrowsAsync<NullReferenceException>(() =>
                    _service.GetAllServiceAsync(null!));

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetAllService(), Times.Once);
            }

            [Fact]
            public async Task UTCID02_GetAllServiceAsync()
            {
                // Arrange
                var services = new List<PetCenterAPI.Models.Service>();

                _serviceRepositoryMock
                    .Setup(x => x.GetAllService())
                    .Returns(services.AsQueryable());

                // Act
                // OData QueryOptions is null, so it throws NullReferenceException in the service
                await Assert.ThrowsAsync<NullReferenceException>(() =>
                    _service.GetAllServiceAsync(null!));

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetAllService(), Times.Once);
            }

            [Fact]
            public async Task UTCID03_GetAllServiceAsync()
            {
                // Arrange
                _serviceRepositoryMock
                    .Setup(x => x.GetAllService())
                    .Throws(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetAllServiceAsync(null!));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
                _serviceRepositoryMock.Verify(x => x.GetAllService(), Times.Once);
            }

            //=========================================================
            // GetAllServiceAdminAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID02_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID03_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((new List<PetCenterAPI.Models.Service>(), 0));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Data);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task UTCID04_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID05_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID06_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID07_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((new List<PetCenterAPI.Models.Service>(), 0));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Data);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task UTCID08_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };
                var services = new List<PetCenterAPI.Models.Service>
            {
                new PetCenterAPI.Models.Service
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = "Pet Grooming",
                    Status = PetCenterAPI.Common.Status.Active
                }
            };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((services, 1));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result.Data);
                Assert.Equal(1, result.TotalCount);
            }

            [Fact]
            public async Task UTCID09_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((new List<PetCenterAPI.Models.Service>(), 0));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Data);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task UTCID10_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ReturnsAsync((new List<PetCenterAPI.Models.Service>(), 0));

                // Act
                var result = await _service.GetAllServiceAdminAsync(spec);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Data);
                Assert.Equal(0, result.TotalCount);
            }

            [Fact]
            public async Task UTCID11_GetAllServiceAdminAsync()
            {
                // Arrange
                var spec = new ServiceSpecification { Page = 1, PageSize = 10 };

                _serviceRepositoryMock
                    .Setup(x => x.GetAllServiceAdminAsync(spec))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetAllServiceAdminAsync(spec));

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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service
                {
                    ServiceId = id,
                    ServiceName = "Pet Grooming",
                    Price = 100
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                // Act
                var result = await _service.GetServiceByIdAsync(id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(id, result.ServiceId);
                Assert.Equal("Pet Grooming", result.ServiceName);
            }

            [Fact]
            public async Task UTCID02_GetServiceByIdAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetServiceByIdAsync(id));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID03_GetServiceByIdAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.GetServiceByIdAsync(id));

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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID03_AddServiceAsync()
            {
                // Arrange
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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(true);

                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.AddServiceAsync(dto));

                // Assert
                Assert.Equal("Service already exists", ex.Message);
            }

            [Fact]
            public async Task UTCID06_AddServiceAsync()
            {
                // Arrange
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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID08_AddServiceAsync()
            {
                // Arrange
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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID10_AddServiceAsync()
            {
                // Arrange
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID11_AddServiceAsync()
            {
                // Arrange
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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID14_AddServiceAsync()
            {
                // Arrange
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
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID17_AddServiceAsync()
            {
                // Arrange
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            [Fact]
            public async Task UTCID18_AddServiceAsync()
            {
                // Arrange
                var dto = new CreateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(false);

                // Act
                await _service.AddServiceAsync(dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.AddServiceAsync(It.IsAny<PetCenterAPI.Models.Service>()), Times.Once);
            }

            //=========================================================
            // UpdateServiceAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID03_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(true);

                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.UpdateServiceAsync(id, dto));

                // Assert
                Assert.Equal("Service already exists", ex.Message);
            }

            [Fact]
            public async Task UTCID06_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID08_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID10_UpdateServiceAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID11_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID14_UpdateServiceAsync()
            {
                // Arrange
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
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            [Fact]
            public async Task UTCID17_UpdateServiceAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.UpdateServiceAsync(id, dto));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID18_UpdateServiceAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.UpdateServiceAsync(id, dto));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }

            [Fact]
            public async Task UTCID19_UpdateServiceAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync((PetCenterAPI.Models.Service?)null);

                // Act
                var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateServiceAsync(id, dto));

                // Assert
                Assert.Equal("Service not found", ex.Message);
            }

            [Fact]
            public async Task UTCID20_UpdateServiceAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id };
                var dto = new UpdateServiceDTO
                {
                    ServiceName = "Valid Name",
                    Price = 100,
                    Duration = 30,
                    ServiceType = 1
                };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                _serviceRepositoryMock
                    .Setup(x => x.CheckServiceExistAsync(It.IsAny<string>(), id))
                    .ReturnsAsync(false);

                // Act
                await _service.UpdateServiceAsync(id, dto);

                // Assert
                _serviceRepositoryMock.Verify(x => x.GetServiceByIdAsync(It.IsAny<Guid>()), Times.Once);
            }

            //=========================================================
            // ChangeServiceStatusAsync()
            //=========================================================

            [Fact]
            public async Task UTCID01_ChangeServiceStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                // Act
                await _service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _serviceRepositoryMock.Verify(x => x.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID02_ChangeServiceStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                // Act
                await _service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _serviceRepositoryMock.Verify(x => x.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID03_ChangeServiceStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();
                var serviceEntity = new PetCenterAPI.Models.Service { ServiceId = id, Status = PetCenterAPI.Common.Status.Inactive };

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync(serviceEntity);

                // Act
                await _service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active);

                // Assert
                _serviceRepositoryMock.Verify(x => x.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active, false), Times.Once);
            }

            [Fact]
            public async Task UTCID04_ChangeServiceStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ReturnsAsync((PetCenterAPI.Models.Service?)null);

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active));

                // Assert
                Assert.Equal("Service not found", ex.Message);
            }

            [Fact]
            public async Task UTCID05_ChangeServiceStatusAsync()
            {
                // Arrange
                var id = Guid.NewGuid();

                _serviceRepositoryMock
                    .Setup(x => x.GetServiceByIdAsync(id))
                    .ThrowsAsync(new Exception("Service Temporarily Unavailable"));

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    _service.ChangeServiceStatusAsync(id, PetCenterAPI.Common.Status.Active));

                // Assert
                Assert.Equal("Service Temporarily Unavailable", ex.Message);
            }


        }
    }
