namespace ShopNetApi.DTOs.Review.Admin
{
    public class AdminReviewStatsResponseDto
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public Dictionary<int, decimal> PercentageDistribution { get; set; } = new();
        public int ReviewsToday { get; set; }
        public int ReviewsThisWeek { get; set; }
        public int ReviewsThisMonth { get; set; }
    }
}
