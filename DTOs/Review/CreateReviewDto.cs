using System.ComponentModel.DataAnnotations;
using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "ProductId là bắt buộc")]
        public long ProductId { get; set; }

        public long? OrderItemId { get; set; }

        [Required(ErrorMessage = "Rating là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment không được vượt quá 1000 ký tự")]
        public string? Comment { get; set; }
    }
}
