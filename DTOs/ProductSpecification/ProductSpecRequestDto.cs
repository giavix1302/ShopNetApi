using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.ProductSpecification
{
    public class ProductSpecRequestDto
    {
        [Required]
        public string Key { get; set; } = null!;

        [Required]
        public string Value { get; set; } = null!;
    }
}
