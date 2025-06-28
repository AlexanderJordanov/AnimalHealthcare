using AnimalHealthcare.Web.ViewModels.Appointment;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAppointmentService
    {
        Task<List<MyAppointmentViewModel>> GetAppointmentsByUserIdAsync(string userId);
    }
}
