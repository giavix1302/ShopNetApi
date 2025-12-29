using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Otp { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
