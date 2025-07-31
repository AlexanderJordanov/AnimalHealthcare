using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserManagement;
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
            // Create a new UserProfile instance with provided data
            var profile = new UserProfile
            {
                Id = userId,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Address = address,
                ProfilePictureUrl = profilePictureUrl
            };

            // Add and persist the new profile
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
            // Find the user by their email from the Identity Users table
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            // Retrieve the associated user profile using the user's ID
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
            // Ensure the requesting user is the owner of the profile
            if (profileId != requestingUserId)
                return null;

            // Fetch the profile and include related Identity user
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
                // Extract user account data from the linked Identity user
                Email = profile.User.Email,

                // Personal profile information
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                ProfilePictureUrl = profile.ProfilePictureUrl,

                // Pets owned by the user
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
            // 1. Authorization check
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // 2. Retrieve profile from DB
            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            // 3. If no changes, short-circuit (optional)
            if (profile.ProfilePictureUrl == profilePictureUrl)
                return ServiceOperationResult.NoChange;

            // 4. Apply update
            profile.ProfilePictureUrl = profilePictureUrl;

            // 5. Save changes
            await _context.SaveChangesAsync();
            return ServiceOperationResult.Success;
        }


        /// <summary>
        /// Builds the view model required to edit the user's email, with authorization validation.
        /// </summary>
        /// <param name="profileId">The ID of the profile whose email is being edited.</param>
        /// <param name="requestingUserId">The ID of the user making the request (must match the profile owner).</param>
        /// <returns>
        /// An <see cref="EditEmailViewModel"/> populated with the current email,
        /// or null if the profile does not exist or the user is unauthorized.
        /// </returns>
        public async Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string profileId, string requestingUserId)
        {
            // Ensure the requesting user is the owner of the profile
            if (profileId != requestingUserId)
                return null;

            // Load the user profile along with associated user account
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            // Return view model with current email or null if profile not found
            return profile == null
                ? null
                : new EditEmailViewModel { Email = profile.User.Email! };
        }

        /// <summary>
        /// Updates the user's email address if authorized and the email has changed.
        /// </summary>
        /// <param name="profileId">The ID of the user profile to update.</param>
        /// <param name="model">The model containing the new email address.</param>
        /// <param name="requestingUserId">The ID of the currently logged-in user (used for authorization).</param>
        /// <returns>
        /// A tuple:
        /// - success: true if the update succeeded;
        /// - unchanged: true if the email was already the same and no update was needed.
        /// </returns>
        public async Task<ServiceOperationResult> UpdateEmailAsync(string profileId, EditEmailViewModel model, string requestingUserId)
        {
            // Step 1: Authorization check
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // Step 2: Try to find the user
            var user = await _userManager.FindByIdAsync(profileId);
            if (user == null)
                return ServiceOperationResult.NotFound;

            // Step 3: Check if email is unchanged
            if (user.Email == model.Email)
                return ServiceOperationResult.NoChange;

            // Step 4: Attempt email change
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var result = await _userManager.ChangeEmailAsync(user, model.Email, token);
            if (!result.Succeeded)
                return ServiceOperationResult.Failed;

            // Step 5: Update username to match new email
            user.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ServiceOperationResult.Failed;

            // Step 6: Refresh sign-in to apply changes
            await _signInManager.RefreshSignInAsync(user);

            return ServiceOperationResult.Success;
        }


        /// <summary>
        /// Builds a view model for editing a user's full name, ensuring the requesting user is authorized.
        /// </summary>
        /// <param name="profileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request (for authorization).</param>
        /// <returns>
        /// A populated <see cref="EditFullNameViewModel"/> if authorized and profile found; otherwise, null.
        /// </returns>
        public async Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string profileId, string requestingUserId)
        {
            // Ensure the requesting user is only modifying their own profile
            if (profileId != requestingUserId)
                return null;

            // Attempt to find the user profile
            var profile = await _context.UserProfiles.FindAsync(profileId);

            // Return the view model if profile exists; otherwise return null
            return profile == null
                ? null
                : new EditFullNameViewModel { FullName = profile.FullName };
        }

        /// <summary>
        /// Updates the full name of a user profile if the requesting user is authorized and the name has changed.
        /// </summary>
        /// <param name="profileId">The ID of the user profile to update.</param>
        /// <param name="model">The view model containing the new full name.</param>
        /// <param name="requestingUserId">The ID of the user making the request (for authorization).</param>
        /// <returns>True if the update was successful; false otherwise.</returns>
        public async Task<ServiceOperationResult> UpdateFullNameAsync(string profileId, EditFullNameViewModel model, string requestingUserId)
        {
            // 1. Authorization check
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // 2. Retrieve the profile
            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            // 3. Check if there's any actual change
            if (profile.FullName.Trim() == model.FullName.Trim())
                return ServiceOperationResult.NoChange;

            // 4. Apply update and persist
            profile.FullName = model.FullName.Trim();
            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }


        /// <summary>
        /// Builds the view model used for editing a user's phone number, 
        /// only if the requesting user is authorized to access the profile.
        /// </summary>
        /// <param name="profileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// An instance of <see cref="EditPhoneNumberViewModel"/> if the profile exists and authorization passes; otherwise, null.
        /// </returns>
        public async Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string profileId, string requestingUserId)
        {
            // Ensure the requesting user is only editing their own profile
            if (profileId != requestingUserId)
                return null;

            // Retrieve the user profile from the database
            var profile = await _context.UserProfiles.FindAsync(profileId);

            // Return the phone number in a view model, or null if profile doesn't exist
            return profile == null
                ? null
                : new EditPhoneNumberViewModel { PhoneNumber = profile.PhoneNumber };
        }

        /// <summary>
        /// Updates a user's phone number if authorized and the new number differs from the current one.
        /// </summary>
        /// <param name="profileId">The ID of the profile to update.</param>
        /// <param name="model">The model containing the new phone number.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// True if the update was successful; false if unauthorized or the profile was not found.
        /// Throws <see cref="InvalidOperationException"/> if the new phone number is the same as the current one.
        /// </returns>
        public async Task<ServiceOperationResult> UpdatePhoneNumberAsync(string profileId, EditPhoneNumberViewModel model, string requestingUserId)
        {
            // 1. Authorization check: users can only edit their own profile
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // 2. Retrieve the user profile
            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            // 3. Normalize phone number input
            var newPhone = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber;

            // 4. Check if value is unchanged
            if (profile.PhoneNumber == newPhone)
                return ServiceOperationResult.NoChange;

            // 5. Apply update
            profile.PhoneNumber = newPhone;
            await _context.SaveChangesAsync();

            // 6. Return success
            return ServiceOperationResult.Success;
        }

        /// <summary>
        /// Builds a view model for editing a user's address, ensuring the requester is authorized.
        /// </summary>
        /// <param name="profileId">The ID of the profile to edit.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// An <see cref="EditAddressViewModel"/> if the user is authorized and the profile exists; otherwise, null.
        /// </returns>
        public async Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string profileId, string requestingUserId)
        {
            // Ensure the user is authorized to access this profile
            if (profileId != requestingUserId)
                return null;

            // Attempt to retrieve the profile
            var profile = await _context.UserProfiles.FindAsync(profileId);

            // Return the view model if the profile exists, otherwise null
            return profile == null ? null : new EditAddressViewModel { Address = profile.Address };
        }

        /// <summary>
        /// Updates the address of a user's profile, verifying the requester's identity and checking for changes.
        /// </summary>
        /// <param name="profileId">The ID of the profile to update.</param>
        /// <param name="model">The view model containing the new address.</param>
        /// <param name="requestingUserId">The ID of the user making the request.</param>
        /// <returns>
        /// A tuple where:
        /// - success: Indicates whether the update was successful.
        /// - unchanged: Indicates whether the submitted address was the same as the existing one.
        /// </returns>
        public async Task<ServiceOperationResult> UpdateAddressAsync(string profileId, EditAddressViewModel model, string requestingUserId)
        {
            // Authorization check
            if (profileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // Retrieve the profile
            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null)
                return ServiceOperationResult.NotFound;

            // Normalize and compare
            var newAddress = model.Address?.Trim();
            var currentAddress = profile.Address?.Trim();

            if (string.Equals(currentAddress, newAddress, StringComparison.Ordinal))
                return ServiceOperationResult.NoChange;

            // Apply the update
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
            // Step 1: Check if user is authorized to delete this profile
            if (targetUserId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            // Step 2: Load user profile with related animals and appointments
            var userProfile = await _context.UserProfiles
                .Include(p => p.Animals)
                .ThenInclude(a => a.Appointments)
                .FirstOrDefaultAsync(p => p.Id == targetUserId);

            // Step 3: Find the Identity user record
            var user = await _userManager.FindByIdAsync(targetUserId);

            // Step 4: Return NotFound if either is missing
            if (userProfile == null || user == null)
                return ServiceOperationResult.NotFound;

            // Step 5: Soft-delete all related appointments and animals
            foreach (var animal in userProfile.Animals)
            {
                foreach (var appointment in animal.Appointments)
                {
                    appointment.IsDeleted = true;
                }

                animal.IsDeleted = true;
            }

            // Step 6: Remove the user profile from the database
            _context.UserProfiles.Remove(userProfile);

            // Step 7: Delete the Identity user account
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return ServiceOperationResult.Failed;

            // Step 8: Sign out the user (log them out)
            await _signInManager.SignOutAsync();

            // Step 9: Save all changes to the database
            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }
    }
}
