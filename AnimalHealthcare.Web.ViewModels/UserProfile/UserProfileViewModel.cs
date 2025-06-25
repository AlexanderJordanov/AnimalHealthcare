namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    public class UserProfileViewModel
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public ICollection<AnimalSummaryViewModel> Animals { get; set; } = new List<AnimalSummaryViewModel>();
    }
}
