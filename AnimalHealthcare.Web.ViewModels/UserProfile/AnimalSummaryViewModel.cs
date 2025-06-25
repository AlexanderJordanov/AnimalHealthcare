namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    public class AnimalSummaryViewModel
    {
        public int Id { get; set; }
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
