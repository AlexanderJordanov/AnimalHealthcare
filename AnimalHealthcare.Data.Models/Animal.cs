using Microsoft.AspNetCore.Identity;

namespace AnimalHealthcare.Data.Models
{
    public class Animal
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Age { get; set; }

        public string Species { get; set; } = null!;

        public string Breed { get; set; } = null!;

        public string UserProfileId { get; set; } = null!;

        // The Animal's owner
        public UserProfile UserProfile { get; set; } = null!;

        public int DoctorId { get; set; }

        // The Animal's doctor
        public Doctor Doctor { get; set; } = null!;

        // The Animal's appointment
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        // Indicates if the animal is deleted (soft delete)
        public bool IsDeleted { get; set; }
    }
}
