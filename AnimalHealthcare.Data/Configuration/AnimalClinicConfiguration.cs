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

            // Seed data
            builder.HasData(SeedClinics());
        }

        private static IEnumerable<AnimalClinic> SeedClinics()
        {
            return new List<AnimalClinic>
            {
                new AnimalClinic
                {
                    Id = 1,
                    Name = "Happy Paws Veterinary Clinic",
                    Address = "123 Main St, Springfield",
                    PhoneNumber = "555-1234",
                    ImageUrl = "/images/clinics/happy-paws.jpg"
                },
                new AnimalClinic
                {
                    Id = 2,
                    Name = "Healthy Tails Animal Hospital",
                    Address = "456 Elm St, Shelbyville",
                    PhoneNumber = "555-5678",
                    ImageUrl = "/images/clinics/healthy-tails-animal-hospital.jpg"
                },
                new AnimalClinic
                {
                    Id = 3,
                    Name = "Gentle Care Pet Clinic",
                    Address = "789 Oak Ave, Capital City",
                    PhoneNumber = "555-9012",
                    ImageUrl = "/images/clinics/gentle-care-pet-clinic.jpg"
                },
                new AnimalClinic
                {
                    Id = 4,
                    Name = "Purrfect Health Vet Center",
                    Address = "321 Maple Rd, Ogdenville",
                    PhoneNumber = "555-3456",
                    ImageUrl = "/images/clinics/purrfect-health-vet-center.jpg"
                },
                new AnimalClinic
                {
                    Id = 5,
                    Name = "Four-Legged Friends Vet",
                    Address = "654 Pine St, North Haverbrook",
                    PhoneNumber = "555-7890",
                    ImageUrl = "/images/clinics/four-legged-friends-vet.jpeg"
                }
            };
        }
    }
}
