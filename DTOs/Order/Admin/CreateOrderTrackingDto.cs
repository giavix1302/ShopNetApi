using ShopNetApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class CreateOrderTrackingDto
    {
        [Required(ErrorMessage = "status là bắt buộc")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "status không hợp lệ")]
        public OrderStatus Status { get; set; }

        [MaxLength(255, ErrorMessage = "location không được vượt quá 255 ký tự")]
        public string? Location { get; set; }

        [MaxLength(1000, ErrorMessage = "description không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [MaxLength(1000, ErrorMessage = "note không được vượt quá 1000 ký tự")]
        public string? Note { get; set; }

        [MaxLength(100, ErrorMessage = "trackingNumber không được vượt quá 100 ký tự")]
        public string? TrackingNumber { get; set; }

        [MaxLength(100, ErrorMessage = "shippingPattern không được vượt quá 100 ký tự")]
        public string? ShippingPattern { get; set; }

        public DateTime? EstimatedDelivery { get; set; }
    }
}

