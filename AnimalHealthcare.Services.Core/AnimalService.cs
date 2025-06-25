using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class AnimalService : IAnimalService
    {
        private readonly AnimalHealthcareDbContext _context;

        public AnimalService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }
        public async Task<List<AnimalSummaryViewModel>> GetAnimalSummariesByOwnerIdAsync(string userId)
        {
            return await _context.Animals
                .Where(a => a.UserProfileId == userId && !a.IsDeleted)
                .Select(a => new AnimalSummaryViewModel
                     {
                         Id = a.Id,
                         Species = a.Species,
                         Breed = a.Breed,
                         Name = a.Name
                     })
                .ToListAsync();
        }
    }
}
