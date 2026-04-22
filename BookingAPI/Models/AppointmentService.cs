using System;
using System.Collections.Generic;

namespace BookingAPI.Models;

public partial class AppointmentService
{
    public Guid AppointmentServiceId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }

    public string? Duration { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
