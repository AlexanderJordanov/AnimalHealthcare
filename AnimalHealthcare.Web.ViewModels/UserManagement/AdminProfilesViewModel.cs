namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminProfilesViewModel
    {
        public string Email { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string? ProfilePictureUrl { get; set; }

        public IEnumerable<SimpleUserProfileViewModel> Users { get; set; } = new List<SimpleUserProfileViewModel>();
    }
}
