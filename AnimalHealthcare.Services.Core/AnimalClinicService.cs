using AnimalHealthcare.Data;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.AnimalClinic;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Services.Core
{
    public class AnimalClinicService : IAnimalClinicService
    {
        private readonly AnimalHealthcareDbContext _context;

        public AnimalClinicService(AnimalHealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<AnimalClinicListViewModel> GetAllClinicsAsync()
        {
            var clinics = await _context.AnimalClinics
                .Select(c => new AnimalClinicListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    ImageUrl = c.ImageUrl!
                })
                .ToListAsync();

            return new AnimalClinicListViewModel
            {
                Clinics = clinics
            };
        }

        public async Task<AnimalClinicDetailsViewModel?> GetClinicDetailsAsync(int id)
        {
            var clinic = await _context.AnimalClinics
                .Where(c => c.Id == id)
                .Select(c => new AnimalClinicDetailsViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    ImageUrl = c.ImageUrl,
                    Doctors = c.Doctors
                        .Where(d => !d.IsDeleted)
                        .Select(d => new AnimalClinicDoctorViewModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Specialization = d.Specialization,
                            ProfileImageUrl = d.ImageUrl!,
                            YearsOfExperience = d.YearsOfExperience,
                            PhoneNumber = d.PhoneNumber
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return clinic;
        }
    }
}
