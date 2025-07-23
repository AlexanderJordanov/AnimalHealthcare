using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalHealthcare.Web.Infrastructure.Extension
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    public static class ApplicationDbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { AdminRole, UserRole };
            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seeded admin           
            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin != null && !await userManager.IsInRoleAsync(admin, AdminRole))
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }

            // Assign all other users the "User" role if they don’t have any
            var users = userManager.Users.ToList();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    await userManager.AddToRoleAsync(user, UserRole);
                }
            }
        }
    }
}
