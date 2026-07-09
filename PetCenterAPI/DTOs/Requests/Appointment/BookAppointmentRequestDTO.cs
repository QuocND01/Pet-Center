namespace PetCenterAPI.DTOs.Requests.Appointment
{
    public class BookAppointmentRequestDTO
    {
        public Guid CustomerId { get; set; }

        public Guid PetId { get; set; }

        public Guid StaffId { get; set; }

        public DateTime AppointmentStart { get; set; }

        public string? Note { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();
    }
}
