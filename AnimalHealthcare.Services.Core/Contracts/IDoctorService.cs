using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IDoctorService
    {
        Task<List<SelectListItem>> GetDoctorsByProcedureAsync(int procedureId);
    }
}
