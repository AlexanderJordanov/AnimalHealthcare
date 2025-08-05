using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        /// <summary>
        /// Retrieves all non-deleted appointments for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose appointments should be fetched.</param>
        /// <returns>A list of appointments represented as <see cref="MyAppointmentViewModel"/> objects.</returns>
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

        /// <summary>
        /// Builds the view model required to create an appointment, including dropdown options
        /// for pets, procedures, and doctors depending on which parameters are preselected.
        /// </summary>
        /// <param name="userId">The ID of the currently logged-in user.</param>
        /// <param name="doctorId">Optional: ID of a preselected doctor.</param>
        /// <param name="procedureId">Optional: ID of a preselected procedure.</param>
        /// <returns>A populated <see cref="CreateAppointmentViewModel"/>.</returns>
        public async Task<CreateAppointmentViewModel> BuildCreateAppointmentViewModelAsync(string userId, int? doctorId = null, int? procedureId = null)
        {
            var pets = await GetUserPetsAsync(userId);

            IEnumerable<SelectListItem> procedures = Enumerable.Empty<SelectListItem>();
            IEnumerable<SelectListItem> doctors = Enumerable.Empty<SelectListItem>();

            if (doctorId.HasValue && procedureId.HasValue)
            {
                procedures = await GetSelectedProcedureAsync(procedureId.Value);
                doctors = await GetSelectedDoctorAsync(doctorId.Value);
            }
            else if (doctorId.HasValue)
            {
                procedures = await GetProceduresByDoctorAsync(doctorId.Value);
                doctors = await GetSelectedDoctorAsync(doctorId.Value);
            }
            else if (procedureId.HasValue)
            {
                procedures = await GetSelectedProcedureAsync(procedureId.Value);
                doctors = await GetDoctorsByProcedureAsync(procedureId.Value);
            }
            else
            {
                procedures = await GetAllProceduresAsync();
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

        /// <summary>
        /// Retrieves available appointment time slots for a doctor on a given date,
        /// excluding weekends, lunch break, booked slots, and past times (if the date is today).
        /// </summary>
        /// <param name="doctorId">The ID of the doctor to check availability for.</param>
        /// <param name="date">The date for which time slots are requested.</param>
        /// <returns>A list of available time slots as <see cref="SelectListItem"/>.</returns>
        public async Task<List<SelectListItem>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Text = "No available time slots", Value = "" }
                };
            }

            if (date.Date < DateTime.Today)
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Text = "No available time slots", Value = "" }
                };
            }

            var workingSlots = new List<string>
                {
                    "08:00", "08:30", "09:00", "09:30",
                    "10:00", "10:30", "11:00", "11:30",
                    "13:00", "13:30", "14:00", "14:30",
                    "15:00", "15:30", "16:00", "16:30"
                };

            //if (date.Date == DateTime.Today)
            //{
            //    var now = DateTime.Now.TimeOfDay;
            //    workingSlots = workingSlots
            //        .Where(t => TimeSpan.Parse(t) > now)
            //        .ToList();
            //}

            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                    && !a.IsDeleted
                    && a.AppointmentDateTime >= date.Date
                    && a.AppointmentDateTime < date.Date.AddDays(1))
                .Select(a => a.AppointmentDateTime.ToString("HH:mm"))
                .ToListAsync();

            var availableSlots = workingSlots
                .Where(t => !bookedSlots.Contains(t))
                .Select(t => new SelectListItem { Value = t, Text = t })
                .ToList();

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

        /// <summary>
        /// Attempts to create an appointment for a user's pet with a doctor and procedure at a selected time.
        /// Performs ownership, availability, and validity checks before persisting the appointment.
        /// </summary>
        /// <param name="model">The view model containing appointment input data.</param>
        /// <param name="userId">The ID of the user attempting to create the appointment.</param>
        /// <returns>True if the appointment was created successfully, otherwise false.</returns>
        public async Task<AppointmentCreationResult> CreateAppointmentAsync(CreateAppointmentViewModel model, string userId)
        {
            var pet = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.AnimalId && a.UserProfileId == userId && !a.IsDeleted);
            if (pet == null)
                return AppointmentCreationResult.PetNotFound;

            var doctorProcedure = await _context.DoctorProcedures
                .AnyAsync(dp => dp.DoctorId == model.DoctorId && dp.ProcedureId == model.ProcedureId);
            if (!doctorProcedure)
                return AppointmentCreationResult.DoctorProcedureMismatch;

            if (!TimeSpan.TryParse(model.TimeSlot, out var time))
                return AppointmentCreationResult.InvalidTimeSlotFormat;

            var appointmentDateTime = model.Date.Date.Add(time);

            bool isBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDateTime == appointmentDateTime &&
                !a.IsDeleted);
            if (isBooked)
                return AppointmentCreationResult.SlotAlreadyBooked;

            if (appointmentDateTime.TimeOfDay == TimeSpan.FromHours(12))
                return AppointmentCreationResult.SlotDuringLunch;

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

            return AppointmentCreationResult.Success;
        }

        /// <summary>
        /// Retrieves detailed information for a specific appointment, including pet, doctor, clinic, and procedure data.
        /// Validates ownership to ensure the requesting user is authorized to view the appointment.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to retrieve.</param>
        /// <param name="requestingUserId">The ID of the user requesting the appointment details.</param>
        /// <returns>
        /// A populated <see cref="AppointmentDetailsViewModel"/> if found and owned by the user; otherwise, null.
        /// </returns>
        public async Task<AppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId, string requestingUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                    .ThenInclude(an => an.UserProfile)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Procedure)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted);

            if (appointment == null || appointment.Animal.IsDeleted || appointment.Animal.UserProfileId != requestingUserId)
            {
                return null; 
            }

            return new AppointmentDetailsViewModel
            {
                OwnerFullName = appointment.Animal.UserProfile.FullName,
                PetName = appointment.Animal.Name,
                Species = appointment.Animal.Species,
                Breed = appointment.Animal.Breed,
                Age = appointment.Animal.Age,
                Gender = appointment.Animal.Gender.ToString(),

                DoctorName = appointment.Doctor.Name,
                Specialization = appointment.Doctor.Specialization,
                YearsOfExperience = appointment.Doctor.YearsOfExperience,
                PhoneNumber = appointment.Doctor.PhoneNumber,
                ClinicName = appointment.Doctor.AnimalClinic.Name,
                ClinicAddress = appointment.Doctor.AnimalClinic.Address,

                ProcedureName = appointment.Procedure.Name,
                ProcedureDescription = appointment.Procedure.Description
            };
        }

        /// <summary>
        /// Builds a view model for canceling an appointment, ensuring it belongs to the requesting user.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to cancel.</param>
        /// <param name="userId">The ID of the user attempting the cancellation.</param>
        /// <returns>
        /// A <see cref="CancelAppointmentViewModel"/> with appointment details if valid; otherwise, null.
        /// </returns>
        public async Task<CancelAppointmentViewModel?> BuildCancelAppointmentViewModelAsync(int appointmentId, string userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    !a.IsDeleted &&
                    a.Animal.UserProfileId == userId); 

            if (appointment == null)
                return null;

            return new CancelAppointmentViewModel
            {
                AppointmentId = appointment.Id,
                PetName = appointment.Animal.Name,
                DoctorName = appointment.Doctor.Name,
                AppointmentTime = appointment.AppointmentDateTime
            };
        }

        /// <summary>
        /// Cancels an appointment by marking it as deleted, only if it belongs to the specified user.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to cancel.</param>
        /// <param name="userId">The ID of the user requesting the cancellation.</param>
        /// <returns>
        /// True if the cancellation succeeded; false if the appointment was not found or unauthorized.
        /// </returns>
        public async Task<ServiceOperationResult> CancelAppointmentAsync(int appointmentId, string userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    !a.IsDeleted &&
                    a.Animal.UserProfileId == userId);

            if (appointment == null)
            {
                var exists = await _context.Appointments.AnyAsync(a => a.Id == appointmentId && !a.IsDeleted);
                return exists ? ServiceOperationResult.Unauthorized : ServiceOperationResult.NotFound;
            }

            appointment.IsDeleted = true;

            await _context.SaveChangesAsync();

            return ServiceOperationResult.Success;
        }

        private async Task<List<SelectListItem>> GetUserPetsAsync(string userId)
        {
            return await _context.Animals
                .Where(a => a.UserProfileId == userId && !a.IsDeleted)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Name} ({a.Species}, {a.Breed})"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetSelectedDoctorAsync(int doctorId)
        {
            return await _context.Doctors
                .Where(d => d.Id == doctorId && !d.IsDeleted)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetSelectedProcedureAsync(int procedureId)
        {
            return await _context.Procedures
                .Where(p => p.Id == procedureId && !p.IsDeleted)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetProceduresByDoctorAsync(int doctorId)
        {
            return await _context.DoctorProcedures
                .Where(dp => dp.DoctorId == doctorId && !dp.Doctor.IsDeleted)
                .Select(dp => new SelectListItem
                {
                    Value = dp.ProcedureId.ToString(),
                    Text = dp.Procedure.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetDoctorsByProcedureAsync(int procedureId)
        {
            return await _context.DoctorProcedures
                .Where(dp => dp.ProcedureId == procedureId && !dp.Doctor.IsDeleted)
                .Select(dp => new SelectListItem
                {
                    Value = dp.DoctorId.ToString(),
                    Text = dp.Doctor.Name
                })
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetAllProceduresAsync()
        {
            return await _context.Procedures
                .Where(p => !p.IsDeleted)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }
    }
}
