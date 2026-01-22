using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Cart
{
    public class AddCartItemDto
    {
        [Required(ErrorMessage = "ProductId là bắt buộc")]
        public long ProductId { get; set; }

        public long? ColorId { get; set; }

        [Required(ErrorMessage = "Quantity là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
