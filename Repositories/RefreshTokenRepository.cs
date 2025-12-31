using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _db;

        public RefreshTokenRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetLatestValidAsync(long userId)
        {
            return await _db.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task RevokeAsync(RefreshToken token)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
