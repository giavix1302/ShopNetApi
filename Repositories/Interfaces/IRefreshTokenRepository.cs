using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetLatestValidAsync(long userId);
        Task RevokeAsync(RefreshToken token);
        Task SaveChangesAsync();
    }
}
