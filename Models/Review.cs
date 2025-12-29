namespace ShopNetApi.Models
{
    public class Review
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
