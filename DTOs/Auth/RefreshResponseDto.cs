namespace ShopNetApi.DTOs.Auth
{
    public class RefreshResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
