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
        public async Task<UnregisterPetViewModel?> GetPetUnregisterViewModelByIdAsync(int id, string? requestingUserId = null)
        {
            var animal = await _context.Animals
                .Where(a => a.Id == id && !a.IsDeleted)
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync();

            if (animal == null) return null;

            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
                return null;

            return new UnregisterPetViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Species = animal.Species,
                Breed = animal.Breed
            };
        }

        public async Task<bool> UnregisterPetAsync(int id, string? requestingUserId = null)
        {
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (animal == null) return false;

            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
                return false;

            _context.Appointments.RemoveRange(animal.Appointments);
            animal.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AnimalDetailsViewModel?> GetAnimalDetailsViewModelAsync(int animalId, string? requestingUserId = null)
        {
            var animal = await _context.Animals
                .Include(a => a.UserProfile)
                .Include(a => a.Appointments)
                .ThenInclude(appt => appt.Doctor)
                .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Appointments)
                .ThenInclude(appt => appt.Procedure)
                .FirstOrDefaultAsync(a => a.Id == animalId && !a.IsDeleted);

            if (animal == null) return null;

            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
            {
                return null; // Or throw a custom ForbiddenException, if desired
            }

            return new AnimalDetailsViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Age = animal.Age,
                Gender = animal.Gender.ToString(),
                Species = animal.Species,
                Breed = animal.Breed,
                Appointments = animal.Appointments.Select(a => new AnimalAppointmentViewModel
                {
                    AppointmentDateTime = a.AppointmentDateTime,
                    DoctorName = a.Doctor.Name,
                    ClinicName = a.Doctor.AnimalClinic.Name,
                    ProcedureName = a.Procedure.Name
                }).ToList()
            };
        }

        public async Task<EditPetViewModel?> BuildEditPetViewModelAsync(int id, string requestingUserId)
        {
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (animal == null || animal.UserProfileId != requestingUserId)
            {
                return null;
            }

            return new EditPetViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Age = animal.Age,
                Species = animal.Species,
                Breed = animal.Breed,
                Gender = animal.Gender
            };
        }

        public async Task<bool> UpdateAnimalAsync(EditPetViewModel model, string requestingUserId)
        {
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.Id && !a.IsDeleted);

            if (animal == null || animal.UserProfileId != requestingUserId)
            {
                return false;
            }

            if (animal.Name == model.Name &&
            animal.Age == model.Age &&
            animal.Species == model.Species &&
            animal.Breed == model.Breed &&
            animal.Gender == model.Gender)
            {
                return false; // no changes
            }

            animal.Name = model.Name;
            animal.Age = model.Age;
            animal.Species = model.Species;
            animal.Breed = model.Breed;
            animal.Gender = model.Gender;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
