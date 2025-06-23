using AnimalHealthcare.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalHealthcare.Data.Configuration
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AppointmentDateTime)
                   .IsRequired();

            builder.Property(a => a.IsDeleted)
                   .HasDefaultValue(false);

            // Animal relationship (many appointments to one animal)
            builder.HasOne(a => a.Animal)
                   .WithMany(an => an.Appointments)
                   .HasForeignKey(a => a.AnimalId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Doctor relationship (many appointments to one doctor)
            builder.HasOne(a => a.Doctor)
                   .WithMany(d => d.Appointments)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Procedure relationship (many appointments to one procedure)
            builder.HasOne(a => a.Procedure)
                   .WithMany()
                   .HasForeignKey(a => a.ProcedureId)
                   .OnDelete(DeleteBehavior.Restrict);

            // UserProfile relationship (many appointments to one user profile)
            builder.HasOne(a => a.UserProfile)
                   .WithMany(up => up.Appointments)
                   .HasForeignKey(a => a.UserProfileId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
