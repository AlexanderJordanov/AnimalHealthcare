using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Runtime.Intrinsics.Arm;

namespace AnimalHealthcare.Data.Configuration
{
    using static AnimalHealthcare.GCommon.ValidationConstants.Procedure;
    public class ProcedureConfiguration : IEntityTypeConfiguration<Procedure>
    {

        public void Configure(EntityTypeBuilder<Procedure> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(NameMaxLength);

            builder.Property(p => p.Description)
                   .IsRequired()
                   .HasMaxLength(DescriptionMaxLength);

            builder.Property(p => p.IsDeleted)
                   .HasDefaultValue(false);

            // Relationship: Procedure -> DoctorProcedure (1-to-many)
            builder.HasMany(p => p.DoctorProcedures)
                   .WithOne(dp => dp.Procedure)
                   .HasForeignKey(dp => dp.ProcedureId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            builder.HasData(SeedProcedures());
        }

        private static IEnumerable<Procedure> SeedProcedures()
        {
            return new List<Procedure>
            {
                new Procedure { Id = 1, Name = "Vaccination", Description = "Routine animal vaccination" },
                new Procedure { Id = 2, Name = "Dental Cleaning", Description = "Professional dental cleaning for pets" },
                new Procedure { Id = 3, Name = "Spay/Neuter", Description = "Spaying or neutering surgery" },
                new Procedure { Id = 4, Name = "Microchipping", Description = "Microchip implantation for ID" },
                new Procedure { Id = 5, Name = "X-Ray", Description = "Standard diagnostic X-ray" },
                new Procedure { Id = 6, Name = "Blood Test", Description = "Routine blood analysis" },
                new Procedure { Id = 7, Name = "Ultrasound", Description = "Abdominal ultrasound exam" },
                new Procedure { Id = 8, Name = "Heartworm Test", Description = "Testing for heartworm disease" },
                new Procedure { Id = 9, Name = "Allergy Testing", Description = "Skin or blood allergy tests" },
                new Procedure { Id = 10, Name = "Orthopedic Surgery", Description = "Surgery for bone/joint issues" },
                new Procedure { Id = 11, Name = "Skin Biopsy", Description = "Collection and analysis of skin sample" },
                new Procedure { Id = 12, Name = "Eye Exam", Description = "Comprehensive eye examination" },
                new Procedure { Id = 13, Name = "Ear Cleaning", Description = "Professional ear cleaning" },
                new Procedure { Id = 14, Name = "Parasite Treatment", Description = "Treatment for internal/external parasites" },
                new Procedure { Id = 15, Name = "Wound Care", Description = "Treatment and bandaging of wounds" },
                new Procedure { Id = 16, Name = "Tumor Removal", Description = "Surgical tumor removal" },
                new Procedure { Id = 17, Name = "Behavioral Consultation", Description = "Consultation for behavioral issues" },
                new Procedure { Id = 18, Name = "Nail Trim", Description = "Trimming of nails/claws" },
                new Procedure { Id = 19, Name = "Grooming", Description = "Basic grooming service" },
                new Procedure { Id = 20, Name = "Emergency Care", Description = "Immediate emergency treatment" },
                new Procedure { Id = 21, Name = "Neurological Consultation",Description = "Specialized consultation for neurological conditions"}
            };
        }
    }
}
