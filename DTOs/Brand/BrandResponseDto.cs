namespace ShopNetApi.DTOs.Brand
{
    public class BrandResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
