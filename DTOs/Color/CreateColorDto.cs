namespace ShopNetApi.DTOs.Color
{
    public class CreateColorDto
    {
        public string ColorName { get; set; } = null!;
        public string? HexCode { get; set; }
    }
}
