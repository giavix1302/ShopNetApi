using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.Order.Admin;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        [EnableRateLimiting("AdminReadPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AdminOrderQueryDto query)
        {
            var result = await _adminOrderService.GetOrdersAsync(query);
            return Ok(ApiResponse<AdminOrderListResponseDto>.Ok(
                "Lấy danh sách đơn hàng thành công", result));
        }

        [EnableRateLimiting("AdminReadPolicy")]
        [HttpGet("{orderId:long}")]
        public async Task<IActionResult> GetById(long orderId)
        {
            var result = await _adminOrderService.GetOrderByIdAsync(orderId);
            return Ok(ApiResponse<AdminOrderDetailDto>.Ok(
                "Lấy thông tin đơn hàng thành công", result));
        }

        [EnableRateLimiting("AdminOrderPolicy")]
        [HttpPut("{orderId:long}/status")]
        public async Task<IActionResult> UpdateStatus(long orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            await _adminOrderService.UpdateOrderStatusAsync(orderId, dto);
            return Ok(ApiResponse<object>.Ok(
                "Cập nhật trạng thái đơn hàng thành công", null));
        }

        [EnableRateLimiting("AdminOrderPolicy")]
        [HttpPut("{orderId:long}/payment")]
        public async Task<IActionResult> UpdatePayment(long orderId, [FromBody] UpdateOrderPaymentDto dto)
        {
            await _adminOrderService.UpdateOrderPaymentAsync(orderId, dto);
            return Ok(ApiResponse<object>.Ok(
                "Cập nhật thanh toán đơn hàng thành công", null));
        }

        [EnableRateLimiting("AdminOrderPolicy")]
        [HttpPost("{orderId:long}/tracking")]
        public async Task<IActionResult> AddTracking(long orderId, [FromBody] CreateOrderTrackingDto dto)
        {
            var trackingId = await _adminOrderService.AddTrackingAsync(orderId, dto);
            return Ok(ApiResponse<object>.Ok(
                "Thêm tracking thành công", new { id = trackingId }));
        }

        [EnableRateLimiting("AdminOrderPolicy")]
        [HttpPut("{orderId:long}/tracking/{trackingId:long}")]
        public async Task<IActionResult> UpdateTracking(long orderId, long trackingId, [FromBody] UpdateOrderTrackingDto dto)
        {
            await _adminOrderService.UpdateTrackingAsync(orderId, trackingId, dto);
            return Ok(ApiResponse<object>.Ok(
                "Cập nhật tracking thành công", null));
        }

        [EnableRateLimiting("AdminOrderPolicy")]
        [HttpDelete("{orderId:long}/tracking/{trackingId:long}")]
        public async Task<IActionResult> DeleteTracking(long orderId, long trackingId)
        {
            await _adminOrderService.DeleteTrackingAsync(orderId, trackingId);
            return Ok(ApiResponse<object>.Ok(
                "Xóa tracking thành công", null));
        }

        [EnableRateLimiting("AdminReadPolicy")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _adminOrderService.GetStatsAsync(from, to);
            return Ok(ApiResponse<OrderStatsResponseDto>.Ok(
                "Lấy thống kê đơn hàng thành công", result));
        }
    }
}

