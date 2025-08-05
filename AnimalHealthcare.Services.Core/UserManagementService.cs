using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class UserManagementService : IUserManagementService
    {
        private readonly AnimalHealthcareDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagementService(AnimalHealthcareDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<SimpleUserProfileViewModel?> GetUserBasicInfoAsync(string userId)
        {
            var userProfile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (userProfile == null || userProfile.User == null)
            {
                return null;
            }

            return new SimpleUserProfileViewModel
            {
                Id = userId,
                FullName = userProfile.FullName,
                Email = userProfile.User.Email!
            };
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var profile = await _context.UserProfiles
                .Include(p => p.Animals)
                .ThenInclude(a => a.Appointments)
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (user == null || profile == null)
                return false;

            foreach (var animal in profile.Animals)
            {
                if (animal.Appointments != null && animal.Appointments.Any())
                {
                    foreach (var appointment in animal.Appointments)
                    {
                        appointment.IsDeleted = true;
                    }
                }

                animal.IsDeleted = true;
            }

            _context.UserProfiles.Remove(profile);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminAnimalDetailsViewModel?> GetAnimalWithAppointmentsAsync(int animalId)
        {
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                    .ThenInclude(appt => appt.Doctor)
                .Include(a => a.Appointments)
                    .ThenInclude(appt => appt.Procedure)
                .FirstOrDefaultAsync(a => a.Id == animalId);

            if (animal == null)
                return null;

            return new AdminAnimalDetailsViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Age = animal.Age,
                Species = animal.Species,
                Breed = animal.Breed,
                Gender = animal.Gender.ToString(),
                UserProfileId = animal.UserProfileId,
                Appointments = animal.Appointments
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AppointmentViewModel
                    {
                        Id = a.Id,
                        AppointmentDateTime = a.AppointmentDateTime,
                        DoctorName = a.Doctor.Name,
                        ProcedureName = a.Procedure.Name
                    })
                    .ToList()
            };
        }

        public async Task<AdminUnregisterAnimalViewModel?> GetUnregisterAnimalViewModelAsync(int animalId)
        {
            var animal = await _context.Animals
                .Where(a => a.Id == animalId && !a.IsDeleted)
                .FirstOrDefaultAsync();

            if (animal == null)
                return null;

            return new AdminUnregisterAnimalViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Species = animal.Species,
                Breed = animal.Breed,
                UserProfileId = animal.UserProfileId
            };
        }

        public async Task<bool> UnregisterAnimalAsync(int animalId)
        {
            var animal = await _context.Animals
                .Include(a => a.Appointments)
                .FirstOrDefaultAsync(a => a.Id == animalId && !a.IsDeleted);

            if (animal == null)
                return false;

            foreach (var appointment in animal.Appointments)
            {
                appointment.IsDeleted = true;
            }

            animal.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public string? GetAnimalOwnerId(int animalId)
        {
            return _context.Animals
                .Where(a => a.Id == animalId)
                .Select(a => a.UserProfileId)
                .FirstOrDefault();
        }

        public async Task<AdminAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Procedure)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return null;

            return new AdminAppointmentDetailsViewModel
            {
                Id = appointment.Id,
                AnimalId = appointment.AnimalId,
                AnimalName = appointment.Animal.Name,
                AppointmentDateTime = appointment.AppointmentDateTime,
                DoctorName = appointment.Doctor.Name,
                DoctorSpecialization = appointment.Doctor.Specialization,
                ClinicName = appointment.Doctor.AnimalClinic.Name,
                ProcedureName = appointment.Procedure.Name,
                ProcedureDescription = appointment.Procedure.Description
            };
        }

        public async Task<AdminCancelAppointmentViewModel?> GetCancelAppointmentViewModelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return null;

            return new AdminCancelAppointmentViewModel
            {
                Id = appointment.Id,
                AppointmentDateTime = appointment.AppointmentDateTime,
                AnimalName = appointment.Animal.Name,
                AnimalId = appointment.AnimalId
            };
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted);

            if (appointment == null)
                return false;

            appointment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
