namespace ShopNetApi.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string TokenHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public string? Device { get; set; }
        public string? IpAddress { get; set; }
    }
}
