namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class UserDetailsViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public List<AdminPetSummaryViewModel> Pets { get; set; } = new List<AdminPetSummaryViewModel>();
    }
}
