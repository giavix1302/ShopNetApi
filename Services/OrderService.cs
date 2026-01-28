using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Order;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<OrderService> _logger;
        private readonly ApplicationDbContext _db;

        public OrderService(
            IOrderRepository orderRepo,
            ICartRepository cartRepo,
            IProductRepository productRepo,
            ICurrentUserService currentUser,
            ILogger<OrderService> logger,
            ApplicationDbContext db)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _currentUser = currentUser;
            _logger = logger;
            _db = db;
        }

        public async Task<OrderResponseDto> CreateFromCartAsync(CreateOrderDto dto)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            // Get user's cart with items
            var cart = await _cartRepo.GetByUserIdWithItemsAsync(userId.Value);
            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new BadRequestException("Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi đặt hàng");

            // Validate all cart items and check stock
            var cartItems = cart.Items.ToList();
            var productsToUpdate = new List<Product>();

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepo.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                    throw new NotFoundException($"Sản phẩm với ID {cartItem.ProductId} không tồn tại");

                if (!product.IsActive)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không còn hoạt động");

                if (cartItem.Quantity > product.StockQuantity)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ số lượng. Còn {product.StockQuantity} sản phẩm");

                // Validate color if specified
                if (cartItem.ColorId.HasValue)
                {
                    var productWithColors = await _productRepo.GetByIdWithIncludesAsync(cartItem.ProductId);
                    if (productWithColors == null || !productWithColors.ProductColors.Any(pc => pc.ColorId == cartItem.ColorId.Value))
                        throw new BadRequestException($"Màu không thuộc sản phẩm '{product.Name}'");
                }

                // Prepare product for stock update
                product.StockQuantity -= cartItem.Quantity;
                productsToUpdate.Add(product);
            }

            // Generate unique OrderNumber
            string orderNumber;
            int retryCount = 0;
            do
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var random = new Random().Next(1000, 9999);
                orderNumber = $"ORD-{timestamp}-{random}";
                retryCount++;
            } while (await _orderRepo.ExistsByOrderNumberAsync(orderNumber) && retryCount < 10);

            if (retryCount >= 10)
                throw new InternalServerException("Không thể tạo mã đơn hàng. Vui lòng thử lại");

            // Calculate total amount
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.UnitPrice);

            // Use transaction to ensure atomicity
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create Order (add directly to context, don't save yet)
                var order = new Order
                {
                    UserId = userId.Value,
                    OrderNumber = orderNumber,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.PENDING,
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = PaymentStatus.PENDING,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync(); // Save to get Order.Id

                // Create OrderItems
                var orderItems = cartItems.Select(cartItem =>
                {
                    var unitPrice = cartItem.UnitPrice;
                    var subtotal = cartItem.Quantity * unitPrice;

                    return new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ColorId = cartItem.ColorId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = unitPrice,
                        Subtotal = subtotal,
                        IsReviewed = false
                    };
                }).ToList();

                _db.OrderItems.AddRange(orderItems);

                // Create initial OrderTracking
                var initialTracking = new OrderTracking
                {
                    OrderId = order.Id,
                    Status = OrderStatus.PENDING,
                    Description = "Đơn hàng đã được tạo",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.OrderTrackings.Add(initialTracking);

                // Update product stock
                foreach (var product in productsToUpdate)
                {
                    _db.Products.Update(product);
                }

                // Clear cart items (delete directly from context)
                var cartItemsToDelete = await _db.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();
                _db.CartItems.RemoveRange(cartItemsToDelete);

                // Save all changes
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Order created successfully. OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}",
                    order.Id, orderNumber, userId.Value);

                // Load order with full details for response
                var createdOrder = await _orderRepo.GetByIdWithFullDetailsAsync(order.Id);
                if (createdOrder == null)
                    throw new NotFoundException("Không thể tải thông tin đơn hàng sau khi tạo");

                return MapToResponse(createdOrder);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Error creating order. UserId={UserId}, OrderNumber={OrderNumber}",
                    userId.Value, orderNumber);
                throw;
            }
        }

        public async Task<List<OrderListResponseDto>> GetMyOrdersAsync()
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var orders = await _orderRepo.GetByUserIdWithItemsAsync(userId.Value);
            return orders.Select(order => new OrderListResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ItemCount = order.Items?.Sum(i => i.Quantity) ?? 0
            }).ToList();
        }

        public async Task<OrderResponseDto> GetMyOrderByIdAsync(long orderId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var order = await _orderRepo.GetByIdWithFullDetailsAsync(orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            if (order.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền xem đơn hàng này");

            return MapToResponse(order);
        }

        public async Task CancelOrderAsync(long orderId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var order = await _orderRepo.GetByIdWithItemsAsync(orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            if (order.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền hủy đơn hàng này");

            if (order.Status != OrderStatus.PENDING)
                throw new BadRequestException("Chỉ có thể hủy đơn hàng ở trạng thái PENDING");

            // Use transaction for cancellation
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Update order status (update directly in context)
                order.Status = OrderStatus.CANCELLED;
                order.UpdatedAt = DateTime.UtcNow;
                _db.Orders.Update(order);

                // Create cancellation tracking
                var cancellationTracking = new OrderTracking
                {
                    OrderId = order.Id,
                    Status = OrderStatus.CANCELLED,
                    Description = "Đơn hàng đã bị hủy bởi khách hàng",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.OrderTrackings.Add(cancellationTracking);

                // Restore product stock
                if (order.Items != null && order.Items.Any())
                {
                    foreach (var orderItem in order.Items)
                    {
                        var product = await _productRepo.GetByIdAsync(orderItem.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += orderItem.Quantity;
                            _db.Products.Update(product);
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Order cancelled. OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}",
                    order.Id, order.OrderNumber, userId.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Error cancelling order. OrderId={OrderId}, UserId={UserId}",
                    orderId, userId.Value);
                throw;
            }
        }

        public async Task<List<OrderTrackingResponseDto>> GetOrderTrackingAsync(long orderId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            if (order.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền xem tracking của đơn hàng này");

            var orderWithTrackings = await _orderRepo.GetByIdWithFullDetailsAsync(orderId);
            if (orderWithTrackings == null)
                throw new NotFoundException("Đơn hàng không tồn tại");

            return orderWithTrackings.Trackings
                .OrderBy(t => t.CreatedAt)
                .Select(MapTrackingToResponse)
                .ToList();
        }

        // ========= MAPPING METHODS =========

        private OrderResponseDto MapToResponse(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items?.Select(MapItemToResponse).ToList() ?? new List<OrderItemResponseDto>(),
                Trackings = order.Trackings?.Select(MapTrackingToResponse).ToList() ?? new List<OrderTrackingResponseDto>()
            };
        }

        private OrderItemResponseDto MapItemToResponse(OrderItem item)
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

        private OrderTrackingResponseDto MapTrackingToResponse(OrderTracking tracking)
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
