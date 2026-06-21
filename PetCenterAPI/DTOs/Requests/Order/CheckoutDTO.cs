using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Order
{
    public class CodOrderItemDTO
    {
        [Required]
        public Guid CartDetailId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class PlaceCodOrderDTO
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid AddressId { get; set; }

        public Guid? VoucherId { get; set; }

        [Required, MinLength(1)]
        public List<CodOrderItemDTO> Items { get; set; } = new();
    }
}
