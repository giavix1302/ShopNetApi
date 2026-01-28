namespace ShopNetApi.DTOs.Order
{
    public class OrderItemResponseDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductSlug { get; set; }
        public long? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHexCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
