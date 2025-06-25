using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserProfileService
    {
        Task CreateUserProfileAsync(string userId, string fullName, string? phoneNumber, string? address, string? profilePictureUrl);
        Task<UserProfile?> GetByEmailAsync(string email);

        Task<UserProfile?> GetProfileByIdAsync(string userId);

        UserProfileViewModel BuildUserProfileViewModel(UserProfile profile, List<AnimalSummaryViewModel> animals);

        Task UpdateProfilePictureAsync(string userId, string profilePictureUrl);
    }
}

