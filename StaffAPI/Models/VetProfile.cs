using System;
using System.Collections.Generic;

namespace StaffAPI.Models;

public partial class VetProfile
{
    public Guid VetProfileId { get; set; }

    public Guid StaffId { get; set; }

    public decimal? ExperienceYears { get; set; }

    public string? Description { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
