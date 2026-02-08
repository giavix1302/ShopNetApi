namespace ShopNetApi.DTOs.User.Admin
{
    public class AdminUserListResponseDto
    {
        public List<AdminUserListItemDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
