using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAppointmentService
    {
        Task<List<MyAppointmentViewModel>> GetAppointmentsByUserIdAsync(string userId);

        Task<CreateAppointmentViewModel> BuildCreateAppointmentViewModelAsync(string userId);

        Task<List<SelectListItem>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);

        Task<bool> CreateAppointmentAsync(CreateAppointmentViewModel model, string userId);

        Task<AppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId, string requestingUserId);
    }
}
