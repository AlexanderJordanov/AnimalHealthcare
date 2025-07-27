namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminAnimalDetailsViewModel
    {
        public string UserProfileId { get; set; } = null!;
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string Gender { get; set; } = null!;

        public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
    }
}
