namespace AnimalHealthcare.Web.ViewModels.Animal
{
    public class AnimalDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;

        public List<AnimalAppointmentViewModel> Appointments { get; set; } = new();
    }
}
