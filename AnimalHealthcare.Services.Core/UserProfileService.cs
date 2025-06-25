using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AnimalHealthcareDbContext _context;

        public UserProfileService(AnimalHealthcareDbContext context)
        {
            _context = context;
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


    }
}
