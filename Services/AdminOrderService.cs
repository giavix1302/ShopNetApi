using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Order;
using ShopNetApi.DTOs.Order.Admin;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(
            IOrderRepository orderRepo,
            ApplicationDbContext db,
            ILogger<AdminOrderService> logger)
        {
            _orderRepo = orderRepo;
            _db = db;
            _logger = logger;
        }

        public async Task<AdminOrderListResponseDto> GetOrdersAsync(AdminOrderQueryDto query)
        {
            var (orders, total) = await _orderRepo.GetAdminListAsync(query);

            var items = orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                UserEmail = o.User?.Email,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                ItemCount = o.Items?.Sum(i => i.Quantity) ?? 0
            }).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)query.PageSize);
            return new AdminOrderListResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = totalPages
            };
        }

        public async Task<AdminOrderDetailDto> GetOrderByIdAsync(long orderId)
        {
            var order = await _orderRepo.GetByIdWithFullDetailsAsync(orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            return new AdminOrderDetailDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserEmail = order.User?.Email,
                UserName = order.User?.FullName,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items?.Select(MapItemToResponse).ToList() ?? new List<OrderItemResponseDto>(),
                Trackings = order.Trackings?.OrderBy(t => t.CreatedAt).Select(MapTrackingToResponse).ToList() ?? new List<OrderTrackingResponseDto>()
            };
        }

        public async Task UpdateOrderStatusAsync(long orderId, UpdateOrderStatusDto dto)
        {
            if (dto.Status == OrderStatus.CANCELLED)
                throw new BadRequestException("Admin không được chuyển đơn hàng sang trạng thái CANCELLED");

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            if (!IsAllowedTransition(order.Status, dto.Status))
                throw new BadRequestException($"Không thể chuyển trạng thái từ {order.Status} sang {dto.Status}");

            if (order.Status == dto.Status)
                return;

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;
            _db.Orders.Update(order);

            _db.OrderTrackings.Add(new OrderTracking
            {
                OrderId = order.Id,
                Status = dto.Status,
                Description = $"Admin cập nhật trạng thái đơn hàng: {dto.Status}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin updated order status. OrderId={OrderId}, Status={Status}", orderId, dto.Status);
        }

        public async Task UpdateOrderPaymentAsync(long orderId, UpdateOrderPaymentDto dto)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            if (dto.PaymentMethod.HasValue)
                order.PaymentMethod = dto.PaymentMethod.Value;

            order.PaymentStatus = dto.PaymentStatus;
            order.UpdatedAt = DateTime.UtcNow;

            _db.Orders.Update(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Admin updated order payment. OrderId={OrderId}, Method={Method}, Status={PaymentStatus}",
                orderId, order.PaymentMethod, order.PaymentStatus);
        }

        public async Task<long> AddTrackingAsync(long orderId, CreateOrderTrackingDto dto)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            var tracking = new OrderTracking
            {
                OrderId = orderId,
                Status = dto.Status,
                Location = dto.Location,
                Description = dto.Description,
                Note = dto.Note,
                TrackingNumber = dto.TrackingNumber,
                ShippingPattern = dto.ShippingPattern,
                EstimatedDelivery = dto.EstimatedDelivery,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.OrderTrackings.Add(tracking);
            await _db.SaveChangesAsync();
            return tracking.Id;
        }

        public async Task UpdateTrackingAsync(long orderId, long trackingId, UpdateOrderTrackingDto dto)
        {
            var tracking = await _db.OrderTrackings.FirstOrDefaultAsync(t => t.Id == trackingId);
            if (tracking == null)
                throw new NotFoundException("Tracking không tồn tại");

            if (tracking.OrderId != orderId)
                throw new BadRequestException("Tracking không thuộc đơn hàng này");

            // Partial update: only update provided fields (null means \"not provided\")
            if (dto.Status.HasValue)
                tracking.Status = dto.Status.Value;

            if (dto.Location != null)
                tracking.Location = dto.Location;

            if (dto.Description != null)
                tracking.Description = dto.Description;

            if (dto.Note != null)
                tracking.Note = dto.Note;

            if (dto.TrackingNumber != null)
                tracking.TrackingNumber = dto.TrackingNumber;

            if (dto.ShippingPattern != null)
                tracking.ShippingPattern = dto.ShippingPattern;

            if (dto.EstimatedDelivery.HasValue)
                tracking.EstimatedDelivery = dto.EstimatedDelivery.Value;

            tracking.UpdatedAt = DateTime.UtcNow;

            _db.OrderTrackings.Update(tracking);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTrackingAsync(long orderId, long trackingId)
        {
            var tracking = await _db.OrderTrackings.FirstOrDefaultAsync(t => t.Id == trackingId);
            if (tracking == null)
                throw new NotFoundException("Tracking không tồn tại");

            if (tracking.OrderId != orderId)
                throw new BadRequestException("Tracking không thuộc đơn hàng này");

            _db.OrderTrackings.Remove(tracking);
            await _db.SaveChangesAsync();
        }

        public async Task<OrderStatsResponseDto> GetStatsAsync(DateTime? from, DateTime? to)
        {
            var (totalOrders, totalRevenue, countByStatus) = await _orderRepo.GetStatsAsync(from, to);

            // Ensure all statuses exist in the dict
            foreach (var status in Enum.GetValues<OrderStatus>())
            {
                if (!countByStatus.ContainsKey(status))
                    countByStatus[status] = 0;
            }

            return new OrderStatsResponseDto
            {
                From = from,
                To = to,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                CountByStatus = countByStatus
            };
        }

        private static bool IsAllowedTransition(OrderStatus from, OrderStatus to)
        {
            if (from == to) return true;

            return from switch
            {
                OrderStatus.PENDING => to == OrderStatus.PROCESSING,
                OrderStatus.PROCESSING => to == OrderStatus.SHIPPED,
                OrderStatus.SHIPPED => to == OrderStatus.DELIVERED,
                OrderStatus.DELIVERED => false,
                OrderStatus.CANCELLED => false,
                _ => false
            };
        }

        private static OrderItemResponseDto MapItemToResponse(OrderItem item)
        {
            return new OrderItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "",
                ProductSlug = item.Product?.Slug,
                ColorId = item.ColorId,
                ColorName = item.Color?.ColorName,
                ColorHexCode = item.Color?.HexCode,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal
            };
        }

        private static OrderTrackingResponseDto MapTrackingToResponse(OrderTracking tracking)
        {
            return new OrderTrackingResponseDto
            {
                Id = tracking.Id,
                Status = tracking.Status,
                Location = tracking.Location,
                Description = tracking.Description,
                Note = tracking.Note,
                TrackingNumber = tracking.TrackingNumber,
                ShippingPattern = tracking.ShippingPattern,
                EstimatedDelivery = tracking.EstimatedDelivery,
                CreatedAt = tracking.CreatedAt,
                UpdatedAt = tracking.UpdatedAt
            };
        }
    }
}

