namespace PetCenterAPI.DTOs.Responses.ManageStaff
{
    /// <summary>Lightweight staff row for the management list.</summary>
    public class StaffListItemResponseDTO
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }
    }

    /// <summary>Full staff detail, including the vet profile when applicable.</summary>
    public class StaffDetailResponseDTO
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }

        /// <summary>Present only when the staff member is a Veterinarian.</summary>
        public VetProfileResponseDTO? VetProfile { get; set; }
    }

    public class VetProfileResponseDTO
    {
        public Guid VetProfileId { get; set; }
        public decimal? ExperienceYears { get; set; }
        public string? Description { get; set; }
        public string? LicenseNumber { get; set; }

        /// <summary>Average star rating computed from this vet's feedbacks (read-only).</summary>
        public decimal Rating { get; set; }
    }

    /// <summary>Assignable role option for the create/update dropdowns.</summary>
    public class RoleResponseDTO
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
