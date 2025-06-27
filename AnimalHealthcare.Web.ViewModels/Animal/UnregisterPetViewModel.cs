namespace AnimalHealthcare.Web.ViewModels.Animal
{
    public class UnregisterPetViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Species { get; set; } = null!;

        public string Breed { get; set; } = null!;
    }
}
