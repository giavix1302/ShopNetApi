using ShopNetApi.DTOs.Order;
using ShopNetApi.DTOs.Review;

namespace ShopNetApi.DTOs.User.Admin
{
    public class AdminUserDetailDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public string? AvatarUrl { get; set; }
        public bool Enabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        
        // Order statistics
        public int OrderCount { get; set; }
        public List<OrderResponseDto> Orders { get; set; } = new();
        
        // Review statistics
        public int ReviewCount { get; set; }
        public List<ReviewResponseDto> Reviews { get; set; } = new();
    }
}
