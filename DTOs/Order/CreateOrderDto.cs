using System.ComponentModel.DataAnnotations;
using ShopNetApi.Models;

namespace ShopNetApi.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "ShippingAddress là bắt buộc")]
        [MaxLength(500, ErrorMessage = "ShippingAddress không được vượt quá 500 ký tự")]
        public string ShippingAddress { get; set; } = null!;

        [Required(ErrorMessage = "PaymentMethod là bắt buộc")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "PaymentMethod không hợp lệ. Chỉ chấp nhận: COD, MOMO, BANK")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
