using ShopNetApi.DTOs.ProductSpecification;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Product
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public long? CategoryId { get; set; }
        [Required]
        public long? BrandId { get; set; }

        public List<ProductSpecRequestDto>? Specifications { get; set; }

        public List<long>? ColorIds { get; set; }
    }
}
