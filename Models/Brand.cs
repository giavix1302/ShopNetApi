namespace ShopNetApi.Models
{
    public class Brand
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
