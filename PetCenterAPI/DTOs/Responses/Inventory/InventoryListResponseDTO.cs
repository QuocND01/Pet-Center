namespace PetCenterAPI.DTOs.Responses.Inventory
{
    public class InventoryListResponseDTO
    {
        public List<InventoryItemResponseDTO> Items { get; set; }
            = new();

        public int TotalRecords { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
