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
        }
    }
}
