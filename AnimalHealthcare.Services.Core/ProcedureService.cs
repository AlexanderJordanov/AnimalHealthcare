using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Procedure;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class ProcedureService : IProcedureService
    {
        private readonly AnimalHealthcareDbContext _context;

        public ProcedureService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProcedureListItemViewModel>> GetAllProceduresAsync()
        {
            return await _context.Procedures
                .Where(p => !p.IsDeleted)
                .Select(p => new ProcedureListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();
        }

        public async Task<ProcedureDetailsViewModel?> GetProcedureDetailsAsync(int procedureId)
        {
            var procedure = await _context.Procedures
                .Where(p => p.Id == procedureId && !p.IsDeleted)
                .Select(p => new ProcedureDetailsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Doctors = p.DoctorProcedures
                        .Where(dp => !dp.Doctor.IsDeleted)
                        .Select(dp => new DoctorForProcedureViewModel
                        {
                            Name = dp.Doctor.Name,
                            Specialization = dp.Doctor.Specialization,
                            YearsOfExperience = dp.Doctor.YearsOfExperience,
                            PhoneNumber = dp.Doctor.PhoneNumber,
                            ClinicName = dp.Doctor.AnimalClinic.Name
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return procedure;
        }

        public async Task<string?> GetProcedureNameByIdAsync(int procedureId)
        {
            return await _context.Procedures
                .Where(p => p.Id == procedureId && !p.IsDeleted)
                .Select(p => p.Name)
                .FirstOrDefaultAsync();
        }
    }
}
