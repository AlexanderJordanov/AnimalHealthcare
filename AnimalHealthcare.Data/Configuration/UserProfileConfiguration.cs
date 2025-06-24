using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Tokens;

namespace AnimalHealthcare.Data.Configuration
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {        
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(up => up.Id);

            builder.Property(up => up.FullName)
                   .IsRequired()
                   .HasMaxLength(FullNameMaxLength);

            builder.Property(up => up.PhoneNumber)
                   .HasMaxLength(PhoneNumberMaxLength);

            builder.Property(up => up.Address)
                   .HasMaxLength(AddressMaxLength);

            builder.Property(up => up.ProfilePictureUrl)
                   .HasMaxLength(ProfilePictureUrlMaxLength);

            // Relationship with IdentityUser (one-to-one)
            builder.HasOne(up => up.User)
                   .WithOne()
                   .HasForeignKey<UserProfile>(up => up.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Animals (one UserProfile to many Animals)
            builder.HasMany(up => up.Animals)
                   .WithOne(a => a.UserProfile)
                   .HasForeignKey(a => a.UserProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship with Appointments (one UserProfile to many Appointments)
            builder.HasMany(up => up.Appointments)
                   .WithOne(ap => ap.UserProfile)
                   .HasForeignKey(ap => ap.UserProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed data for the admin user profile
            builder.HasData(GenerateAdminProfile());
        }

        private static UserProfile GenerateAdminProfile()
        {
            return new UserProfile
            {
                Id = "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2",
                FullName = "Admin User",
                PhoneNumber = "123-456-7890",
                Address = "123 Admin St, Admin City, Admin Country",
                ProfilePictureUrl = "/images/profiles/admin.jpg"
            };
        }
    }
}
