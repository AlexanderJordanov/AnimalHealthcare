using Microsoft.AspNetCore.Identity;

namespace AnimalHealthcare.Data.Models
{
    public class UserProfile
    {
        public string Id { get; set; } = null!;

        // Navigation property for the user associated with this profile
        public IdentityUser User { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? ProfilePictureUrl { get; set; }

        // Navigation properties for related entities
        public ICollection<Animal> Animals { get; set; } = new HashSet<Animal>();

        // Navigation property for related appointments
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
