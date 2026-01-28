using ShopNetApi.DTOs.Review;

namespace ShopNetApi.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto);
        Task<ReviewListPaginatedResponseDto> GetMyReviewsAsync(ReviewQueryDto? query);
        Task<ReviewResponseDto> GetMyReviewByIdAsync(long reviewId);
        Task<ReviewResponseDto> UpdateAsync(long reviewId, UpdateReviewDto dto);
        Task DeleteAsync(long reviewId);
        Task<ReviewListPaginatedResponseDto> GetProductReviewsAsync(long productId, ReviewQueryDto query);
        Task<ReviewStatsResponseDto> GetProductReviewStatsAsync(long productId);
    }
}
