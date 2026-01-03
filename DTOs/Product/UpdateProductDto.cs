namespace ShopNetApi.DTOs.Product
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ColorId { get; set; }
    }
}
