using System;
using System.Collections.Generic;

namespace MedicalAPI.Models;

public partial class PrescriptionItem
{
    public Guid PrescriptionItemId { get; set; }

    public Guid RecordId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string Dosage { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Note { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
