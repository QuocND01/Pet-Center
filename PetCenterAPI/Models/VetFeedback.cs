using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class VetFeedback
{
    public Guid VetFeedbackId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid StaffId { get; set; }

    public int? Star { get; set; }

    public string? Comment { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
