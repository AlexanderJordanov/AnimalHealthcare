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

        /// <summary>
        /// Retrieves all available procedures that are not marked as deleted.
        /// </summary>
        /// <returns>
        /// A list of procedure view models containing ID and name for each procedure.
        /// </returns>
        public async Task<IEnumerable<ProcedureListItemViewModel>> GetAllProceduresAsync()
        {
            return await _context.Procedures
                .Where(p => !p.IsDeleted) // Filter out soft-deleted procedures
                .Select(p => new ProcedureListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync(); // Execute the query and return results
        }

        /// <summary>
        /// Retrieves detailed information about a specific procedure, including its description
        /// and the list of doctors qualified to perform it.
        /// </summary>
        /// <param name="procedureId">The ID of the procedure to retrieve.</param>
        /// <returns>
        /// A ProcedureDetailsViewModel containing procedure info and associated doctors,
        /// or null if the procedure doesn't exist or is deleted.
        /// </returns>
        public async Task<ProcedureDetailsViewModel?> GetProcedureDetailsAsync(int procedureId)
        {
            var procedure = await _context.Procedures
                .Where(p => p.Id == procedureId && !p.IsDeleted) // Filter by ID and exclude deleted
                .Select(p => new ProcedureDetailsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Doctors = p.DoctorProcedures
                        .Where(dp => !dp.Doctor.IsDeleted) // Only include active doctors
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
                .FirstOrDefaultAsync(); // Return null if no match found

            return procedure;
        }

        /// <summary>
        /// Retrieves the name of a procedure by its ID.
        /// </summary>
        /// <param name="procedureId">The ID of the procedure.</param>
        /// <returns>
        /// The name of the procedure if found and not deleted; otherwise, null.
        /// </returns>
        public async Task<string?> GetProcedureNameByIdAsync(int procedureId)
        {
            return await _context.Procedures
                .Where(p => p.Id == procedureId && !p.IsDeleted) // Only consider non-deleted procedures
                .Select(p => p.Name)
                .FirstOrDefaultAsync(); // Return the name or null if not found
        }
    }
}
