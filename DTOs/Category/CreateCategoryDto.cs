using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string? Description { get; set; }
    }
}
