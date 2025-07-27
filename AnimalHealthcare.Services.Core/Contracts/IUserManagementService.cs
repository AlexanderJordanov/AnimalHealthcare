using AnimalHealthcare.Web.ViewModels.Animal;
using AnimalHealthcare.Web.ViewModels.Appointment;
using AnimalHealthcare.Web.ViewModels.UserManagement;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserManagementService
    {
        Task<IEnumerable<SimpleUserProfileViewModel>> GetAllUserProfilesAsync(string excludeUserId);

        Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId);

        Task<SimpleUserProfileViewModel?> GetUserBasicInfoAsync(string userId);

        Task<bool> DeleteUserAsync(string userId);

        Task<AdminAnimalDetailsViewModel?> GetAnimalWithAppointmentsAsync(int animalId);

        Task<AdminUnregisterAnimalViewModel?> GetUnregisterAnimalViewModelAsync(int animalId);

        Task<bool> UnregisterAnimalAsync(int animalId);

        string? GetAnimalOwnerId(int animalId);

        Task<AdminAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId);

        Task<AdminCancelAppointmentViewModel?> GetCancelAppointmentViewModelAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId);
    }
}
