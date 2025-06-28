using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Animal;
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

        public async Task RegisterAnimalAsync(string userId, RegisterPetViewModel model)
        {
            var animal = new Animal
            {
                Name = model.Name,
                Age = model.Age,
                Species = model.Species,
                Breed = model.Breed,
                Gender = model.Gender,
                UserProfileId = userId
            };

            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
        }

        public async Task<UnregisterPetViewModel?> GetPetUnregisterViewModelByIdAsync(int id)
        {
            return await _context.Animals
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new UnregisterPetViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Species = a.Species,
                    Breed = a.Breed
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> UnregisterPetAsync(int id)
        {
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (animal == null) return false;

            _context.Appointments.RemoveRange(animal.Appointments);

            animal.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
