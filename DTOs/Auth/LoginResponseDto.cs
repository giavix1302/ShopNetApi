namespace ShopNetApi.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public UserResponseDto User { get; set; } = null!;
    }
}
