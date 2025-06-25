using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core.Contracts;

namespace AnimalHealthcare.Services.Core
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AnimalHealthcareDbContext _context;

        public UserProfileService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

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
    }
}
