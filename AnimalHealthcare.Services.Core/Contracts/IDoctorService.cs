using AnimalHealthcare.Web.ViewModels.Appointment;
using AnimalHealthcare.Web.ViewModels.Doctor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IDoctorService
    {
        Task<List<SelectListItem>> GetDoctorsByProcedureAsync(int procedureId);

        Task<DoctorListViewModel> GetDoctorsAsync(int page, int pageSize, string? sortBy, string? filterBy);

        Task<DoctorDetailsViewModel?> GetDoctorDetailsAsync(int doctorId);

        Task<string?> GetDoctorNameByIdAsync(int doctorId);
    }
}
