using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<CreateAppointmentViewModel> BuildCreateAppointmentViewModelAsync(string userId, int? doctorId = null, int? procedureId = null)
        {
            var pets = await _context.Animals
                .Where(a => a.UserProfileId == userId && !a.IsDeleted)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Name} ({a.Species}, {a.Breed})"
                })
                .ToListAsync();

            IEnumerable<SelectListItem> procedures = Enumerable.Empty<SelectListItem>();
            IEnumerable<SelectListItem> doctors = Enumerable.Empty<SelectListItem>();

            if (doctorId.HasValue)
            {
                procedures = await _context.DoctorProcedures
                    .Where(dp => dp.DoctorId == doctorId.Value)
                    .Select(dp => new SelectListItem
                    {
                        Value = dp.ProcedureId.ToString(),
                        Text = dp.Procedure.Name
                    })
                    .ToListAsync();

                var doctor = await _context.Doctors
                    .Where(d => d.Id == doctorId.Value && !d.IsDeleted)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .FirstOrDefaultAsync();

                doctors = doctor != null ? new List<SelectListItem> { doctor } : Enumerable.Empty<SelectListItem>();
            }
            else if (procedureId.HasValue)
            {
                procedures = await _context.Procedures
                    .Where(p => p.Id == procedureId.Value && !p.IsDeleted)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();

                doctors = await _context.DoctorProcedures
                    .Where(dp => dp.ProcedureId == procedureId.Value && !dp.Doctor.IsDeleted)
                    .Select(dp => new SelectListItem
                    {
                        Value = dp.DoctorId.ToString(),
                        Text = dp.Doctor.Name
                    })
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                procedures = await _context.Procedures
                    .Where(p => !p.IsDeleted)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();
            }

            return new CreateAppointmentViewModel
            {
                UserPets = pets,
                Procedures = procedures,
                Doctors = doctors,
                DoctorId = doctorId ?? 0,
                ProcedureId = procedureId ?? 0,
                Date = DateTime.Today
            };
        }



        public async Task<List<SelectListItem>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            // 1. Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return new List<SelectListItem>
                    {
                        new SelectListItem { Text = "No available time slots", Value = "" }
                    };
            }

            // 2. Define standard working time slots (excluding lunch break)
            var workingSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30",
                "10:00", "10:30", "11:00", "11:30",
                "13:00", "13:30", "14:00", "14:30",
                "15:00", "15:30", "16:00", "16:30"
            };

            // 3. Remove past time slots if date is today
            if (date.Date == DateTime.Today)
            {
                var now = DateTime.Now;
                workingSlots = workingSlots
                    .Where(t =>
                    {
                        var slotTime = DateTime.ParseExact(t, "HH:mm", null);
                        return slotTime > now;
                    })
                    .ToList();
            }

            // 4. Get already booked time slots for that doctor on that date
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                            && !a.IsDeleted
                            && a.AppointmentDateTime.Date == date.Date)
                .Select(a => a.AppointmentDateTime.ToString("HH:mm"))
                .ToListAsync();

            // 5. Remove booked slots
            var availableSlots = workingSlots
                .Where(t => !bookedSlots.Contains(t))
                .Select(t => new SelectListItem { Value = t, Text = t })
                .ToList();

            // 6. Fallback if no available time
            if (!availableSlots.Any())
            {
                availableSlots.Add(new SelectListItem
                {
                    Text = "No available time slots",
                    Value = ""
                });
            }

            return availableSlots;
        }

        public async Task<bool> CreateAppointmentAsync(CreateAppointmentViewModel model, string userId)
        {
            // Check animal ownership
            var pet = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.AnimalId && a.UserProfileId == userId && !a.IsDeleted);

            if (pet == null)
                return false;

            // Check if the doctor can perform the procedure
            var doctorProcedure = await _context.DoctorProcedures
                .AnyAsync(dp => dp.DoctorId == model.DoctorId && dp.ProcedureId == model.ProcedureId);

            if (!doctorProcedure)
                return false;

            // Parse selected time slot to DateTime
            if (!TimeSpan.TryParse(model.TimeSlot, out var time))
                return false;

            var appointmentDateTime = model.Date.Date.Add(time);

            // Check if doctor is available at this time (not booked and not lunch time)
            bool isBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDateTime == appointmentDateTime &&
                !a.IsDeleted);

            if (isBooked || appointmentDateTime.TimeOfDay == TimeSpan.FromHours(12)) // 12:00-13:00 lunch time excluded elsewhere too
                return false;

            // Create appointment
            var appointment = new Appointment
            {
                AnimalId = model.AnimalId,
                ProcedureId = model.ProcedureId,
                DoctorId = model.DoctorId,
                AppointmentDateTime = appointmentDateTime,
                UserProfileId = userId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId, string requestingUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                    .ThenInclude(an => an.UserProfile)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Procedure)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Animal.IsDeleted || appointment.Animal.UserProfileId != requestingUserId)
            {
                return null; // Not found or access denied
            }

            return new AppointmentDetailsViewModel
            {
                // Pet & Owner
                OwnerFullName = appointment.Animal.UserProfile.FullName,
                PetName = appointment.Animal.Name,
                Species = appointment.Animal.Species,
                Breed = appointment.Animal.Breed,
                Age = appointment.Animal.Age,
                Gender = appointment.Animal.Gender.ToString(),

                // Doctor
                DoctorName = appointment.Doctor.Name,
                Specialization = appointment.Doctor.Specialization,
                YearsOfExperience = appointment.Doctor.YearsOfExperience,
                PhoneNumber = appointment.Doctor.PhoneNumber,
                ClinicName = appointment.Doctor.AnimalClinic.Name,
                ClinicAddress = appointment.Doctor.AnimalClinic.Address,

                // Procedure
                ProcedureName = appointment.Procedure.Name,
                ProcedureDescription = appointment.Procedure.Description
            };
        }

        public async Task<CancelAppointmentViewModel?> BuildCancelAppointmentViewModelAsync(int appointmentId, string userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted && a.Animal.UserProfileId == userId);

            if (appointment == null) return null;

            return new CancelAppointmentViewModel
            {
                AppointmentId = appointment.Id,
                PetName = appointment.Animal.Name,
                DoctorName = appointment.Doctor.Name,
                AppointmentTime = appointment.AppointmentDateTime
            };
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted && a.Animal.UserProfileId == userId);

            if (appointment == null) return false;

            appointment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
