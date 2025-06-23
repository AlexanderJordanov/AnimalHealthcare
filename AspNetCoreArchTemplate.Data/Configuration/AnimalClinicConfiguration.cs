using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalHealthcare.Data.Configuration
{
    using static AnimalHealthcare.GCommon.ValidationConstants.AnimalClinic;
    public class AnimalClinicConfiguration : IEntityTypeConfiguration<AnimalClinic>
    {
        public void Configure(EntityTypeBuilder<AnimalClinic> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(NameMaxLength);

            builder.Property(c => c.Address)
                   .IsRequired()
                   .HasMaxLength(AddressMaxLength);

            builder.Property(c => c.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(PhoneNumberMaxLength);

            builder.Property(c => c.ImageUrl)
                   .HasMaxLength(ImageUrlMaxLength);

            // Relationship: AnimalClinic -> Doctors (one clinic to many doctors)
            builder.HasMany(c => c.Doctors)
                   .WithOne(d => d.AnimalClinic)
                   .HasForeignKey(d => d.AnimalClinicId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
