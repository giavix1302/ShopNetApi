using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class AdminOrderListItemDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public long UserId { get; set; }
        public string? UserEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}

