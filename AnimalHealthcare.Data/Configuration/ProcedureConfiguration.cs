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
        }
    }
}
