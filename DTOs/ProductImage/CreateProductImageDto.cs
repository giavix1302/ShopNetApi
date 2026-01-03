namespace ShopNetApi.DTOs.ProductImage
{
    public class CreateProductImageDto
    {
        public long ProductId { get; set; }
        public IFormFile Image { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; } = false;
    }
}
