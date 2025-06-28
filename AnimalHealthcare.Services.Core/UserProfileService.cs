using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
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

        // Used in registration logic to create a new user profile
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

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return await _context.UserProfiles.FindAsync(user.Id);
        }

        public async Task<UserProfile?> GetProfileByIdAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            return await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == profileId);
        }

        public UserProfileViewModel BuildUserProfileViewModel(UserProfile profile, List<AnimalSummaryViewModel> animals)
        {
            return new UserProfileViewModel
            {
                Email = profile.User.Email,
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                Animals = animals
            };
        }

        public async Task<bool> UpdateProfilePictureAsync(string profileId, string? profilePictureUrl, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return false;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null) return false;

            profile.ProfilePictureUrl = profilePictureUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            return profile == null ? null : new EditEmailViewModel { Email = profile.User.Email! };
        }

        public async Task<(bool success, bool unchanged)> UpdateEmailAsync(string profileId, EditEmailViewModel model, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return (false, false);

            var user = await _userManager.FindByIdAsync(profileId);
            if (user == null) return (false, false);

            if (user.Email == model.Email)
                return (true, true);

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var result = await _userManager.ChangeEmailAsync(user, model.Email, token);
            if (!result.Succeeded) return (false, false);

            user.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return (false, false);

            await _signInManager.RefreshSignInAsync(user);
            return (true, false);
        }

        public async Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            return profile == null ? null : new EditFullNameViewModel { FullName = profile.FullName };
        }

        public async Task<bool> UpdateFullNameAsync(string profileId, EditFullNameViewModel model, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return false;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null) return false;

            if (profile.FullName == model.FullName)
                return false;

            profile.FullName = model.FullName;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EditPhoneNumberViewModel?> BuildEditPhoneNumberViewModelAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            return profile == null ? null : new EditPhoneNumberViewModel { PhoneNumber = profile.PhoneNumber };
        }

        public async Task<bool> UpdatePhoneNumberAsync(string profileId, EditPhoneNumberViewModel model, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return false;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null) return false;

            var newPhone = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber;

            if (profile.PhoneNumber == newPhone)
                throw new InvalidOperationException("Phone number is unchanged.");

            profile.PhoneNumber = newPhone;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EditAddressViewModel?> BuildEditAddressViewModelAsync(string profileId, string requestingUserId)
        {
            if (profileId != requestingUserId)
                return null;

            var profile = await _context.UserProfiles.FindAsync(profileId);
            return profile == null ? null : new EditAddressViewModel { Address = profile.Address };
        }

        public async Task<(bool success, bool unchanged)> UpdateAddressAsync(string profileId, EditAddressViewModel model, string requestingUserId)
        {
            if (profileId != requestingUserId) return (false, false);

            var profile = await _context.UserProfiles.FindAsync(profileId);
            if (profile == null) return (false, false);

            if (profile.Address == model.Address)
            {
                return (true, true); // success, unchanged
            }

            profile.Address = model.Address;
            await _context.SaveChangesAsync();
            return (true, false);
        }

    }
}
