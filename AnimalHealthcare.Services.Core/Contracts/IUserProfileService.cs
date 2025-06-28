using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserProfileService
    {
        Task CreateUserProfileAsync(string userId, string fullName, string? phoneNumber, string? address, string? profilePictureUrl);
        Task<UserProfile?> GetByEmailAsync(string email);

        Task<UserProfile?> GetProfileByIdAsync(string profileId, string requestingUserId);

        UserProfileViewModel BuildUserProfileViewModel(UserProfile profile, List<AnimalSummaryViewModel> animals);

        Task<bool> UpdateProfilePictureAsync(string profileId, string? profilePictureUrl, string requestingUserId);

        Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string profileId, string requestingUserId);

        Task<(bool success, bool unchanged)> UpdateEmailAsync(string profileId, EditEmailViewModel model, string requestingUserId);

        Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string profileId, string requestingUserId);

        Task<bool> UpdateFullNameAsync(string profileId, EditFullNameViewModel model, string requestingUserId);

        Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string profileId, string requestingUserId);

        Task<bool> UpdatePhoneNumberAsync(string profileId, EditPhoneNumberViewModel model, string requestingUserId);

        Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string profileId, string requestingUserId);

        Task<(bool success, bool unchanged)> UpdateAddressAsync(string profileId, EditAddressViewModel model, string requestingUserId);
    }
}

