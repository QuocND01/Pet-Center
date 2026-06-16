using PetCenterClient.DTOs;

namespace PetCenterClient.ViewModels.ManageStaff
{
    /// <summary>Backing model for the staff management list page.</summary>
    public class StaffIndexViewModel
    {
        public List<StaffListItemDto> Items { get; set; } = new();
        public List<RoleDto> Roles { get; set; } = new();

        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        /// <summary>"active" (default), "inactive" or "all".</summary>
        public string Status { get; set; } = "active";

        public Guid? RoleId { get; set; }
    }
}
