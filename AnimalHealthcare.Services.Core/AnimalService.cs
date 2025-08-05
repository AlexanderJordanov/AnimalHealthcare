using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
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

        /// <summary>
        /// Retrieves a list of basic animal summaries owned by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user (owner).</param>
        /// <returns>A list of animal summary view models.</returns>
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

        /// <summary>
        /// Registers a new animal (pet) for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the animal.</param>
        /// <param name="model">The view model containing the animal's registration data.</param>
        public async Task RegisterAnimalAsync(string userId, RegisterPetViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Species))
            {
                throw new ArgumentException("Pet name and species are required.");
            }

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

        /// <summary>
        /// Retrieves the necessary data to confirm pet unregistration, verifying ownership if needed.
        /// </summary>
        /// <param name="id">The ID of the animal to unregister.</param>
        /// <param name="requestingUserId">The ID of the user requesting the operation (optional).</param>
        /// <returns>
        /// An <see cref="UnregisterPetViewModel"/> if the animal exists and, if applicable, belongs to the requesting user;
        /// otherwise, <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Unregisters a pet by marking it as deleted and removing its associated appointments.
        /// </summary>
        /// <param name="id">The ID of the pet to unregister.</param>
        /// <param name="requestingUserId">
        /// Optional. The ID of the user requesting the unregistration.
        /// If provided, ownership is validated before proceeding.
        /// </param>
        /// <returns>
        /// <c>true</c> if the pet was successfully unregistered; otherwise, <c>false</c>.
        /// </returns>
        public async Task<ServiceOperationResult> UnregisterPetAsync(int id, string? requestingUserId = null)
        {
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (animal == null)
                return ServiceOperationResult.NotFound;

            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            foreach (var appointment in animal.Appointments)
                appointment.IsDeleted = true;

            animal.IsDeleted = true;

            await _context.SaveChangesAsync();
            return ServiceOperationResult.Success;
        }


        /// <summary>
        /// Retrieves detailed information for a specific animal, including its appointments,
        /// related doctors, procedures, and clinics. Optionally verifies ownership.
        /// </summary>
        /// <param name="animalId">The ID of the animal to retrieve.</param>
        /// <param name="requestingUserId">
        /// Optional. The ID of the user requesting the data.
        /// If provided, verifies the animal belongs to the requesting user.
        /// </param>
        /// <returns>
        /// A fully populated <see cref="AnimalDetailsViewModel"/> if found and authorized, otherwise <c>null</c>.
        /// </returns>
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
                return null;
            }

            return new AnimalDetailsViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Age = animal.Age,
                Gender = animal.Gender.ToString(),
                Species = animal.Species,
                Breed = animal.Breed,
                Appointments = animal.Appointments
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AnimalAppointmentViewModel
                    {
                        AppointmentDateTime = a.AppointmentDateTime,
                        DoctorName = a.Doctor.Name,
                        ClinicName = a.Doctor.AnimalClinic.Name,
                        ProcedureName = a.Procedure.Name
                    }).ToList()
            };
        }

        /// <summary>
        /// Builds an <see cref="EditPetViewModel"/> for a pet that belongs to the requesting user.
        /// </summary>
        /// <param name="id">The ID of the pet to edit.</param>
        /// <param name="requestingUserId">The ID of the user attempting to edit the pet.</param>
        /// <returns>
        /// An <see cref="EditPetViewModel"/> containing editable fields for the pet,
        /// or <c>null</c> if the pet is not found or does not belong to the requesting user.
        /// </returns>
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

        /// <summary>
        /// Updates an existing animal's information if it belongs to the requesting user and changes are detected.
        /// </summary>
        /// <param name="model">The updated pet data from the edit form.</param>
        /// <param name="requestingUserId">The ID of the user attempting the update.</param>
        /// <returns>
        /// <c>true</c> if the update was applied successfully; 
        /// <c>false</c> if the pet wasn't found, wasn't owned by the user, or no changes were made.
        /// </returns>
        public async Task<ServiceOperationResult> UpdateAnimalAsync(EditPetViewModel model, string requestingUserId)
        {
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.Id && !a.IsDeleted);

            if (animal == null)
                return ServiceOperationResult.NotFound;

            if (animal.UserProfileId != requestingUserId)
                return ServiceOperationResult.Unauthorized;

            if (animal.Name.Trim() == model.Name.Trim() &&
                animal.Age == model.Age &&
                animal.Species.Trim() == model.Species.Trim() &&
                animal.Breed.Trim() == model.Breed.Trim() &&
                animal.Gender == model.Gender)
            {
                return ServiceOperationResult.NoChange;
            }

            animal.Name = model.Name.Trim();
            animal.Age = model.Age;
            animal.Species = model.Species.Trim();
            animal.Breed = model.Breed.Trim();
            animal.Gender = model.Gender;

            await _context.SaveChangesAsync();
            return ServiceOperationResult.Success;
        }
    }
}
