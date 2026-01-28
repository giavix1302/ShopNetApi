using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order
{
    public class OrderTrackingResponseDto
    {
        public long Id { get; set; }
        public OrderStatus Status { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingPattern { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
