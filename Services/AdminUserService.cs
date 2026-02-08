using Microsoft.AspNetCore.Identity;
using ShopNetApi.DTOs.Order;
using ShopNetApi.DTOs.Review;
using ShopNetApi.DTOs.User.Admin;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminUserService> logger)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<AdminUserListResponseDto> GetUsersAsync(AdminUserQueryDto query)
        {
            var (users, total) = await _userRepo.GetAdminListAsync(query);

            var items = new List<AdminUserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var orderCount = await _userRepo.GetOrderCountByUserIdAsync(user.Id);
                var reviewCount = await _userRepo.GetReviewCountByUserIdAsync(user.Id);

                items.Add(new AdminUserListItemDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    AvatarUrl = user.AvatarUrl,
                    Enabled = user.Enabled,
                    CreatedAt = user.CreatedAt,
                    OrderCount = orderCount,
                    ReviewCount = reviewCount,
                    Roles = roles.ToList()
                });
            }

            var totalPages = (int)Math.Ceiling(total / (double)query.PageSize);
            return new AdminUserListResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = totalPages
            };
        }

        public async Task<AdminUserDetailDto> GetUserByIdAsync(long userId)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            if (user == null)
                throw new NotFoundException("Người dùng không tồn tại");

            var roles = await _userManager.GetRolesAsync(user);

            // Map orders
            var orders = user.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.Items?.Select(i => new OrderItemResponseDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product?.Name ?? "",
                        ProductSlug = i.Product?.Slug,
                        ColorId = i.ColorId,
                        ColorName = i.Color?.ColorName,
                        ColorHexCode = i.Color?.HexCode,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal
                    }).ToList() ?? new List<OrderItemResponseDto>(),
                    Trackings = o.Trackings?.OrderBy(t => t.CreatedAt).Select(t => new OrderTrackingResponseDto
                    {
                        Id = t.Id,
                        Status = t.Status,
                        Location = t.Location,
                        Description = t.Description,
                        Note = t.Note,
                        TrackingNumber = t.TrackingNumber,
                        ShippingPattern = t.ShippingPattern,
                        EstimatedDelivery = t.EstimatedDelivery,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToList() ?? new List<OrderTrackingResponseDto>()
                }).ToList();

            // Map reviews
            var reviews = user.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.FullName ?? r.User.Email ?? "",
                    UserAvatarUrl = r.User.AvatarUrl,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? "",
                    ProductSlug = r.Product?.Slug ?? "",
                    OrderItemId = r.OrderItemId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList();

            return new AdminUserDetailDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Note = user.Note,
                AvatarUrl = user.AvatarUrl,
                Enabled = user.Enabled,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                OrderCount = orders.Count,
                Orders = orders,
                ReviewCount = reviews.Count,
                Reviews = reviews
            };
        }
    }
}
