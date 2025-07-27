namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminPetSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
    }
}
