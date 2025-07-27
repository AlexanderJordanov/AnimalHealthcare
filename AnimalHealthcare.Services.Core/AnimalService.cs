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

        /// <summary>
        /// Retrieves a list of basic animal summaries owned by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user (owner).</param>
        /// <returns>A list of animal summary view models.</returns>
        public async Task<List<AnimalSummaryViewModel>> GetAnimalSummariesByOwnerIdAsync(string userId)
        {
            return await _context.Animals
                // Filter out animals that are soft-deleted and not owned by the user
                .Where(a => a.UserProfileId == userId && !a.IsDeleted)
                // Project each animal to a simplified summary view model
                .Select(a => new AnimalSummaryViewModel
                {
                    Id = a.Id,
                    Species = a.Species,
                    Breed = a.Breed,
                    Name = a.Name
                })
                .ToListAsync(); // Execute the query asynchronously and return the list
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

            // Create a new Animal entity using the provided model data
            var animal = new Animal
            {
                Name = model.Name,
                Age = model.Age,
                Species = model.Species,
                Breed = model.Breed,
                Gender = model.Gender,
                UserProfileId = userId
            };

            // Add the new animal to the context for tracking
            _context.Animals.Add(animal);

            // Save the changes to the database
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
            // Fetch the animal by ID, ensuring it is not marked as deleted
            var animal = await _context.Animals
                .Where(a => a.Id == id && !a.IsDeleted)
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync();

            // Return null if no animal is found
            if (animal == null) return null;

            // If a user ID is provided, verify the animal belongs to them
            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
                return null;

            // Return the view model for confirmation
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
        public async Task<bool> UnregisterPetAsync(int id, string? requestingUserId = null)
        {
            // Fetch the animal including its appointments and user profile, only if it’s not already deleted
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            // If the animal does not exist, return false
            if (animal == null)
            {
                return false;
            }
                
            // If user validation is needed, ensure the requester owns the animal
            if (requestingUserId != null && animal.UserProfileId != requestingUserId) 
            {
                return false;
            }

            
            foreach (var appointment in animal.Appointments)
            {
                // Mark each appointment as logically deleted
                appointment.IsDeleted = true;
            }

            // Mark the animal as logically deleted
            animal.IsDeleted = true;

            // Save all changes to the database
            await _context.SaveChangesAsync();
            return true;
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
            // Retrieve the animal and include its owner, appointments, and related doctor/clinic/procedure data
            var animal = await _context.Animals
                .Include(a => a.UserProfile)
                .Include(a => a.Appointments)
                    .ThenInclude(appt => appt.Doctor)
                        .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Appointments)
                    .ThenInclude(appt => appt.Procedure)
                .FirstOrDefaultAsync(a => a.Id == animalId && !a.IsDeleted);

            // Return null if animal is not found
            if (animal == null) return null;

            // If a requesting user is specified, ensure they own this animal
            if (requestingUserId != null && animal.UserProfileId != requestingUserId)
            {
                return null; // Optionally, throw a ForbiddenException instead
            }

            // Build and return the view model
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
            // Retrieve the pet by ID if it's not marked as deleted
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            // Return null if the pet doesn't exist or doesn't belong to the requesting user
            if (animal == null || animal.UserProfileId != requestingUserId)
            {
                return null;
            }

            // Construct and return the view model with the pet's current data
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
        public async Task<bool?> UpdateAnimalAsync(EditPetViewModel model, string requestingUserId)
        {
            // Retrieve the animal by ID and ensure it is not marked as deleted
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.Id && !a.IsDeleted);

            // Ensure the animal exists and belongs to the requesting user
            if (animal == null || animal.UserProfileId != requestingUserId)
            {
                return null; // Not found or unauthorized
            }

            // Return false early if the submitted values match the existing ones
            if (animal.Name == model.Name &&
                animal.Age == model.Age &&
                animal.Species == model.Species &&
                animal.Breed == model.Breed &&
                animal.Gender == model.Gender)
            {
                return false; // No changes detected
            }

            // Apply updates
            animal.Name = model.Name;
            animal.Age = model.Age;
            animal.Species = model.Species;
            animal.Breed = model.Breed;
            animal.Gender = model.Gender;

            // Save changes to the database
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
