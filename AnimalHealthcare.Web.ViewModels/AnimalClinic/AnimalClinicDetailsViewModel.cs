namespace AnimalHealthcare.Web.ViewModels.AnimalClinic
{
    public class AnimalClinicDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public IEnumerable<AnimalClinicDoctorViewModel> Doctors { get; set; } = new List<AnimalClinicDoctorViewModel>();
    }
}
