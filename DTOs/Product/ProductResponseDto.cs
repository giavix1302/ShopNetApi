namespace ShopNetApi.DTOs.Product
{
    public class ProductResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public long? BrandId { get; set; }
        public string? BrandName { get; set; }

        public List<ProductColorResponseDto> Colors { get; set; } = [];

        public DateTime CreatedAt { get; set; }
    }
}
