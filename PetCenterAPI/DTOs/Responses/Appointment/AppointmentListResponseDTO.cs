namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AppointmentListResponseDTO
    {
        public Guid AppointmentId { get; set; }

        public string PetName { get; set; } = string.Empty;

        public string VetName { get; set; } = string.Empty;

        public DateTime AppointmentStart { get; set; }
        public DateTime AppointmentEnd { get; set; }

        public int Status { get; set; }

        public decimal Total { get; set; }
    }

}
