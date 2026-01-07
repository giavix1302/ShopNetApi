using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Product
{
    public class CreateProductColorDto
    {
        [Required]
        public long ColorId { get; set; }
    }
}
