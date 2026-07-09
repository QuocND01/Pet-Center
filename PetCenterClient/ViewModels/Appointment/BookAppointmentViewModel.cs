using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Appointment
{
    public class BookAppointmentViewModel
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid PetId { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public DateTime AppointmentStart { get; set; }

        public string? Note { get; set; }

        [Required]
        public List<Guid> ServiceIds { get; set; } = new();

        // Dùng để bind dropdown

        public List<SelectListItem> Customers { get; set; } = new();

        public List<SelectListItem> Pets { get; set; } = new();

        public List<SelectListItem> Staffs { get; set; } = new();

        public List<SelectListItem> Services { get; set; } = new();
    }

}
