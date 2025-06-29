using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnimalHealthcare.Services.Core
{
    public class DoctorService : IDoctorService
    {
        private readonly AnimalHealthcareDbContext _context;

        public DoctorService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetDoctorsByProcedureAsync(int procedureId)
        {
            var doctors = await _context.DoctorProcedures
                .Where(dp => dp.ProcedureId == procedureId && !dp.Doctor.IsDeleted)
                .Select(dp => new SelectListItem
                {
                    Value = dp.DoctorId.ToString(),
                    Text = dp.Doctor.Name
                })
                .ToListAsync();

            return doctors;
        }
    }
}
