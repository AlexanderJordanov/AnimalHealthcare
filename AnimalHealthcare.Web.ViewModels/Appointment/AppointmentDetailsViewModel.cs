namespace AnimalHealthcare.Web.ViewModels.Appointment
{
    public class AppointmentDetailsViewModel
    {
        // Pet Info
        public string OwnerFullName { get; set; } = null!;
        public string PetName { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;

        // Doctor Info
        public string DoctorName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string ClinicAddress { get; set; } = null!;

        // Procedure Info
        public string ProcedureName { get; set; } = null!;
        public string ProcedureDescription { get; set; } = null!;
    }
}
