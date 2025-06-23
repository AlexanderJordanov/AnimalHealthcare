namespace AnimalHealthcare.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using AnimalHealthcare.Data.Models;

    public class AnimalHealthcareDbContext : IdentityDbContext
    {
        public AnimalHealthcareDbContext(DbContextOptions<AnimalHealthcareDbContext> options)
            : base(options)
        {

        }

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<Animal> Animals { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<AnimalClinic> AnimalClinics { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Procedure> Procedures { get; set; } = null!;
        public DbSet<DoctorProcedure> DoctorProcedures { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnimalHealthcareDbContext).Assembly);
        }
    }
}
