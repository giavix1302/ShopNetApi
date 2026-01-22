using ShopNetApi.DTOs.Auth;

namespace ShopNetApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task RegisterAsync(RegisterDto dto);
        Task<string> VerifyRegisterOtpAsync(VerifyOtpDto dto);
        Task<string?> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
