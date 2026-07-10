using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.Models;

public partial class Pet
{
    public Guid PetId { get; set; }

    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string PetName { get; set; }

    [StringLength(100)]
    public string? Species { get; set; }

    [StringLength(100)]
    public string? Breed { get; set; }

    [StringLength(50)]
    public string? Gender { get; set; }

    [Range(0, 10000)]
    public decimal? Weight { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? PetAvatar { get; set; }

    public string? PublicId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    = new List<MedicalRecord>();

    public virtual Customer Customer { get; set; } = null!;
}
