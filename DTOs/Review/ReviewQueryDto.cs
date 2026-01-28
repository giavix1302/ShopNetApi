using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.Review
{
    public class ReviewQueryDto
    {
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int? Rating { get; set; }

        public string? SortBy { get; set; } = "createdAt"; // "createdAt" | "rating"

        public string? SortDir { get; set; } = "desc"; // "asc" | "desc"

        [Range(1, int.MaxValue, ErrorMessage = "page phải >= 1")]
        public int Page { get; set; } = 1;

        [Range(1, 200, ErrorMessage = "pageSize phải từ 1 đến 200")]
        public int PageSize { get; set; } = 20;
    }
}
