namespace ShopNetApi.DTOs.Review
{
    public class ReviewListPaginatedResponseDto
    {
        public List<ReviewListResponseDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
