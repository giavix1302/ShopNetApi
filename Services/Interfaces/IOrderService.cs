using ShopNetApi.DTOs.Order;

namespace ShopNetApi.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateFromCartAsync(CreateOrderDto dto);
        Task<List<OrderListResponseDto>> GetMyOrdersAsync();
        Task<OrderResponseDto> GetMyOrderByIdAsync(long orderId);
        Task CancelOrderAsync(long orderId);
        Task<List<OrderTrackingResponseDto>> GetOrderTrackingAsync(long orderId);
    }
}
