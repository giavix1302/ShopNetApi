using ShopNetApi.DTOs.ProductSpecification;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public long CategoryId { get; set; }
        [Required]
        public long BrandId { get; set; }

        public List<ProductSpecRequestDto>? Specifications { get; set; }

        public List<long> ColorIds { get; set; }
    }
}
