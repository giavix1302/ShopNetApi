using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
