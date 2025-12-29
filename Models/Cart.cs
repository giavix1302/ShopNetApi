namespace ShopNetApi.Models
{
    public class Cart
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
