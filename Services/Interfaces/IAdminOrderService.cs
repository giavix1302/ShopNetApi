using ShopNetApi.DTOs.Order.Admin;

namespace ShopNetApi.Services.Interfaces
{
    public interface IAdminOrderService
    {
        Task<AdminOrderListResponseDto> GetOrdersAsync(AdminOrderQueryDto query);
        Task<AdminOrderDetailDto> GetOrderByIdAsync(long orderId);
        Task UpdateOrderStatusAsync(long orderId, UpdateOrderStatusDto dto);
        Task UpdateOrderPaymentAsync(long orderId, UpdateOrderPaymentDto dto);

        Task<long> AddTrackingAsync(long orderId, CreateOrderTrackingDto dto);
        Task UpdateTrackingAsync(long orderId, long trackingId, UpdateOrderTrackingDto dto);
        Task DeleteTrackingAsync(long orderId, long trackingId);

        Task<OrderStatsResponseDto> GetStatsAsync(DateTime? from, DateTime? to);
    }
}

