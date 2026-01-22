using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNetApi.DTOs.Cart;
using ShopNetApi.DTOs.Common;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Controllers
{
    [ApiController]
    [Route("api/carts")]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCart()
        {
            var result = await _cartService.GetMyCartAsync();
            return Ok(ApiResponse<CartResponseDto>.Ok(
                "Lấy giỏ hàng thành công", result));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            var result = await _cartService.AddItemAsync(dto);
            return Ok(ApiResponse<CartItemResponseDto>.Ok(
                "Thêm sản phẩm vào giỏ hàng thành công", result));
        }

        [HttpPut("items/{itemId:long}")]
        public async Task<IActionResult> UpdateItem(long itemId, [FromBody] UpdateCartItemDto dto)
        {
            var result = await _cartService.UpdateItemAsync(itemId, dto);
            return Ok(ApiResponse<CartItemResponseDto>.Ok(
                "Cập nhật sản phẩm trong giỏ hàng thành công", result));
        }

        [HttpDelete("items/{itemId:long}")]
        public async Task<IActionResult> DeleteItem(long itemId)
        {
            await _cartService.DeleteItemAsync(itemId);
            return Ok(ApiResponse<object>.Ok(
                "Xóa sản phẩm khỏi giỏ hàng thành công", null));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync();
            return Ok(ApiResponse<object>.Ok(
                "Xóa toàn bộ giỏ hàng thành công", null));
        }
    }
}
