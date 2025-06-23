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
        }
    }
}
