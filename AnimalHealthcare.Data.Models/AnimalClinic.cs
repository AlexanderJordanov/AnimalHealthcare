namespace AnimalHealthcare.Data.Models
{
    public class AnimalClinic
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? ImageUrl { get; set; }

        // Navigation property - Doctors working at this clinic
        public ICollection<Doctor> Doctors { get; set; } = new HashSet<Doctor>();
    }
}
