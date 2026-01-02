namespace ShopNetApi.Services.Interfaces
{
    public interface ICurrentUserService
    {
        long? UserId { get; }
        string? Email { get; }
        string? Name { get; }
    }
}
