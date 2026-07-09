using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class AppointmentSnapshot
{
    public Guid AppointmentSnapshotId { get; set; }

    public Guid AppointmentId { get; set; }

    public string Species { get; set; } = null!;

    public string Breed { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal? ExperienceYears { get; set; }

    public decimal? Rating { get; set; }

    public string VetName { get; set; } = null!;

    public virtual Appointment Appointment { get; set; } = null!;
}
