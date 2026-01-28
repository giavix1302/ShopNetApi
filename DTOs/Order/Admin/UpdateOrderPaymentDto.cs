using ShopNetApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Order.Admin
{
    public class UpdateOrderPaymentDto
    {
        // Optional: allow updating method later (e.g. after switching gateway)
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "paymentMethod không hợp lệ. Chỉ chấp nhận: COD, MOMO, BANK")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Required(ErrorMessage = "paymentStatus là bắt buộc")]
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "paymentStatus không hợp lệ")]
        public PaymentStatus PaymentStatus { get; set; }
    }
}

