using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.ProductSpecification
{
    public class ProductSpecRequestDto
    {
        [Required]
        public string SpecName { get; set; } = null!;

        [Required]
        public string SpecValue { get; set; } = null!;
    }
}
