namespace ShopNetApi.DTOs.Auth
{
    public class UserResponseDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
