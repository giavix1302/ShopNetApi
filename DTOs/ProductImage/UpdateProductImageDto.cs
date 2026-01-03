namespace ShopNetApi.DTOs.ProductImage
{
    public class UpdateProductImageDto
    {
        public IFormFile? Image { get; set; }
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
    }
}
