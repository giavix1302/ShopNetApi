using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Review.Admin
{
    public class AdminReviewQueryDto
    {
        public long? ProductId { get; set; }
        public long? UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int? Rating { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "createdAt"; // "createdAt" | "rating" | "updatedAt"

        public string? SortDir { get; set; } = "desc"; // "asc" | "desc"

        [Range(1, int.MaxValue, ErrorMessage = "page phải >= 1")]
        public int Page { get; set; } = 1;

        [Range(1, 200, ErrorMessage = "pageSize phải từ 1 đến 200")]
        public int PageSize { get; set; } = 20;
    }
}
