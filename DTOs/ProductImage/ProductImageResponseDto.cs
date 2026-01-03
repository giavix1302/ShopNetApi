namespace ShopNetApi.DTOs.ProductImage
{
    public class ProductImageResponseDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
    }
}
