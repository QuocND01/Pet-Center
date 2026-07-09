namespace PetCenterAPI.DTOs.Requests.Appointment
{
    public class UpdateAppointmentRequestDTO
    {
        public Guid StaffId { get; set; }

        public DateTime AppointmentStart { get; set; }

        public string? Note { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();
    }
}
