using System.Globalization;
using System.Text.RegularExpressions;
using PetCenterAPI.DTOs.Requests.ManageStaff;
using PetCenterAPI.DTOs.Responses.ManageStaff;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly PasswordService _passwordService;

        private const string VeterinarianRoleName = "Veterinarian";
        private const string DefaultResetPassword = "123456";
        private const string AvatarFolder = "petcenter/staff";
        private const int MinimumStaffAge = 18;

        public StaffService(
            IStaffRepository staffRepository,
            ICloudinaryService cloudinaryService,
            PasswordService passwordService)
        {
            _staffRepository = staffRepository;
            _cloudinaryService = cloudinaryService;
            _passwordService = passwordService;
        }

        // ============================================================
        // STAFF — VIEW LIST
        // ============================================================
        public async Task<List<StaffListItemResponseDTO>> GetAllAsync()
        {
            var staffs = await _staffRepository.GetAllAsync();
            return staffs.Select(MapToListItem).ToList();
        }

        // ============================================================
        // STAFF — VIEW DETAIL
        // ============================================================
        public async Task<StaffDetailResponseDTO?> GetByIdAsync(Guid staffId)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff == null) return null;

            var role = PickRole(staff);

            var dto = new StaffDetailResponseDTO
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                Gender = staff.Gender,
                BirthDate = staff.BirthDate,
                HireDate = staff.HireDate,
                Avatar = staff.Avatar,
                IsActive = staff.IsActive,
                CreatedAt = staff.CreatedAt,
                UpdatedAt = staff.UpdatedAt,
                RoleName = role?.RoleName,
                RoleId = role?.RoleId
            };

            if (staff.VetProfile != null && staff.VetProfile.IsActive)
            {
                var rating = await _staffRepository.GetVetAverageRatingAsync(staff.StaffId);
                dto.VetProfile = new VetProfileResponseDTO
                {
                    VetProfileId = staff.VetProfile.VetProfileId,
                    ExperienceYears = staff.VetProfile.ExperienceYears,
                    Description = staff.VetProfile.Description,
                    LicenseNumber = staff.VetProfile.LicenseNumber,
                    Rating = rating
                };
            }

            return dto;
        }

        // ============================================================
        // ROLES — assignable dropdown options
        // ============================================================
        public async Task<List<RoleResponseDTO>> GetAssignableRolesAsync()
        {
            var roles = await _staffRepository.GetAssignableRolesAsync();
            return roles.Select(r => new RoleResponseDTO
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();
        }

        // ============================================================
        // STAFF — CREATE (+ optional vet profile)
        // ============================================================
        public async Task<(bool Success, string Message, Guid? StaffId)> CreateAsync(CreateStaffRequestDTO request)
        {
            var role = await _staffRepository.GetRoleByIdAsync(request.RoleId);
            if (role == null || !role.IsActive)
                return (false, "Selected role does not exist.", null);

            var email = request.Email.Trim().ToLower();
            if (await _staffRepository.EmailExistsAsync(email))
                return (false, $"Email '{request.Email}' is already in use.", null);

            if (await _staffRepository.PhoneExistsAsync(request.PhoneNumber.Trim()))
                return (false, "Phone number is already in use.", null);

            if (CalculateAge(request.BirthDate) < MinimumStaffAge)
                return (false, $"Staff must be at least {MinimumStaffAge} years old.", null);

            if (request.HireDate.Date > DateTime.UtcNow.Date)
                return (false, "Hire date cannot be in the future.", null);

            bool isVet = IsVeterinarian(role);
            if (isVet)
            {
                if (string.IsNullOrWhiteSpace(request.LicenseNumber))
                    return (false, "License number is required for a veterinarian.", null);
                if (request.ExperienceYears == null)
                    return (false, "Experience years is required for a veterinarian.", null);
                if (await _staffRepository.LicenseNumberExistsAsync(request.LicenseNumber.Trim()))
                    return (false, $"License number '{request.LicenseNumber}' is already in use.", null);
            }

            string? avatarUrl = null;
            string? publicId = null;
            if (request.Avatar is { Length: > 0 })
            {
                var upload = await _cloudinaryService.UploadImageAsync(request.Avatar, AvatarFolder);
                if (upload == null || upload.Error != null)
                    return (false, "Failed to upload avatar image.", null);
                avatarUrl = upload.SecureUrl?.ToString();
                publicId = upload.PublicId;
            }

            var staff = new Staff
            {
                StaffId = Guid.NewGuid(),
                FullName = NormalizeFullName(request.FullName),
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                HireDate = request.HireDate,
                PasswordHash = _passwordService.Hash(request.Password),
                Avatar = avatarUrl,
                PublicId = publicId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            staff.Roles.Add(role);

            if (isVet)
            {
                staff.VetProfile = new VetProfile
                {
                    VetProfileId = Guid.NewGuid(),
                    StaffId = staff.StaffId,
                    LicenseNumber = request.LicenseNumber!.Trim(),
                    Description = request.Description?.Trim(),
                    ExperienceYears = request.ExperienceYears,
                    IsActive = true
                };
            }

            await _staffRepository.CreateAsync(staff);
            return (true, "Staff created successfully.", staff.StaffId);
        }

        // ============================================================
        // STAFF — UPDATE (rating/experience/license are NOT updatable)
        // ============================================================
        public async Task<(bool Success, string Message)> UpdateAsync(Guid staffId, UpdateStaffRequestDTO request)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff == null)
                return (false, "Staff not found.");

            if (!staff.IsActive)
                return (false, "Cannot update an inactive staff member.");

            var role = await _staffRepository.GetRoleByIdAsync(request.RoleId);
            if (role == null || !role.IsActive)
                return (false, "Selected role does not exist.");

            var email = request.Email.Trim().ToLower();
            if (await _staffRepository.EmailExistsAsync(email, staffId))
                return (false, $"Email '{request.Email}' is already in use.");

            if (await _staffRepository.PhoneExistsAsync(request.PhoneNumber.Trim(), staffId))
                return (false, "Phone number is already in use.");

            if (CalculateAge(request.BirthDate) < MinimumStaffAge)
                return (false, $"Staff must be at least {MinimumStaffAge} years old.");

            if (request.HireDate.Date > DateTime.UtcNow.Date)
                return (false, "Hire date cannot be in the future.");

            staff.FullName = NormalizeFullName(request.FullName);
            staff.Email = email;
            staff.PhoneNumber = request.PhoneNumber.Trim();
            staff.Gender = request.Gender;
            staff.BirthDate = request.BirthDate;
            staff.HireDate = request.HireDate;
            staff.UpdatedAt = DateTime.UtcNow;

            if (request.Avatar is { Length: > 0 })
            {
                var upload = await _cloudinaryService.UploadImageAsync(request.Avatar, AvatarFolder);
                if (upload == null || upload.Error != null)
                    return (false, "Failed to upload avatar image.");

                if (!string.IsNullOrEmpty(staff.PublicId))
                    await _cloudinaryService.DeleteImageAsync(staff.PublicId);

                staff.Avatar = upload.SecureUrl?.ToString();
                staff.PublicId = upload.PublicId;
            }

            if (request.ResetPassword)
                staff.PasswordHash = _passwordService.Hash(DefaultResetPassword);

            // Single-role assignment
            staff.Roles.Clear();
            staff.Roles.Add(role);

            if (IsVeterinarian(role))
            {
                if (staff.VetProfile == null)
                {
                    staff.VetProfile = new VetProfile
                    {
                        VetProfileId = Guid.NewGuid(),
                        StaffId = staff.StaffId,
                        Description = request.Description?.Trim(),
                        ExperienceYears = 0,
                        IsActive = true
                    };
                }
                else
                {
                    // Only Description is editable; license/experience/rating are locked.
                    staff.VetProfile.IsActive = true;
                    staff.VetProfile.Description = request.Description?.Trim();
                }
            }
            else if (staff.VetProfile != null)
            {
                // No longer a vet: keep the record but deactivate it.
                staff.VetProfile.IsActive = false;
            }

            await _staffRepository.SaveChangesAsync();
            return (true, "Staff updated successfully.");
        }

        // ============================================================
        // STAFF — SOFT DELETE
        // ============================================================
        public async Task<(bool Success, string Message)> DeleteAsync(Guid staffId)
        {
            var staff = await _staffRepository.GetByIdAsync(staffId);
            if (staff == null)
                return (false, "Staff not found.");

            if (!staff.IsActive)
                return (false, "Staff is already inactive.");

            staff.IsActive = false;
            staff.UpdatedAt = DateTime.UtcNow;

            await _staffRepository.SaveChangesAsync();
            return (true, "Staff deactivated successfully.");
        }

        // ============================================================
        // HELPERS
        // ============================================================
        /// <summary>
        /// Trims, collapses inner whitespace and applies Title Case so names are stored
        /// consistently for both Vietnamese ("nguyễn văn a" → "Nguyễn Văn A") and
        /// foreign names ("john o'brien" → "John O'Brien").
        /// </summary>
        private static string NormalizeFullName(string raw)
        {
            var collapsed = Regex.Replace(raw.Trim(), @"\s+", " ");
            var titled = CultureInfo.GetCultureInfo("vi-VN").TextInfo.ToTitleCase(collapsed.ToLower());
            // ToTitleCase lowercases the letter after an apostrophe (e.g. "O'brien");
            // fix it so names like O'Brien / D'Angelo capitalize correctly.
            return Regex.Replace(titled, @"'(\p{L})", m => "'" + m.Groups[1].Value.ToUpper());
        }

        private static bool IsVeterinarian(Role role) =>
            string.Equals(role.RoleName, VeterinarianRoleName, StringComparison.OrdinalIgnoreCase);

        /// <summary>Prefer an assignable (non-Admin) role; fall back to the first one.</summary>
        private static Role? PickRole(Staff staff)
        {
            return staff.Roles.FirstOrDefault(r => r.RoleName != "Admin")
                   ?? staff.Roles.FirstOrDefault();
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private static StaffListItemResponseDTO MapToListItem(Staff staff)
        {
            var role = PickRole(staff);
            return new StaffListItemResponseDTO
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                Gender = staff.Gender,
                BirthDate = staff.BirthDate,
                HireDate = staff.HireDate,
                Avatar = staff.Avatar,
                IsActive = staff.IsActive,
                RoleName = role?.RoleName,
                RoleId = role?.RoleId
            };
        }
    }
}
