namespace ShopNetApi.Models
{
    public class Product
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;

        public long? CategoryId { get; set; }
        public Category? Category { get; set; }

        public long? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public long? ColorId { get; set; }
        public Color? Color { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
