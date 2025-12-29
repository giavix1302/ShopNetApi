using ShopNetApi.Models;

namespace ShopNetApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> SignInAsync(ApplicationUser user);

        Task<string?> RefreshAsync(string refreshToken);

        Task LogoutAsync(string refreshToken);

    }
}
