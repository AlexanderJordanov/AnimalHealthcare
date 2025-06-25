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

        Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string userId);

        Task<(bool success, bool unchanged)> UpdateEmailAsync(string userId, EditEmailViewModel model);

        Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string userId);

        Task<bool> UpdateFullNameAsync(string userId, EditFullNameViewModel model);

        Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string userId);

        Task UpdatePhoneNumberAsync(string userId, EditPhoneNumberViewModel model);

        Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string userId);

        Task<bool> UpdateAddressAsync(string userId, EditAddressViewModel model);
    }
}

