using ShopNetApi.Models;
using ShopNetApi.DTOs.Order.Admin;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(long id);
        Task<Order?> GetByIdWithItemsAsync(long id);
        Task<Order?> GetByIdWithFullDetailsAsync(long id); // Includes Items, Trackings, User, Products
        Task<List<Order>> GetByUserIdAsync(long userId);
        Task<List<Order>> GetByUserIdWithItemsAsync(long userId);
        Task<bool> ExistsByOrderNumberAsync(string orderNumber);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);

        // ========= ADMIN QUERIES =========
        Task<(List<Order> Items, int TotalItems)> GetAdminListAsync(AdminOrderQueryDto query);
        Task<OrderTracking?> GetTrackingByIdAsync(long trackingId);
        Task<List<OrderTracking>> GetTrackingsByOrderIdAsync(long orderId);
        Task<(int TotalOrders, decimal TotalRevenue, Dictionary<OrderStatus, int> CountByStatus)> GetStatsAsync(DateTime? from, DateTime? to);
    }
}
