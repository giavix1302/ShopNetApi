namespace ShopNetApi.Models
{
    public class OrderTracking
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string? Location { get; set; }
        public string? Description { get; set; }

        public OrderStatus Status { get; set; }
        public string? Note { get; set; }

        public string? TrackingNumber { get; set; }
        public string? ShippingPattern { get; set; }

        public DateTime? EstimatedDelivery { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
