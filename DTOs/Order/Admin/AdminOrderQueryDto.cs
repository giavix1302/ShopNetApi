using ShopNetApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class AdminOrderQueryDto
    {
        public OrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public long? UserId { get; set; }
        public string? OrderNumber { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }

        // Sorting
        // Allowed: createdAt,totalAmount,status
        public string? SortBy { get; set; } = "createdAt";

        // asc|desc
        public string? SortDir { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "page phải >= 1")]
        public int Page { get; set; } = 1;

        [Range(1, 200, ErrorMessage = "pageSize phải từ 1 đến 200")]
        public int PageSize { get; set; } = 20;
    }
}

