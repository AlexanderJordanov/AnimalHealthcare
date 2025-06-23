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
        }
    }
}
