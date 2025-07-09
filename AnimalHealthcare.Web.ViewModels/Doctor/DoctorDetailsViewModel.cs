namespace AnimalHealthcare.Web.ViewModels.Doctor
{
    public class DoctorDetailsViewModel
    {
        // Doctor info
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string ProceduresPerformed { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = null!;

        // Clinic info
        public string ClinicName { get; set; } = null!;
        public string ClinicAddress { get; set; } = null!;
        public string ClinicPhoneNumber { get; set; } = null!;
        public string ClinicImageUrl { get; set; } = null!;
    }
}
