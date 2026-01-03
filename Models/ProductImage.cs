namespace ShopNetApi.Models
{
    public class ProductImage
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; } = false;
        public string PublicId { get; set; } = null!;
    }
}
