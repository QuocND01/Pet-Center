using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalAPI.Models;

public partial class MedicalRecord
{
    [Key]
    public Guid RecordId { get; set; }

    public Guid AppointmentId { get; set; }

    public string Diagnosis { get; set; } = null!;

    public string Treatment { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
