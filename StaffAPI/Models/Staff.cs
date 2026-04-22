using System;
using System.Collections.Generic;

namespace StaffAPI.Models;

public partial class Staff
{
    public Guid StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string Gender { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Avatar { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual VetProfile? VetProfile { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
