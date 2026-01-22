using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Cart
{
    public class UpdateCartItemDto
    {
        [Required(ErrorMessage = "Quantity là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
