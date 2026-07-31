using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Appointment
{
    public class UpdateAppointmentViewModel
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public DateTime AppointmentStart { get; set; }

        public string? Note { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Must select at least 1 service.")]
        public List<Guid> ServiceIds { get; set; } = new List<Guid>();
    }
}
