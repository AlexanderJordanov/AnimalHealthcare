using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AnimalHealthcareDbContext _context;

        public AppointmentService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }
        public async Task<List<MyAppointmentViewModel>> GetAppointmentsByUserIdAsync(string userId)
        {
            return await _context.Appointments
                .Where(a => a.UserProfileId == userId && !a.IsDeleted)
                .Include(a => a.Animal)
                .Include(a => a.Procedure)
                .Select(a => new MyAppointmentViewModel
                {
                    Id = a.Id,
                    PetName = a.Animal.Name,
                    ProcedureName = a.Procedure.Name,
                    AppointmentDateTime = a.AppointmentDateTime
                })
                .ToListAsync();
        }
    }
}
