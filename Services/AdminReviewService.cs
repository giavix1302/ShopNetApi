using ShopNetApi.Data;
using ShopNetApi.DTOs.Review.Admin;
using ShopNetApi.Exceptions;
using ShopNetApi.Repositories.Interfaces;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdminReviewService> _logger;

        public AdminReviewService(
            IReviewRepository reviewRepo,
            ApplicationDbContext db,
            ILogger<AdminReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _db = db;
            _logger = logger;
        }

        public async Task<AdminReviewListResponseDto> GetReviewsAsync(AdminReviewQueryDto query)
        {
            var (reviews, total) = await _reviewRepo.GetAdminListAsync(query);

            var items = reviews.Select(r => new AdminReviewListItemDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserEmail = r.User.Email ?? "N/A",
                UserName = r.User.FullName,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                OrderItemId = r.OrderItemId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)query.PageSize);
            return new AdminReviewListResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = totalPages
            };
        }

        public async Task<AdminReviewDetailDto> GetReviewByIdAsync(long reviewId)
        {
            var review = await _reviewRepo.GetByIdWithFullDetailsAsync(reviewId);
            if (review == null)
                throw new NotFoundException("Review không tồn tại");

            return new AdminReviewDetailDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserEmail = review.User.Email ?? "N/A",
                UserName = review.User.FullName,
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

        public async Task DeleteReviewAsync(long reviewId)
        {
            var review = await _reviewRepo.GetByIdWithFullDetailsAsync(reviewId);
            if (review == null)
                throw new NotFoundException("Review không tồn tại");

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
                _logger.LogError(ex, "Error deleting review. ReviewId={ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<AdminReviewStatsResponseDto> GetStatsAsync(DateTime? from, DateTime? to)
        {
            var stats = await _reviewRepo.GetAdminStatsAsync(from, to);

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

            return new AdminReviewStatsResponseDto
            {
                TotalReviews = stats.TotalReviews,
                AverageRating = stats.AverageRating,
                RatingDistribution = stats.RatingDistribution,
                PercentageDistribution = percentageDistribution,
                ReviewsToday = stats.ReviewsToday,
                ReviewsThisWeek = stats.ReviewsThisWeek,
                ReviewsThisMonth = stats.ReviewsThisMonth
            };
        }
    }
}
