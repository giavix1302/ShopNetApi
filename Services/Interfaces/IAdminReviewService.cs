using ShopNetApi.DTOs.Review.Admin;

namespace ShopNetApi.Services.Interfaces
{
    public interface IAdminReviewService
    {
        Task<AdminReviewListResponseDto> GetReviewsAsync(AdminReviewQueryDto query);
        Task<AdminReviewDetailDto> GetReviewByIdAsync(long reviewId);
        Task DeleteReviewAsync(long reviewId);
        Task<AdminReviewStatsResponseDto> GetStatsAsync(DateTime? from, DateTime? to);
    }
}
