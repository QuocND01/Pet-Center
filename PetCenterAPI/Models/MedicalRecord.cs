using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class MedicalRecord
{
    public Guid RecordId { get; set; }

    public Guid PetId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? DiseaseId { get; set; }

    public string DiseaseNameSnapshot { get; set; } = null!;

    public string Diagnosis { get; set; } = null!;

    public string Treatment { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Status { get; set; }

    // Navigation Properties
    public virtual Pet Pet { get; set; } = null!;

    public virtual Appointment? Appointment { get; set; }

    public virtual Disease? Disease { get; set; }

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; }
        = new List<PrescriptionItem>();
}