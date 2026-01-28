namespace ShopNetApi.DTOs.Review.Admin
{
    public class AdminReviewListResponseDto
    {
        public List<AdminReviewListItemDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
