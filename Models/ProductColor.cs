namespace ShopNetApi.Models
{
    public class ProductColor
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public long ColorId { get; set; }
        public Color Color { get; set; } = null!;
    }
}
