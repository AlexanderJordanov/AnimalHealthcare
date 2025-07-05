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

        public async Task<DoctorListViewModel> GetDoctorsAsync(int page,int pageSize,string? sortBy,string? filterBy)
        {
            var query = _context.Doctors
                .Include(d => d.AnimalClinic)
                .Where(d => !d.IsDeleted);

            // Apply filtering
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

            // Apply sorting
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

            // Get filter options
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

        public async Task<DoctorDetailsViewModel?> GetDoctorDetailsAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.AnimalClinic)
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (doctor == null) return null;

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
                ClinicImageUrl = doctor.AnimalClinic.ImageUrl!
            };
        }

        public async Task<string?> GetDoctorNameByIdAsync(int doctorId)
        {
            return await _context.Doctors
                .Where(d => d.Id == doctorId && !d.IsDeleted)
                .Select(d => d.Name)
                .FirstOrDefaultAsync();
        }
    }
}
