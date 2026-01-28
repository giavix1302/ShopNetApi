using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Review;
using ShopNetApi.Exceptions;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ReviewService> _logger;
        private readonly ApplicationDbContext _db;

        public ReviewService(
            IReviewRepository reviewRepo,
            IProductRepository productRepo,
            ICurrentUserService currentUser,
            ILogger<ReviewService> logger,
            ApplicationDbContext db)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
            _currentUser = currentUser;
            _logger = logger;
            _db = db;
        }

        public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            // Validate product exists and is active
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại");
            if (!product.IsActive)
                throw new BadRequestException("Sản phẩm không còn hoạt động");

            // Check if user already reviewed this product
            var existingReview = await _reviewRepo.GetByUserIdAndProductIdAsync(userId.Value, dto.ProductId);
            if (existingReview != null)
                throw new ConflictException("Bạn đã review sản phẩm này rồi");

            OrderItem? orderItem = null;
            if (dto.OrderItemId.HasValue)
            {
                // Validate OrderItem exists and belongs to user
                orderItem = await _db.OrderItems
                    .Include(oi => oi.Order)
                    .FirstOrDefaultAsync(oi => oi.Id == dto.OrderItemId.Value);

                if (orderItem == null)
                    throw new NotFoundException("OrderItem không tồn tại");

                if (orderItem.Order.UserId != userId.Value)
                    throw new ForbiddenException("OrderItem không thuộc về bạn");

                if (orderItem.Order.Status != OrderStatus.DELIVERED)
                    throw new BadRequestException("Chỉ có thể review sản phẩm từ đơn hàng đã được giao");

                if (orderItem.IsReviewed)
                    throw new BadRequestException("Sản phẩm này đã được review rồi");

                if (orderItem.ProductId != dto.ProductId)
                    throw new BadRequestException("OrderItem không khớp với ProductId");
            }
            else
            {
                // If OrderItemId not provided, check if user purchased this product
                var hasPurchased = await _db.OrderItems
                    .Include(oi => oi.Order)
                    .AnyAsync(oi => oi.Order.UserId == userId.Value
                        && oi.ProductId == dto.ProductId
                        && oi.Order.Status == OrderStatus.DELIVERED);

                if (!hasPurchased)
                    throw new BadRequestException("Bạn chỉ có thể review sản phẩm đã mua");
            }

            // Use transaction to ensure atomicity
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create review
                var review = new Review
                {
                    UserId = userId.Value,
                    ProductId = dto.ProductId,
                    OrderItemId = dto.OrderItemId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _reviewRepo.AddAsync(review);

                // Update OrderItem.IsReviewed if OrderItemId provided
                if (orderItem != null)
                {
                    orderItem.IsReviewed = true;
                    _db.OrderItems.Update(orderItem);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Get created review with full details
                var createdReview = await _reviewRepo.GetByIdWithFullDetailsAsync(review.Id);
                if (createdReview == null)
                    throw new InternalServerException("Không thể lấy thông tin review vừa tạo");

                return MapToResponse(createdReview);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating review. UserId={UserId}, ProductId={ProductId}", userId.Value, dto.ProductId);
                throw;
            }
        }

        public async Task<ReviewListPaginatedResponseDto> GetMyReviewsAsync(ReviewQueryDto? query)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            query ??= new ReviewQueryDto();

            var (reviews, totalItems) = await _reviewRepo.GetByUserIdPaginatedAsync(userId.Value, query);
            var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

            var items = reviews.Select(MapToListResponse).ToList();

            return new ReviewListPaginatedResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<ReviewResponseDto> GetMyReviewByIdAsync(long reviewId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var review = await _reviewRepo.GetByIdWithFullDetailsAsync(reviewId);
            if (review == null)
                throw new NotFoundException("Review không tồn tại");

            if (review.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền xem review này");

            return MapToResponse(review);
        }

        public async Task<ReviewResponseDto> UpdateAsync(long reviewId, UpdateReviewDto dto)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null)
                throw new NotFoundException("Review không tồn tại");

            if (review.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền cập nhật review này");

            // Update fields
            if (dto.Rating.HasValue)
                review.Rating = dto.Rating.Value;

            if (dto.Comment != null)
                review.Comment = dto.Comment;

            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepo.UpdateAsync(review);

            // Get updated review with full details
            var updatedReview = await _reviewRepo.GetByIdWithFullDetailsAsync(reviewId);
            if (updatedReview == null)
                throw new InternalServerException("Không thể lấy thông tin review vừa cập nhật");

            return MapToResponse(updatedReview);
        }

        public async Task DeleteAsync(long reviewId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("User chưa đăng nhập");

            var review = await _reviewRepo.GetByIdWithFullDetailsAsync(reviewId);
            if (review == null)
                throw new NotFoundException("Review không tồn tại");

            if (review.UserId != userId.Value)
                throw new ForbiddenException("Bạn không có quyền xóa review này");

            // Use transaction to ensure atomicity
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Delete review
                await _reviewRepo.DeleteAsync(review);

                // Update OrderItem.IsReviewed if OrderItemId exists
                if (review.OrderItemId.HasValue)
                {
                    var orderItem = await _db.OrderItems.FindAsync(review.OrderItemId.Value);
                    if (orderItem != null)
                    {
                        orderItem.IsReviewed = false;
                        _db.OrderItems.Update(orderItem);
                        await _db.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting review. ReviewId={ReviewId}, UserId={UserId}", reviewId, userId.Value);
                throw;
            }
        }

        public async Task<ReviewListPaginatedResponseDto> GetProductReviewsAsync(long productId, ReviewQueryDto query)
        {
            // Validate product exists
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại");

            var (reviews, totalItems) = await _reviewRepo.GetByProductIdAsync(productId, query);
            var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

            var items = reviews.Select(MapToListResponse).ToList();

            return new ReviewListPaginatedResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<ReviewStatsResponseDto> GetProductReviewStatsAsync(long productId)
        {
            // Validate product exists
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Sản phẩm không tồn tại");

            var stats = await _reviewRepo.GetProductStatsAsync(productId);

            // Calculate percentage distribution
            var percentageDistribution = new Dictionary<int, decimal>();
            if (stats.TotalReviews > 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var count = stats.RatingDistribution.ContainsKey(i) ? stats.RatingDistribution[i] : 0;
                    var percentage = Math.Round((decimal)count / stats.TotalReviews * 100, 2);
                    percentageDistribution[i] = percentage;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    percentageDistribution[i] = 0;
                }
            }

            return new ReviewStatsResponseDto
            {
                TotalReviews = stats.TotalReviews,
                AverageRating = stats.AverageRating,
                RatingDistribution = stats.RatingDistribution,
                PercentageDistribution = percentageDistribution
            };
        }

        private ReviewResponseDto MapToResponse(Review review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            if (review.User == null)
                throw new InvalidOperationException($"Review {review.Id} has null User. Please ensure User is included in query.");

            if (review.Product == null)
                throw new InvalidOperationException($"Review {review.Id} has null Product. Please ensure Product is included in query.");

            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User.FullName ?? review.User.Email ?? "Unknown",
                UserAvatarUrl = review.User.AvatarUrl,
                ProductId = review.ProductId,
                ProductName = review.Product.Name,
                ProductSlug = review.Product.Slug,
                OrderItemId = review.OrderItemId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        private ReviewListResponseDto MapToListResponse(Review review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            if (review.User == null)
                throw new InvalidOperationException($"Review {review.Id} has null User. Please ensure User is included in query.");

            if (review.Product == null)
                throw new InvalidOperationException($"Review {review.Id} has null Product. Please ensure Product is included in query.");

            return new ReviewListResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User.FullName ?? review.User.Email ?? "Unknown",
                UserAvatarUrl = review.User.AvatarUrl,
                ProductId = review.ProductId,
                ProductName = review.Product.Name,
                ProductSlug = review.Product.Slug,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}
