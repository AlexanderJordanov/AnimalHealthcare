namespace AnimalHealthcare.Data.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public int AnimalId { get; set; }
        
        // The Animal associated with the appointment
        public Animal Animal { get; set; } = null!;

        public int DoctorId { get; set; }
        
        // The Doctor associated with the appointment
        public Doctor Doctor { get; set; } = null!;

        public int ProcedureId { get; set; }

        // The Procedure associated with the appointment
        public Procedure Procedure { get; set; } = null!;

        public string UserProfileId { get; set; } = null!;

        // The User who created the appointment
        public UserProfile UserProfile { get; set; } = null!;


        // Indicates if the appointment is deleted (soft delete)
        public bool IsDeleted { get; set; } = false;
    }
}
