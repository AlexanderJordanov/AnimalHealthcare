namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminUnregisterAnimalViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string UserProfileId { get; set; } = null!;
    }
}
