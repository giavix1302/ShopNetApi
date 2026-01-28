using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order
{
    public class OrderListResponseDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }
}
