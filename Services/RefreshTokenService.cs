using ShopNetApi.Services.Interfaces;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace ShopNetApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDatabase _redis;

        public RefreshTokenService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        public async Task SaveAsync(string refreshToken, long userId, TimeSpan ttl)
        {
            var hash = HashToken(refreshToken);
            await _redis.StringSetAsync(
                $"refresh_token:{hash}",
                userId,
                ttl
            );
        }

        public async Task<long?> ValidateAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var value = await _redis.StringGetAsync($"refresh_token:{hash}");

            if (!value.HasValue)
                return null;

            return (long)value;
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            await _redis.KeyDeleteAsync($"refresh_token:{hash}");
        }

    }
}
