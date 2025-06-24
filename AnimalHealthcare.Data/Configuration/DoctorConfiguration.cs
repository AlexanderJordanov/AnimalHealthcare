using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHealthcare.Data.Configuration
{
    using static AnimalHealthcare.GCommon.ValidationConstants.Doctor;
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(NameMaxLength);

            builder.Property(d => d.Specialization)
                   .IsRequired()
                   .HasMaxLength(SpecializationMaxLength);

            builder.Property(d => d.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(PhoneNumberMaxLength);

            builder.Property(d => d.ImageUrl)
                   .HasMaxLength(ImageUrlMaxLength);

            builder.Property(d => d.IsDeleted)
                   .HasDefaultValue(false);

            // Relationship with AnimalClinic (many doctors to one clinic)
            builder.HasOne(d => d.AnimalClinic)
                   .WithMany(c => c.Doctors)
                   .HasForeignKey(d => d.AnimalClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship with Animals (one doctor to many animals)
            builder.HasMany(d => d.Animals)
                   .WithOne(a => a.Doctor)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship with DoctorProcedures (one doctor to many doctorprocedures)
            builder.HasMany(d => d.DoctorProcedures)
                   .WithOne(dp => dp.Doctor)
                   .HasForeignKey(dp => dp.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Appointments (one doctor to many appointments)
            builder.HasMany(d => d.Appointments)
                   .WithOne(a => a.Doctor)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            builder.HasData(SeedDoctors());
        }

        private static IEnumerable<Doctor> SeedDoctors()
        {
            return new List<Doctor>
    {
        // Clinic 1: 4 doctors
        new Doctor { Id = 1, Name = "Dr. Sarah Smith", Specialization = "Surgery", YearsOfExperience = 10, PhoneNumber = "555-1001", ImageUrl = "/images/doctors/sarah-smith.jpg", AnimalClinicId = 1 },
        new Doctor { Id = 2, Name = "Dr. James Brown", Specialization = "Dentistry", YearsOfExperience = 8, PhoneNumber = "555-1002", ImageUrl = "/images/doctors/james-brown.jpg", AnimalClinicId = 1 },
        new Doctor { Id = 3, Name = "Dr. Laura Green", Specialization = "Dermatology", YearsOfExperience = 6, PhoneNumber = "555-1003", ImageUrl = "/images/doctors/laura-green.jpg", AnimalClinicId = 1 },
        new Doctor { Id = 4, Name = "Dr. Mark White", Specialization = "Radiology", YearsOfExperience = 9, PhoneNumber = "555-1004", ImageUrl = "/images/doctors/mark-white.jpg", AnimalClinicId = 1 },

        // Clinic 2: 3 doctors
        new Doctor { Id = 5, Name = "Dr. Olivia Wilson", Specialization = "Cardiology", YearsOfExperience = 12, PhoneNumber = "555-1005", ImageUrl = "/images/doctors/olivia-wilson.jpg", AnimalClinicId = 2 },
        new Doctor { Id = 6, Name = "Dr. Daniel Martinez", Specialization = "Neurology", YearsOfExperience = 7, PhoneNumber = "555-1006", ImageUrl = "/images/doctors/daniel-martinez.jpg", AnimalClinicId = 2 },
        new Doctor { Id = 7, Name = "Dr. Emma Clark", Specialization = "Ophthalmology", YearsOfExperience = 5, PhoneNumber = "555-1007", ImageUrl = "/images/doctors/emma-clark.jpg", AnimalClinicId = 2 },

        // Clinic 3: 3 doctors
        new Doctor { Id = 8, Name = "Dr. Michael Lee", Specialization = "Orthopedics", YearsOfExperience = 11, PhoneNumber = "555-1008", ImageUrl = "/images/doctors/michael-lee.jpg", AnimalClinicId = 3 },
        new Doctor { Id = 9, Name = "Dr. Sophia Garcia", Specialization = "Oncology", YearsOfExperience = 5, PhoneNumber = "555-1009", ImageUrl = "/images/doctors/sophia-garcia.jpg", AnimalClinicId = 3 },
        new Doctor { Id = 10, Name = "Dr. Liam King", Specialization = "Endocrinology", YearsOfExperience = 4, PhoneNumber = "555-1010", ImageUrl = "/images/doctors/liam-king.jpg", AnimalClinicId = 3 },

        // Clinic 4: 2 doctors
        new Doctor { Id = 11, Name = "Dr. William Johnson", Specialization = "Emergency Medicine", YearsOfExperience = 13, PhoneNumber = "555-1011", ImageUrl = "/images/doctors/william-johnson.jpg", AnimalClinicId = 4 },
        new Doctor { Id = 12, Name = "Dr. Mia Hernandez", Specialization = "General Practice", YearsOfExperience = 4, PhoneNumber = "555-1012", ImageUrl = "/images/doctors/mia-hernandez.jpg", AnimalClinicId = 4 },

        // Clinic 5: 2 doctors
        new Doctor { Id = 13, Name = "Dr. Ethan Robinson", Specialization = "Pediatrics", YearsOfExperience = 8, PhoneNumber = "555-1013", ImageUrl = "/images/doctors/ethan-robinson.jpg", AnimalClinicId = 5 },
        new Doctor { Id = 14, Name = "Dr. Isabella Lopez", Specialization = "Geriatrics", YearsOfExperience = 6, PhoneNumber = "555-1014", ImageUrl = "/images/doctors/isabella-lopez.jpg", AnimalClinicId = 5 },
    };
        }
    }
}
