using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace AnimalHealthcare.Data.Configuration
{
    using static AnimalHealthcare.GCommon.ValidationConstants.Animal;
    public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
    {
        public void Configure(EntityTypeBuilder<Animal> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(NameMaxLength);

            builder.Property(a => a.Age)
                   .IsRequired();

            builder.Property(a => a.Species)
                   .IsRequired()
                   .HasMaxLength(SpeciesMaxLength);

            builder.Property(a => a.Breed)
                   .IsRequired()
                   .HasMaxLength(BreedMaxLength);

            builder.Property(a => a.Gender)
                    .HasConversion<string>()
                    .IsRequired();

            builder.Property(a => a.IsDeleted)
                   .HasDefaultValue(false);

            // Relationship: Animal -> Doctor (many animals to one doctor)
            builder.HasOne(a => a.Doctor)
                   .WithMany(d => d.Animals)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Animal -> UserProfile (many animals to one user)
            builder.HasOne(a => a.UserProfile)
                   .WithMany(up => up.Animals)
                   .HasForeignKey(a => a.UserProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Animal -> Appointments (one animal to many appointments)
            builder.HasMany(a => a.Appointments)
                   .WithOne(ap => ap.Animal)
                   .HasForeignKey(ap => ap.AnimalId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            builder.HasData(SeedAnimals());
        }

        private static IEnumerable<Animal> SeedAnimals()
        {
            return new List<Animal>
            {
                new Animal
                {
                    Id = 1,
                    Name = "Buddy",
                    Species = "Dog",
                    Breed = "Labrador Retriever",
                    Age = 4,
                    Gender = GCommon.Enums.AnimalGender.Male,
                    IsDeleted = false,
                    UserProfileId = "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2"
                },
                new Animal
                {
                    Id = 2,
                    Name = "Whiskers",
                    Species = "Cat",
                    Breed = "Siamese",
                    Age = 3,
                    Gender = GCommon.Enums.AnimalGender.Female,
                    IsDeleted = false,
                    UserProfileId = "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2"
                }
            };
        }
    }
}
