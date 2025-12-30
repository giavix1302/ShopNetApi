using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Brand
{
    public class CreateBrandDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string? Description { get; set; }
    }
}
