using AutoMapper;
using Moq;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCenterTestProject.SupplierTest
{
    public class SupplierUnitTest_UpdateAsync
    {
        private readonly Mock<ISupplierRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SupplierService _service;
        public SupplierUnitTest_UpdateAsync()
        {
            _repositoryMock = new Mock<ISupplierRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new SupplierService(
                _repositoryMock.Object,
                _mapperMock.Object);
        }
        #region Unit Test UpdateAsync
        [Fact]
        public async Task UTCID01_UpdateSupplier_ShouldReturnTrue()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0101234567"
            };

            var supplier = new Supplier();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _repositoryMock
                .Setup(x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result);

            _repositoryMock.Verify(x => x.Update(supplier), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UTCID02_UpdateSupplier_NotFound_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.UpdateAsync(id,
                new UpdateSupplierRequestDTO());

            Assert.False(result);
        }
        [Fact]
        public async Task UTCID03_UpdateSupplier_DuplicateTaxId_ShouldThrow()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0101234567"
            };

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(new Supplier());

            _repositoryMock
                .Setup(x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id))
                .ReturnsAsync(new Supplier
                {
                    SupplierId = Guid.NewGuid(),
                    TaxId = dto.TaxId,
                    IsActive = true
                });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(id, dto));
        }
        [Fact]
        public async Task UTCID04_UpdateSupplier_NullTaxId_ShouldCheckOtherDuplicateFields()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = null,
                SupplierName = "ABC Supplier",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0123456789"
            };

            var supplier = new Supplier();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _repositoryMock
                .Setup(x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result);

            _repositoryMock.Verify(
                x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id),
                Times.Once);

            _repositoryMock.Verify(x => x.Update(supplier), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UTCID05_UpdateSupplier_EmptyTaxId_ShouldCheckDuplicateFields()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "",
                SupplierName = "ABC Supplier",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0123456789"
            };

            var supplier = new Supplier();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _repositoryMock
                .Setup(x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result);

            _repositoryMock.Verify(
                x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Update(supplier),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
        [Fact]
        public async Task UTCID06_UpdateSupplier_SaveChangesException_ShouldThrow()
        {
            var id = Guid.NewGuid();

            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0101234567"
            };

            var supplier = new Supplier();

            _repositoryMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(supplier);

            _repositoryMock
                .Setup(x => x.FindDuplicateAsync(
                    dto.TaxId,
                    dto.SupplierName,
                    dto.SupplierEmail,
                    dto.SupplierPhoneNumber,
                    id))
                .ReturnsAsync((Supplier?)null);

            _repositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<Exception>(
                () => _service.UpdateAsync(id, dto));
        }
        #endregion
        #region Dto Validation Tests
        private static IList<ValidationResult> Validate(UpdateSupplierRequestDTO dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                dto,
                context,
                results,
                validateAllProperties: true);

            return results;
        }
        [Fact]
        public void UTCID01_ValidSupplier_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0123456789",
                SupplierName = "ABC Supplier",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                ContactPerson = "John",
                SupplierDescription = "Good supplier"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID02_SupplierNameNull_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = null!,
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Supplier name is required");
        }
        [Fact]
        public void UTCID03_SupplierName50Characters_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = new string('A', 50),
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID04_SupplierNameTooLong_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = new string('A', 51),
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Supplier name cannot exceed 50 characters");
        }
        [Fact]
        public void UTCID05_TaxIdNull_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = null,
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID06_TaxIdEmpty_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "",
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID07_TaxId10Digits_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0123456789",
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID08_TaxIdBranch_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "0123456789001",
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID09_TaxId9Digits_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "123456789", // 9 chữ số
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            // Act
            var result = Validate(dto);

            // Assert: Sửa lại câu thông báo khớp hoàn toàn với ErrorMessage ở DTO
            Assert.Contains(result, v => v.ErrorMessage == "Tax identification number must be either 10 digits or 13 digits.");
        }

        [Fact]
        public void UTCID10_TaxId11Digits_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "12345678901", // 11 chữ số
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            // Act
            var result = Validate(dto);

            // Assert: Kiểm tra xem có lỗi ở field TaxId và chứa đúng nội dung chính của ErrorMessage
            Assert.Contains(result, v =>
                v.MemberNames.Contains(nameof(UpdateSupplierRequestDTO.TaxId)) &&
                v.ErrorMessage != null &&
                v.ErrorMessage.Contains("10 digits or 13 digits"));
        }

        [Fact]
        public void UTCID11_TaxIdContainsLetters_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateSupplierRequestDTO
            {
                TaxId = "012345678A", // Chứa chữ cái
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            // Act
            var result = Validate(dto);

            // Assert
            Assert.Contains(result, v =>
                v.MemberNames.Contains(nameof(UpdateSupplierRequestDTO.TaxId)) &&
                v.ErrorMessage != null &&
                v.ErrorMessage.Contains("10 digits or 13 digits"));
        }
        [Fact]
        public void UTCID12_EmailNull_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = null!,
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Email is required");
        }
        [Fact]
        public void UTCID13_EmailEmpty_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Email is required");
        }
        [Fact]
        public void UTCID14_InvalidEmail_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abcgmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Invalid email format");
        }
        [Fact]
        public void UTCID15_Email255Characters_ShouldPassValidation()
        {
            var email = new string('a', 243) + "@gmail.com"; // tổng 253 ký tự

            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = email,
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID16_EmailTooLong_ShouldFailValidation()
        {
            var email = new string('a', 246) + "@gmail.com";

            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = email,
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Email cannot exceed 255 characters");
        }
        [Fact]
        public void UTCID17_PhoneNull_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = null!,
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Phone number is required");
        }
        [Fact]
        public void UTCID18_Phone9Digits_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "091234567",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Phone must be 10-11 digits");
        }
        [Fact]
        public void UTCID19_Phone10Digits_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID20_Phone11Digits_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "09123456789",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID21_Phone12Digits_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "091234567890",
                SupplierAddress = "Can Tho"
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Phone must be 10-11 digits");
        }
        [Fact]
        public void UTCID22_AddressNull_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = null!
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage == "Address is required");
        }
        [Fact]
        public void UTCID23_Address200Characters_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = new string('A', 200)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID24_AddressTooLong_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = new string('A', 256)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Address cannot exceed 255 characters");
        }
        [Fact]
        public void UTCID25_ContactPersonNull_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                ContactPerson = null
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID26_ContactPerson200Characters_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                ContactPerson = new string('A', 200)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID27_ContactPersonTooLong_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                ContactPerson = new string('A', 51)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                    "Contact person cannot exceed 50 characters");
        }
        [Fact]
        public void UTCID28_SupplierDescriptionNull_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                SupplierDescription = null
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID29_SupplierDescription200Characters_ShouldPassValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                SupplierDescription = new string('A', 200)
            };

            var result = Validate(dto);

            Assert.Empty(result);
        }
        [Fact]
        public void UTCID30_SupplierDescriptionTooLong_ShouldFailValidation()
        {
            var dto = new UpdateSupplierRequestDTO
            {
                SupplierName = "ABC",
                SupplierEmail = "abc@gmail.com",
                SupplierPhoneNumber = "0912345678",
                SupplierAddress = "Can Tho",
                SupplierDescription = new string('A', 256)
            };

            var result = Validate(dto);

            Assert.Contains(result,
                x => x.ErrorMessage ==
                "Supplier description cannot exceed 255 characters");
        }
        #endregion

    }
}
