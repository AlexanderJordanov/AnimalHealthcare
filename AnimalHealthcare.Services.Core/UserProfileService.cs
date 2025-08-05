using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AnimalHealthcareDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserProfileService(AnimalHealthcareDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Creates a new user profile upon registration.
        /// </summary>
        /// <param name="userId">The ID of the user (typically from Identity).</param>
        /// <param name="fullName">The full name of the user.</param>
        /// <param name="phoneNumber">Optional phone number of the user.</param>
        /// <param name="address">Optional address of the user.</param>
        /// <param name="profilePictureUrl">Optional profile picture URL.</param>
        public async Task CreateUserProfileAsync(string userId, string fullName, string? phoneNumber, string? address, string? profilePictureUrl)
        {
            var profile = new UserProfile
            {
                Id = userId,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Address = address,
                ProfilePictureUrl = profilePictureUrl
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a user profile based on the user's email address.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>The corresponding <see cref="UserProfile"/> or null if not found.</returns>
        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            return await _context.UserProfiles.FindAsync(user.Id);
        }

        /// <summary>
        /// Retrieves a user profile by ID, only if the requesting user matches the profile owner.
        /// </summary>
        /// <param name="profileId">The ID of the profile to retrieve.</param>
        /// <param name="requestingUserId">The ID of the user making the request (for authorization).</param>
        /// <returns>The <see cref="UserProfile"/> if access is allowed; otherwise, null.</returns>
        public async Task<UserProfile?> GetProfileByIdAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            return await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == profileId);
        }

        /// <summary>
        /// Constructs a user profile view model using profile information and a list of the user's animals.
        /// </summary>
        /// <param name="profile">The user's profile entity containing personal and contact info.</param>
        /// <param name="animals">A list of animal summaries associated with the user.</param>
        /// <returns>A fully populated <see cref="UserProfileViewModel"/> object for use in the UI.</returns>
        public UserProfileViewModel BuildUserProfileViewModel(UserProfile profile, List<AnimalSummaryViewModel> animals)
        {
            return new UserProfileViewModel
            {
                ProfileId = profile.Id,

                Email = profile.User.Email,

                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                ProfilePictureUrl = profile.ProfilePictureUrl,

                Animals = animals
            };
        }

        /// <summary>
        /// Updates the profile picture URL for a given user profile, ensuring the requester is authorized.
        /// </summary>
        /// <param name="profileId">The ID of the profile to update.</param>
        /// <param name="profilePictureUrl">The new profile picture URL (nullable to allow removal).</param>
        /// <param name="requestingUserId">The ID of the user making the request (used for authorization).</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<ServiceOperationResult> UpdateProfilePictureAsync(string profileId, string? profilePictureUrl, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            if (profile.ProfilePictureUrl == profilePictureUrl)
                return ServiceOperationResult.NoChange;

            profile.ProfilePictureUrl = profilePictureUrl;

            await _context.SaveChangesAsync();
            return ServiceOperationResult.Success;
        }

        /// <summary>
        /// Builds the view model required to edit the user's email, with authorization validation.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile whose email is being edited.</param>
        /// <param name="requestingUserId">The ID of the user making the request (must match the profile owner).</param>
        /// <returns>
        /// An <see cref="EditEmailViewModel"/> populated with the current email,
        /// or null if the profile does not exist or the user is unauthorized.
        /// </returns>
        public async Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string targetProfileId, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == targetProfileId);

            if (profile == null || profile.User == null)

            return new EditEmailViewModel
            {
                ProfileId = targetProfileId,
                Email = profile.User.Email ?? string.Empty
            };
        }

        /// <summary>
        /// Updates the user's email address if authorized and the email has changed.
        /// </summary>
        /// <param name="targetProfileId">The ID of the user profile to update.</param>
        /// <param name="model">The model containing the new email address.</param>
        /// <param name="requestingUserId">The ID of the currently logged-in user (used for authorization).</param>
        /// <returns>
        /// A tuple:
        /// - success: true if the update succeeded;
        /// - unchanged: true if the email was already the same and no update was needed.
        /// </returns>
        public async Task<ServiceOperationResult> UpdateEmailAsync(string targetProfileId, EditEmailViewModel model, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var user = await _userManager.FindByIdAsync(targetProfileId);
            if (user == null)
                return ServiceOperationResult.NotFound;

            if (string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                return ServiceOperationResult.NoChange;

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var result = await _userManager.ChangeEmailAsync(user, model.Email, token);
            if (!result.Succeeded)
                return ServiceOperationResult.Failed;

            user.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ServiceOperationResult.Failed;

            await _signInManager.RefreshSignInAsync(user);
            return ServiceOperationResult.Success;
        }

        /// <summary>
        /// Builds a view model for editing a user's full name, ensuring the requesting user is authorized.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request (for authorization).</param>
        /// <returns>
        /// A populated <see cref="EditFullNameViewModel"/> if authorized and profile found; otherwise, null.
        /// </returns>
        public async Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string targetProfileId, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return null;

            return new EditFullNameViewModel
            {
                ProfileId = targetProfileId,
                FullName = profile.FullName
            };
        }

        /// <summary>
        /// Updates the full name of a user profile if the requesting user is authorized and the name has changed.
        /// </summary>
        /// <param name="targetProfileId">The ID of the user profile to update.</param>
        /// <param name="model">The view model containing the new full name.</param>
        /// <param name="requestingUserId">The ID of the user making the request (for authorization).</param>
        /// <returns>True if the update was successful; false otherwise.</returns>
        public async Task<ServiceOperationResult> UpdateFullNameAsync(string targetProfileId, EditFullNameViewModel model, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            var newName = (model.FullName ?? string.Empty).Trim();
            var currentName = (profile.FullName ?? string.Empty).Trim();

            if (string.Equals(currentName, newName, StringComparison.Ordinal))
                return ServiceOperationResult.NoChange;

            profile.FullName = newName;
            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }

        /// <summary>
        /// Builds the view model used for editing a user's phone number, 
        /// only if the requesting user is authorized to access the profile.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// An instance of <see cref="EditPhoneNumberViewModel"/> if the profile exists and authorization passes; otherwise, null.
        /// </returns>
        public async Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string targetProfileId, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return null;

            return new EditPhoneNumberViewModel
            {
                ProfileId = targetProfileId,
                PhoneNumber = profile.PhoneNumber
            };
        }

        /// <summary>
        /// Updates a user's phone number if authorized and the new number differs from the current one.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile to update.</param>
        /// <param name="model">The model containing the new phone number.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// True if the update was successful; false if unauthorized or the profile was not found.
        /// Throws <see cref="InvalidOperationException"/> if the new phone number is the same as the current one.
        /// </returns>
        public async Task<ServiceOperationResult> UpdatePhoneNumberAsync(string targetProfileId, EditPhoneNumberViewModel model, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            var newPhone = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber!.Trim();

            var currentPhone = profile.PhoneNumber?.Trim();

            if (string.Equals(currentPhone, newPhone, StringComparison.Ordinal))
                return ServiceOperationResult.NoChange;

            profile.PhoneNumber = newPhone;
            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }

        /// <summary>
        /// Builds a view model for editing a user's address, ensuring the requester is authorized.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// An <see cref="EditAddressViewModel"/> if the user is authorized and the profile exists; otherwise, null.
        /// </returns>
        public async Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string targetProfileId, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return null;

            return new EditAddressViewModel
            {
                ProfileId = targetProfileId,
                Address = profile.Address
            };
        }


        /// <summary>
        /// Updates the address of a user's profile, verifying the requester's identity and checking for changes.
        /// </summary>
        /// <param name="targetProfileId">The ID of the profile to update.</param>
        /// <param name="model">The view model containing the new address.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// A tuple where:
        /// - success: Indicates whether the update was successful.
        /// - unchanged: Indicates whether the submitted address was the same as the existing one.
        /// </returns>
        public async Task<ServiceOperationResult> UpdateAddressAsync(string targetProfileId, EditAddressViewModel model, string requestingUserId)
        {
            if (targetProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var profile = await _context.UserProfiles.FindAsync(targetProfileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            var newAddress = model.Address?.Trim();
            var currentAddress = profile.Address?.Trim();

            if (string.Equals(currentAddress, newAddress, StringComparison.Ordinal))
                return ServiceOperationResult.NoChange;

            profile.Address = newAddress;
            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }



        /// <summary>
        /// Deletes a user profile, its associated identity user, and all related animals and appointments.
        /// Ensures that only the profile owner can perform this operation.
        /// </summary>
        /// <param name="targetUserId">The ID of the user profile to delete.</param>
        /// <param name="requestingUserId">The ID of the user making the deletion request.</param>
        /// <returns>
        /// True if deletion was successful; false otherwise.
        /// </returns>
        public async Task<ServiceOperationResult> DeleteUserProfileAsync(string targetUserId, string requestingUserId)
        {
            if (targetUserId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            var userProfile = await _context.UserProfiles
                .Include(p => p.Animals)
                .ThenInclude(a => a.Appointments)
                .FirstOrDefaultAsync(p => p.Id == targetUserId);

            var user = await _userManager.FindByIdAsync(targetUserId);

            if (userProfile == null || user == null)
                return ServiceOperationResult.NotFound;

            foreach (var animal in userProfile.Animals)
            {
                foreach (var appointment in animal.Appointments)
                {
                    appointment.IsDeleted = true;
                }

                animal.IsDeleted = true;
            }

            _context.UserProfiles.Remove(userProfile);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return ServiceOperationResult.Failed;

            await _signInManager.SignOutAsync();

            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }
    }
}
