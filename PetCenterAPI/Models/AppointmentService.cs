using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class AppointmentService
{
    public Guid AppointmentServiceId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }

    public int Duration { get; set; }
    public DateTime? CompleteAt { get; set; }
    public int? Status { get; set; }
    public int ServiceType { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
