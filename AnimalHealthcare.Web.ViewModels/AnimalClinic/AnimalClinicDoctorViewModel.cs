namespace AnimalHealthcare.Web.ViewModels.AnimalClinic
{
    public class AnimalClinicDoctorViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Specialization { get; set; } = null!;

        public int YearsOfExperience { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string ProfileImageUrl { get; set; } = null!;
    }
}
