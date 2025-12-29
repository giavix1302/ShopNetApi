namespace ShopNetApi.Models
{
    public class Order
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.PENDING;

        public string? ShippingAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderTracking> Trackings { get; set; } = new List<OrderTracking>();
    }
}
