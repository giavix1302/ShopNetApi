namespace ShopNetApi.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }
}
