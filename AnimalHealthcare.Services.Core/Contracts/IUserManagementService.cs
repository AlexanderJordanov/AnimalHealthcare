using AnimalHealthcare.Web.ViewModels.UserManagement;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserManagementService
    {
        Task<IEnumerable<SimpleUserProfileViewModel>> GetAllUserProfilesAsync(string excludeUserId);

        Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId);
    }
}
