using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Color
{
    public class UpdateColorDto
    {
        [Required]
        public string ColorName { get; set; } = null!;
        [Required]
        public string? HexCode { get; set; }
    }
}
