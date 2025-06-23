namespace AnimalHealthcare.Data.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;

        public int YearsOfExperience { get; set; }
        public string PhoneNumber { get; set; } = null!;

        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int AnimalClinicId { get; set; }

        // Navigation property for the clinic where the doctor works
        public AnimalClinic AnimalClinic { get; set; } = null!;

        // Navigation property for related animals
        public ICollection<Animal> Animals { get; set; } = new HashSet<Animal>();

        // Navigation property for related procedures
        public ICollection<DoctorProcedure> DoctorProcedures { get; set; } = new HashSet<DoctorProcedure>();

        // Navigation property for related appointments
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
