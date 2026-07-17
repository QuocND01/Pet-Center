namespace PetCenterAPI.DTOs.Requests.Appointment
{
    public class GetAvailableSlotsRequestDTO
    {
        public Guid StaffId { get; set; }

        public DateOnly Date { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();
    }
}
