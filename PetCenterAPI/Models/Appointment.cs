using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Appointment
{
    public Guid AppointmentId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid PetId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime AppointmentStart { get; set; }

    public DateTime AppointmentEnd { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Note { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual AppointmentSnapshot? AppointmentSnapshot { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
        = new List<MedicalRecord>();

    public virtual Pet Pet { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
