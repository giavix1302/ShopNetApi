namespace ShopNetApi.Models
{
    public class Color
    {
        public long Id { get; set; }

        public string ColorName { get; set; } = null!;
        public string? HexCode { get; set; }
    }
}
