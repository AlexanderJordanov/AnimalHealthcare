using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Web.ViewModels.UserManagement;
using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserProfileService
    {
        Task CreateUserProfileAsync(string userId, string fullName, string? phoneNumber, string? address, string? profilePictureUrl);

        Task<UserProfile?> GetByEmailAsync(string email);

        Task<UserProfile?> GetProfileByIdAsync(string profileId, string requestingUserId);

        UserProfileViewModel BuildUserProfileViewModel(UserProfile profile, List<AnimalSummaryViewModel> animals);

        Task<ServiceOperationResult> UpdateProfilePictureAsync(string profileId, string? profilePictureUrl, string requestingUserId);

        Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string profileId, string requestingUserId);

        Task<ServiceOperationResult> UpdateEmailAsync(string profileId, EditEmailViewModel model, string requestingUserId);

        Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string profileId, string requestingUserId);

        Task<ServiceOperationResult> UpdateFullNameAsync(string profileId, EditFullNameViewModel model, string requestingUserId);

        Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string profileId, string requestingUserId);

        Task<ServiceOperationResult> UpdatePhoneNumberAsync(string profileId, EditPhoneNumberViewModel model, string requestingUserId);

        Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string profileId, string requestingUserId);

        Task<ServiceOperationResult> UpdateAddressAsync(string profileId, EditAddressViewModel model, string requestingUserId);

        Task<ServiceOperationResult> DeleteUserProfileAsync(string targetUserId, string requestingUserId);
    }
}

