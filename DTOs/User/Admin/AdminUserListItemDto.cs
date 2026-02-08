namespace ShopNetApi.DTOs.User.Admin
{
    public class AdminUserListItemDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderCount { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
