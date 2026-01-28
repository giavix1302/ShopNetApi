using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Order.Admin;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            return await _db.Orders
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Order?> GetByIdWithItemsAsync(long id)
        {
            return await _db.Orders
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Color)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Order?> GetByIdWithFullDetailsAsync(long id)
        {
            return await _db.Orders
                .Include(x => x.User)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Color)
                .Include(x => x.Trackings)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Order>> GetByUserIdAsync(long userId)
        {
            return await _db.Orders
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByUserIdWithItemsAsync(long userId)
        {
            return await _db.Orders
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Color)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByOrderNumberAsync(string orderNumber)
        {
            return await _db.Orders
                .AnyAsync(x => x.OrderNumber == orderNumber);
        }

        public async Task AddAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }

        // ========= ADMIN QUERIES =========

        public async Task<(List<Order> Items, int TotalItems)> GetAdminListAsync(AdminOrderQueryDto query)
        {
            var q = _db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Items)
                .AsQueryable();

            if (query.Status.HasValue)
                q = q.Where(o => o.Status == query.Status.Value);

            if (query.PaymentStatus.HasValue)
                q = q.Where(o => o.PaymentStatus == query.PaymentStatus.Value);

            if (query.PaymentMethod.HasValue)
                q = q.Where(o => o.PaymentMethod == query.PaymentMethod.Value);

            if (query.UserId.HasValue)
                q = q.Where(o => o.UserId == query.UserId.Value);

            if (!string.IsNullOrWhiteSpace(query.OrderNumber))
                q = q.Where(o => o.OrderNumber.ToLower().Contains(query.OrderNumber.Trim().ToLower()));

            if (query.From.HasValue)
                q = q.Where(o => o.CreatedAt >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(o => o.CreatedAt <= query.To.Value);

            if (query.MinTotal.HasValue)
                q = q.Where(o => o.TotalAmount >= query.MinTotal.Value);

            if (query.MaxTotal.HasValue)
                q = q.Where(o => o.TotalAmount <= query.MaxTotal.Value);

            // Sorting
            var sortBy = (query.SortBy ?? "createdAt").Trim().ToLower();
            var sortDir = (query.SortDir ?? "desc").Trim().ToLower();

            q = (sortBy, sortDir) switch
            {
                ("totalamount", "asc") => q.OrderBy(o => o.TotalAmount),
                ("totalamount", "desc") => q.OrderByDescending(o => o.TotalAmount),
                ("status", "asc") => q.OrderBy(o => o.Status),
                ("status", "desc") => q.OrderByDescending(o => o.Status),
                ("createdat", "asc") => q.OrderBy(o => o.CreatedAt),
                _ => q.OrderByDescending(o => o.CreatedAt)
            };

            var total = await q.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<OrderTracking?> GetTrackingByIdAsync(long trackingId)
        {
            return await _db.OrderTrackings
                .FirstOrDefaultAsync(t => t.Id == trackingId);
        }

        public async Task<List<OrderTracking>> GetTrackingsByOrderIdAsync(long orderId)
        {
            return await _db.OrderTrackings
                .AsNoTracking()
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<(int TotalOrders, decimal TotalRevenue, Dictionary<OrderStatus, int> CountByStatus)> GetStatsAsync(DateTime? from, DateTime? to)
        {
            var q = _db.Orders.AsNoTracking().AsQueryable();

            if (from.HasValue)
                q = q.Where(o => o.CreatedAt >= from.Value);

            if (to.HasValue)
                q = q.Where(o => o.CreatedAt <= to.Value);

            var totalOrders = await q.CountAsync();
            var totalRevenue = await q.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var grouped = await q
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var dict = grouped.ToDictionary(x => x.Status, x => x.Count);
            return (totalOrders, totalRevenue, dict);
        }
    }
}
