using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Common;
using ShopNetApi.DTOs.Order;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var result = await _orderService.CreateFromCartAsync(dto);
            return Ok(ApiResponse<OrderResponseDto>.Ok(
                "Tạo đơn hàng thành công", result));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders()
        {
            var result = await _orderService.GetMyOrdersAsync();
            return Ok(ApiResponse<List<OrderListResponseDto>>.Ok(
                "Lấy danh sách đơn hàng thành công", result));
        }

        [HttpGet("me/{orderId:long}")]
        public async Task<IActionResult> GetMyOrderById(long orderId)
        {
            var result = await _orderService.GetMyOrderByIdAsync(orderId);
            return Ok(ApiResponse<OrderResponseDto>.Ok(
                "Lấy thông tin đơn hàng thành công", result));
        }

        [HttpPut("{orderId:long}/cancel")]
        public async Task<IActionResult> CancelOrder(long orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            return Ok(ApiResponse<object>.Ok(
                "Hủy đơn hàng thành công", null));
        }

        [HttpGet("{orderId:long}/tracking")]
        public async Task<IActionResult> GetOrderTracking(long orderId)
        {
            var result = await _orderService.GetOrderTrackingAsync(orderId);
            return Ok(ApiResponse<List<OrderTrackingResponseDto>>.Ok(
                "Lấy thông tin tracking thành công", result));
        }
    }
}
