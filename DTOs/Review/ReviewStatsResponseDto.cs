namespace ShopNetApi.DTOs.Review
{
    public class ReviewStatsResponseDto
    {
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public Dictionary<int, decimal> PercentageDistribution { get; set; } = new();
    }
}
