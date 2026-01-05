namespace ShopNetApi.DTOs.Color
{
    public class ColorResponseDto
    {
        public long Id { get; set; }
        public string ColorName { get; set; } = null!;
        public string? HexCode { get; set; }
    }
}
