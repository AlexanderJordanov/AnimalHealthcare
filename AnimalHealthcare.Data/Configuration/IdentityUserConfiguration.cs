using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace AnimalHealthcare.Data.Configuration
{
    public class IdentityUserConfiguration : IEntityTypeConfiguration<IdentityUser>
    {
        public void Configure(EntityTypeBuilder<IdentityUser> builder)
        {
            // Seed data for the admin user
            builder.HasData(SeedAdminUser());
        }
        private static IdentityUser SeedAdminUser()
        {
            return new IdentityUser
            {
                Id = "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2",
                UserName = "admin@animalhealthcare.com",
                NormalizedUserName = "ADMIN@ANIMALHEALTHCARE.COM",
                Email = "admin@animalhealthcare.com",
                NormalizedEmail = "ADMIN@ANIMALHEALTHCARE.COM",
                EmailConfirmed = true,
                SecurityStamp = "11111111-1111-1111-1111-111111111111",  // example constant
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222", // example constant
                PasswordHash = "AQAAAAIAAYagAAAAEBmKoup3TJhw47bvcqlUwPabiIwPFZOLI7qc46/vZm2L+gLbHeatMyc2TEcT+t/Hjw==" // hash for Admin123!
            };
        }
    }
}
