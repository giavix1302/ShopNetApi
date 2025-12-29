using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string? FullName { get; set; }
    }
}
