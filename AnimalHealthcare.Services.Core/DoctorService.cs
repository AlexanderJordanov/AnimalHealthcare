using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Doctor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class DoctorService : IDoctorService
    {       
        private readonly AnimalHealthcareDbContext _context;

        public DoctorService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of doctors who are able to perform the specified procedure.
        /// </summary>
        /// <param name="procedureId">The ID of the procedure.</param>
        /// <returns>A list of doctors formatted as <see cref="SelectListItem"/> for use in dropdowns.</returns>
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

        /// <summary>
        /// Retrieves a paginated and optionally filtered/sorted list of doctors.
        /// </summary>
        /// <param name="page">The current page number (1-based).</param>
        /// <param name="pageSize">The number of doctors per page.</param>
        /// <param name="sortBy">The sorting criteria ("speciality", "clinic", or default by name).</param>
        /// <param name="filterBy">The filter value to apply based on sortBy.</param>
        /// <returns>A <see cref="DoctorListViewModel"/> containing doctor items, paging info, and filter options.</returns>
        public async Task<DoctorListViewModel> GetDoctorsAsync(int page, int pageSize, string? sortBy, string? filterBy)
        {
            var query = _context.Doctors
                .Include(d => d.AnimalClinic)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(filterBy))
            {
                if (sortBy == "speciality")
                {
                    query = query.Where(d => d.Specialization == filterBy);
                }
                else if (sortBy == "clinic")
                {
                    query = query.Where(d => d.AnimalClinic.Name == filterBy);
                }
            }

            query = sortBy switch
            {
                "speciality" => query.OrderBy(d => d.Specialization),
                "clinic" => query.OrderBy(d => d.AnimalClinic.Name),
                _ => query.OrderBy(d => d.Name)
            };

            var totalDoctors = await query.CountAsync();

            var doctors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var filters = sortBy switch
            {
                "speciality" => await _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.Specialization)
                    .Distinct()
                    .OrderBy(s => s)
                    .Select(s => new SelectListItem { Value = s, Text = s })
                    .ToListAsync(),

                "clinic" => await _context.AnimalClinics
                    .Select(c => new SelectListItem { Value = c.Name, Text = c.Name })
                    .ToListAsync(),

                _ => new List<SelectListItem>()
            };

            return new DoctorListViewModel
            {
                Doctors = doctors.Select(d => new DoctorListItemViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specialization = d.Specialization,
                    ClinicName = d.AnimalClinic.Name,
                    ProfileImageUrl = d.ImageUrl
                }),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalDoctors / pageSize),
                CurrentSort = sortBy,
                CurrentFilter = filterBy,
                AvailableFilters = filters
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific doctor, including their clinic and procedures they can perform.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor to retrieve.</param>
        /// <returns>
        /// A <see cref="DoctorDetailsViewModel"/> containing doctor and clinic details, or null if the doctor doesn't exist or is deleted.
        /// </returns>
        public async Task<DoctorDetailsViewModel?> GetDoctorDetailsAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.AnimalClinic)
                .Include(d => d.DoctorProcedures)
                    .ThenInclude(dp => dp.Procedure)
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (doctor == null) return null;

            var procedures = doctor.DoctorProcedures
                .Select(dp => dp.Procedure.Name)
                .ToList();

            return new DoctorDetailsViewModel
            {
                Name = doctor.Name,
                Specialization = doctor.Specialization,
                YearsOfExperience = doctor.YearsOfExperience,
                PhoneNumber = doctor.PhoneNumber,
                ProfileImageUrl = doctor.ImageUrl,

                ClinicName = doctor.AnimalClinic.Name,
                ClinicAddress = doctor.AnimalClinic.Address,
                ClinicPhoneNumber = doctor.AnimalClinic.PhoneNumber,
                ClinicImageUrl = doctor.AnimalClinic.ImageUrl!,

                ProceduresPerformed = string.Join(", ", procedures)
            };
        }

        /// <summary>
        /// Retrieves the name of a doctor by their ID.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor.</param>
        /// <returns>
        /// The name of the doctor if found and not deleted; otherwise, null.
        /// </returns>
        public async Task<string?> GetDoctorNameByIdAsync(int doctorId)
        {
            return await _context.Doctors
                .Where(d => d.Id == doctorId && !d.IsDeleted)
                .Select(d => d.Name)
                .FirstOrDefaultAsync();
        }
    }
}
