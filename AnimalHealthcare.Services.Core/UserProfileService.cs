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

        // Used in login logic to retrieve user profile by email
        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return await _context.UserProfiles.FindAsync(user.Id);
        }

        public async Task<UserProfile?> GetProfileByIdAsync(string userId)
        {
            return await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == userId);
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

        public async Task UpdateProfilePictureAsync(string userId, string profilePictureUrl)
        {
            var profile = await _context.UserProfiles.FindAsync(userId);
            if (profile == null) return;

            profile.ProfilePictureUrl = profilePictureUrl;
            await _context.SaveChangesAsync();
        }

        public async Task<EditEmailViewModel?> BuildEditEmailViewModelAsync(string userId)
        {
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (profile == null) return null;

            return new EditEmailViewModel
            {
                Email = profile.User.Email!
            };
        }

        public async Task<(bool success, bool unchanged)> UpdateEmailAsync(string userId, EditEmailViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, false);
            }

            if (user.Email == model.Email)
            {
                return (true, true); // success, but unchanged
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var result = await _userManager.ChangeEmailAsync(user, model.Email, token);

            if (!result.Succeeded)
            {
                return (false, false);
            }

            user.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return (false, false);
            }

            await _signInManager.RefreshSignInAsync(user);

            return (true, false); // success, email changed
        }

        public async Task<EditFullNameViewModel?> BuildEditFullNameViewModelAsync(string userId)
        {
            var profile = await _context.UserProfiles.FindAsync(userId);
            if (profile == null) return null;

            return new EditFullNameViewModel
            {
                FullName = profile.FullName
            };
        }

        public async Task<bool> UpdateFullNameAsync(string userId, EditFullNameViewModel model)
        {
            var profile = await _context.UserProfiles.FindAsync(userId);
            if (profile == null) throw new InvalidOperationException("Profile not found.");

            if (profile.FullName == model.FullName)
            {
                return false; // unchanged
            }

            profile.FullName = model.FullName;
            await _context.SaveChangesAsync();

            return true; // updated
        }
    }
}
