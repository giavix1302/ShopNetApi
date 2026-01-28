using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order
{
    public class OrderResponseDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? ShippingAddress { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public List<OrderTrackingResponseDto> Trackings { get; set; } = new();
    }
}
