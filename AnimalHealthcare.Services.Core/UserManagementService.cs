using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class UserManagementService : IUserManagementService
    {
        private readonly AnimalHealthcareDbContext _context;

        public UserManagementService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SimpleUserProfileViewModel>> GetAllUserProfilesAsync(string excludeUserId)
        {
            return await _context.UserProfiles
                .Where(p => p.Id != excludeUserId)
                .Select(p => new SimpleUserProfileViewModel
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Email = p.User.Email!
                })
                .ToListAsync();
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId)
        {
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .Include(p => p.Animals)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (profile == null)
                return null;

            return new UserDetailsViewModel
            {
                UserId = profile.Id,
                Email = profile.User.Email!,
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber ?? "N/A",
                Address = profile.Address ?? "N/A",
                Pets = profile.Animals
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AdminPetSummaryViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Species = a.Species,
                        Breed = a.Breed
                    })
                    .ToList()
            };
        }

    }
}
