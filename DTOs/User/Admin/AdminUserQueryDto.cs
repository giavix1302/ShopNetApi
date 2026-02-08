using System.ComponentModel.DataAnnotations;

namespace ShopNetApi.DTOs.User.Admin
{
    public class AdminUserQueryDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool? Enabled { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // Sorting
        // Allowed: createdAt, email, fullName
        public string? SortBy { get; set; } = "createdAt";

        // asc|desc
        public string? SortDir { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "page phải >= 1")]
        public int Page { get; set; } = 1;

        [Range(1, 200, ErrorMessage = "pageSize phải từ 1 đến 200")]
        public int PageSize { get; set; } = 20;
    }
}
