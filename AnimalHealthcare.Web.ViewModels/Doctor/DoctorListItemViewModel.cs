namespace AnimalHealthcare.Web.ViewModels.Doctor
{
    public class DoctorListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Specialization { get; set; } = null!;

        public string ClinicName { get; set; } = null!;

        public string? ProfileImageUrl { get; set; }
    }
}
