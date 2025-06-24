using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalHealthcare.Data.Configuration
{
    public class DoctorProcedureConfiguration : IEntityTypeConfiguration<DoctorProcedure>
    {
        public void Configure(EntityTypeBuilder<DoctorProcedure> builder)
        {
            // Composite primary key
            builder.HasKey(dp => new { dp.DoctorId, dp.ProcedureId });

            // Relationship: DoctorProcedure -> Doctor (many-to-one)
            builder.HasOne(dp => dp.Doctor)
                   .WithMany(d => d.DoctorProcedures)
                   .HasForeignKey(dp => dp.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship: DoctorProcedure -> Procedure (many-to-one)
            builder.HasOne(dp => dp.Procedure)
                   .WithMany(p => p.DoctorProcedures)
                   .HasForeignKey(dp => dp.ProcedureId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Seed data for DoctorProcedure
            builder.HasData(SeedDoctorProcedures());
        }

        private static IEnumerable<DoctorProcedure> SeedDoctorProcedures()
        {
            return new List<DoctorProcedure>
            {
                // Surgery
                new DoctorProcedure { DoctorId = 1, ProcedureId = 3 },  // Spay/Neuter
                new DoctorProcedure { DoctorId = 1, ProcedureId = 10 }, // Orthopedic Surgery
                new DoctorProcedure { DoctorId = 1, ProcedureId = 16 }, // Tumor Removal

                // Dentistry
                new DoctorProcedure { DoctorId = 2, ProcedureId = 2 },  // Dental Cleaning

                // Dermatology
                new DoctorProcedure { DoctorId = 3, ProcedureId = 9 },  // Allergy Testing
                new DoctorProcedure { DoctorId = 3, ProcedureId = 11 }, // Skin Biopsy

                // Radiology
                new DoctorProcedure { DoctorId = 4, ProcedureId = 5 },  // X-Ray
                new DoctorProcedure { DoctorId = 4, ProcedureId = 7 },  // Ultrasound

                // Cardiology
                new DoctorProcedure { DoctorId = 5, ProcedureId = 8 },  // Heartworm Test
                new DoctorProcedure { DoctorId = 5, ProcedureId = 6 },  // Blood Test

                // Neurology
                new DoctorProcedure { DoctorId = 6, ProcedureId = 21 }, // Neurological Consultation

                // Ophthalmology
                new DoctorProcedure { DoctorId = 7, ProcedureId = 12 }, // Eye Exam

                // Orthopedics
                new DoctorProcedure { DoctorId = 8, ProcedureId = 10 }, // Orthopedic Surgery

                // Oncology
                new DoctorProcedure { DoctorId = 9, ProcedureId = 16 }, // Tumor Removal

                // Endocrinology
                new DoctorProcedure { DoctorId = 10, ProcedureId = 6 }, // Blood Test

                // Emergency Medicine
                new DoctorProcedure { DoctorId = 11, ProcedureId = 20 }, // Emergency Care
                new DoctorProcedure { DoctorId = 11, ProcedureId = 15 }, // Wound Care

                // General Practice
                new DoctorProcedure { DoctorId = 12, ProcedureId = 1 },  // Vaccination
                new DoctorProcedure { DoctorId = 12, ProcedureId = 4 },  // Microchipping
                new DoctorProcedure { DoctorId = 12, ProcedureId = 13 }, // Ear Cleaning
                new DoctorProcedure { DoctorId = 12, ProcedureId = 14 }, // Parasite Treatment

                // Pediatrics
                new DoctorProcedure { DoctorId = 13, ProcedureId = 1 },  // Vaccination
                new DoctorProcedure { DoctorId = 13, ProcedureId = 18 }, // Nail Trim

                // Geriatrics
                new DoctorProcedure { DoctorId = 14, ProcedureId = 19 }, // Grooming
                new DoctorProcedure { DoctorId = 14, ProcedureId = 17 }  // Behavioral Consultation
            };
        }
    }
}
