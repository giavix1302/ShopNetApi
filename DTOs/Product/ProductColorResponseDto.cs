namespace ShopNetApi.DTOs.Product
{
    public class ProductColorResponseDto
    {
        public long Id { get; set; }
        public string ColorName { get; set; } = null!;
        public string? HexCode { get; set; }
    }
}
