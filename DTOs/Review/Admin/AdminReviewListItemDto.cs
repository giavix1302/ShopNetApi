namespace ShopNetApi.DTOs.Review.Admin
{
    public class AdminReviewListItemDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? UserName { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public long? OrderItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
