namespace PetCenterClient.ViewModels.Appointment
{
    public class UpdateAppointmentPageViewModel
    {
        public Guid AppointmentId { get; set; }
        public Guid? StaffId { get; set; } // Nullable: Cho phép không bắt buộc chọn bác sĩ cố định
        public DateTime AppointmentStart { get; set; }
        public string? Note { get; set; }
        public List<Guid> SelectedServiceIds { get; set; } = new List<Guid>();

        // Master Data từ GetBookingDataAsync()
        public List<BookingStaffViewModel> Staffs { get; set; } = new List<BookingStaffViewModel>();
        public List<BookingServiceViewModel> Services { get; set; } = new List<BookingServiceViewModel>();

        // Thông tin Thú cưng (Cố định theo Lịch hẹn)
        public string PetName { get; set; } = string.Empty;
        public string? PetAvatar { get; set; }
    }
}
