namespace PetCenterClient.ViewModels.Inventory
{
    public class InventoryListResponseViewModel
    {
        public List<InventoryViewModel> Items { get; set; }
            = new();

        public int TotalRecords { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
