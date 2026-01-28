namespace ShopNetApi.DTOs.Review
{
    public class ReviewListResponseDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserAvatarUrl { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductSlug { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
