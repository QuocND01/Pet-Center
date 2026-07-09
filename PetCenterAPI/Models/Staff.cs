using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

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

    public string? PublicId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ImportStock> ImportStocks { get; set; } = new List<ImportStock>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual ICollection<VetFeedback> VetFeedbacks { get; set; } = new List<VetFeedback>();

    public virtual VetProfile? VetProfile { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<ScheduleException> ScheduleExceptions { get; set; }
    = new List<ScheduleException>();
}
