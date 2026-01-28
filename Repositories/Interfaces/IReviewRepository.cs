using ShopNetApi.DTOs.Review;
using ShopNetApi.DTOs.Review.Admin;
using ShopNetApi.Models;

namespace ShopNetApi.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(long id);
        Task<Review?> GetByIdWithFullDetailsAsync(long id);
        Task<Review?> GetByUserIdAndProductIdAsync(long userId, long productId);
        Task<List<Review>> GetByUserIdAsync(long userId);
        Task<(List<Review> Items, int TotalItems)> GetByProductIdAsync(long productId, ReviewQueryDto query);
        Task<(List<Review> Items, int TotalItems)> GetByUserIdPaginatedAsync(long userId, ReviewQueryDto query);
        Task<ReviewStats> GetProductStatsAsync(long productId);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Review review);

        // ========= ADMIN QUERIES =========
        Task<(List<Review> Items, int TotalItems)> GetAdminListAsync(AdminReviewQueryDto query);
        Task<AdminReviewStats> GetAdminStatsAsync(DateTime? from, DateTime? to);
    }

    public class ReviewStats
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    public class AdminReviewStats
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int ReviewsToday { get; set; }
        public int ReviewsThisWeek { get; set; }
        public int ReviewsThisMonth { get; set; }
    }
}
