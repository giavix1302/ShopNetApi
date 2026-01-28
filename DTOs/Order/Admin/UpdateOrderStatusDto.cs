using ShopNetApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "status là bắt buộc")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "status không hợp lệ")]
        public OrderStatus Status { get; set; }
    }
}

