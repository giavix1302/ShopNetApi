namespace ShopNetApi.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
        string HashToken(string token);
        Task SaveAsync(string refreshToken, long userId, TimeSpan ttl);
        Task<long?> ValidateAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
