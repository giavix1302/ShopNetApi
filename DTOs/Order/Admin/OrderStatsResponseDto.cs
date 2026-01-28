using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class OrderStatsResponseDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public Dictionary<OrderStatus, int> CountByStatus { get; set; } = new();
    }
}

