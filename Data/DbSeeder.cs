using Microsoft.AspNetCore.Identity;

namespace ShopNetApi.Data
{
    public class DbSeeder
    {
        public static async Task SeedRoles(IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole<long>>>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<long>(role)
                    );
                }
            }
        }
    }
}
