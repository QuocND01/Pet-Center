using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Cart
{
    /// <summary>Add a product to the current customer's cart.</summary>
    public class AddToCartRequestDTO
    {
        [Required(ErrorMessage = "Product is required")]
        public Guid ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
    }

    /// <summary>Update the quantity of an existing cart line.</summary>
    public class UpdateCartDetailRequestDTO
    {
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
    }
}
