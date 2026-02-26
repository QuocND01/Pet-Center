using System;
using System.Collections.Generic;

namespace Pet_Center.Models;

public partial class Staff
{
    public Guid StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime? BirthDay { get; set; }

    public string? Gender { get; set; }

    public string? Email { get; set; }

    public DateTime? HiredDate { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ImportStock> ImportStocks { get; set; } = new List<ImportStock>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
