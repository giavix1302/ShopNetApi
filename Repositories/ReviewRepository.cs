using Microsoft.EntityFrameworkCore;
using ShopNetApi.Data;
using ShopNetApi.DTOs.Review;
using ShopNetApi.DTOs.Review.Admin;
using ShopNetApi.Models;
using ShopNetApi.Repositories.Interfaces;

namespace ShopNetApi.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _db;

        public ReviewRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Review?> GetByIdAsync(long id)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review?> GetByIdWithFullDetailsAsync(long id)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.OrderItem)
                    .ThenInclude(oi => oi!.Order)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review?> GetByUserIdAndProductIdAsync(long userId, long productId)
        {
            return await _db.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<List<Review>> GetByUserIdAsync(long userId)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Review> Items, int TotalItems)> GetByProductIdAsync(long productId, ReviewQueryDto query)
        {
            var q = _db.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .AsQueryable();

            // Filter by rating
            if (query.Rating.HasValue)
            {
                q = q.Where(r => r.Rating == query.Rating.Value);
            }

            // Sorting
            var sortBy = (query.SortBy ?? "createdAt").Trim().ToLower();
            var sortDir = (query.SortDir ?? "desc").Trim().ToLower();

            q = (sortBy, sortDir) switch
            {
                ("rating", "asc") => q.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("rating", "desc") => q.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("createdat", "asc") => q.OrderBy(r => r.CreatedAt),
                _ => q.OrderByDescending(r => r.CreatedAt)
            };

            var total = await q.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<(List<Review> Items, int TotalItems)> GetByUserIdPaginatedAsync(long userId, ReviewQueryDto query)
        {
            var q = _db.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .AsQueryable();

            // Filter by rating
            if (query.Rating.HasValue)
            {
                q = q.Where(r => r.Rating == query.Rating.Value);
            }

            // Sorting
            var sortBy = (query.SortBy ?? "createdAt").Trim().ToLower();
            var sortDir = (query.SortDir ?? "desc").Trim().ToLower();

            q = (sortBy, sortDir) switch
            {
                ("rating", "asc") => q.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("rating", "desc") => q.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("createdat", "asc") => q.OrderBy(r => r.CreatedAt),
                _ => q.OrderByDescending(r => r.CreatedAt)
            };

            var total = await q.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<ReviewStats> GetProductStatsAsync(long productId)
        {
            var reviews = await _db.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            var totalReviews = reviews.Count;
            if (totalReviews == 0)
            {
                return new ReviewStats
                {
                    TotalReviews = 0,
                    AverageRating = 0,
                    RatingDistribution = new Dictionary<int, int>
                    {
                        { 1, 0 },
                        { 2, 0 },
                        { 3, 0 },
                        { 4, 0 },
                        { 5, 0 }
                    }
                };
            }

            var averageRating = (decimal)reviews.Average(r => r.Rating);
            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all ratings 1-5 are present
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            return new ReviewStats
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 2),
                RatingDistribution = ratingDistribution
            };
        }

        public async Task AddAsync(Review review)
        {
            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            _db.Reviews.Update(review);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Review review)
        {
            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
        }

        // ========= ADMIN QUERIES =========

        public async Task<(List<Review> Items, int TotalItems)> GetAdminListAsync(AdminReviewQueryDto query)
        {
            var q = _db.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            // Filter by productId
            if (query.ProductId.HasValue)
            {
                q = q.Where(r => r.ProductId == query.ProductId.Value);
            }

            // Filter by userId
            if (query.UserId.HasValue)
            {
                q = q.Where(r => r.UserId == query.UserId.Value);
            }

            // Filter by rating
            if (query.Rating.HasValue)
            {
                q = q.Where(r => r.Rating == query.Rating.Value);
            }

            // Filter by date range
            if (query.From.HasValue)
            {
                q = q.Where(r => r.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                q = q.Where(r => r.CreatedAt <= query.To.Value);
            }

            // Sorting
            var sortBy = (query.SortBy ?? "createdAt").Trim().ToLower();
            var sortDir = (query.SortDir ?? "desc").Trim().ToLower();

            q = (sortBy, sortDir) switch
            {
                ("rating", "asc") => q.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("rating", "desc") => q.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ("updatedat", "asc") => q.OrderBy(r => r.UpdatedAt),
                ("updatedat", "desc") => q.OrderByDescending(r => r.UpdatedAt),
                ("createdat", "asc") => q.OrderBy(r => r.CreatedAt),
                _ => q.OrderByDescending(r => r.CreatedAt)
            };

            var total = await q.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, total);
        }

        public async Task<AdminReviewStats> GetAdminStatsAsync(DateTime? from, DateTime? to)
        {
            var q = _db.Reviews.AsNoTracking().AsQueryable();

            if (from.HasValue)
            {
                q = q.Where(r => r.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                q = q.Where(r => r.CreatedAt <= to.Value);
            }

            var reviews = await q.ToListAsync();

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0 ? (decimal)reviews.Average(r => r.Rating) : 0m;
            var ratingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all ratings 1-5 are present
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            // Calculate reviews by period
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var reviewsToday = reviews.Count(r => r.CreatedAt >= todayStart);
            var reviewsThisWeek = reviews.Count(r => r.CreatedAt >= weekStart);
            var reviewsThisMonth = reviews.Count(r => r.CreatedAt >= monthStart);

            return new AdminReviewStats
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 2),
                RatingDistribution = ratingDistribution,
                ReviewsToday = reviewsToday,
                ReviewsThisWeek = reviewsThisWeek,
                ReviewsThisMonth = reviewsThisMonth
            };
        }
    }
}
