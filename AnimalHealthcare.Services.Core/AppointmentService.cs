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

        /// <summary>
        /// Retrieves all non-deleted appointments for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose appointments should be fetched.</param>
        /// <returns>A list of appointments represented as <see cref="MyAppointmentViewModel"/> objects.</returns>
        public async Task<List<MyAppointmentViewModel>> GetAppointmentsByUserIdAsync(string userId)
        {
            return await _context.Appointments
                .Where(a => a.UserProfileId == userId && !a.IsDeleted) // Filter by user and exclude deleted appointments
                .Include(a => a.Animal)     // Include related animal (pet) data
                .Include(a => a.Procedure)  // Include related procedure data
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
            // Get all pets for the current user
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

            if (doctorId.HasValue && procedureId.HasValue)
            {
                // Case: both doctor and procedure are known
                procedures = await _context.Procedures
                    .Where(p => p.Id == procedureId.Value && !p.IsDeleted)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();

                doctors = await _context.Doctors
                    .Where(d => d.Id == doctorId.Value && !d.IsDeleted)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .ToListAsync();
            }
            else if (doctorId.HasValue)
            {
                // Case: only doctor is known — get the procedures they can perform
                procedures = await _context.DoctorProcedures
                    .Where(dp => dp.DoctorId == doctorId.Value && !dp.Doctor.IsDeleted)
                    .Select(dp => new SelectListItem
                    {
                        Value = dp.ProcedureId.ToString(),
                        Text = dp.Procedure.Name
                    })
                    .ToListAsync();

                doctors = await _context.Doctors
                    .Where(d => d.Id == doctorId.Value && !d.IsDeleted)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .ToListAsync();
            }
            else if (procedureId.HasValue)
            {
                // Case: only procedure is known — get doctors who can perform it
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
                // Default case: no preselection — load all available procedures
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

        /// <summary>
        /// Retrieves available appointment time slots for a doctor on a given date,
        /// excluding weekends, lunch break, booked slots, and past times (if the date is today).
        /// </summary>
        /// <param name="doctorId">The ID of the doctor to check availability for.</param>
        /// <param name="date">The date for which time slots are requested.</param>
        /// <returns>A list of available time slots as <see cref="SelectListItem"/>.</returns>
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

            // 2. Define standard working hours (excluding 12:00–13:00 lunch break)
            var workingSlots = new List<string>
                {
                    "08:00", "08:30", "09:00", "09:30",
                    "10:00", "10:30", "11:00", "11:30",
                    "13:00", "13:30", "14:00", "14:30",
                    "15:00", "15:30", "16:00", "16:30"
                };

            // 3. Exclude past time slots if the selected date is today
            if (date.Date == DateTime.Today)
            {
                var now = DateTime.Now.TimeOfDay;
                workingSlots = workingSlots
                    .Where(t => TimeSpan.Parse(t) > now)
                    .ToList();
            }

            // 4. Fetch already booked appointment times for that doctor on the selected date
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                    && !a.IsDeleted
                    && a.AppointmentDateTime >= date.Date
                    && a.AppointmentDateTime < date.Date.AddDays(1))
                .Select(a => a.AppointmentDateTime.ToString("HH:mm"))
                .ToListAsync();

            // 5. Filter out booked slots from working slots
            var availableSlots = workingSlots
                .Where(t => !bookedSlots.Contains(t))
                .Select(t => new SelectListItem { Value = t, Text = t })
                .ToList();

            // 6. Handle case where no time slots are left
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
        public async Task<bool> CreateAppointmentAsync(CreateAppointmentViewModel model, string userId)
        {
            // 1. Validate that the selected pet belongs to the current user
            var pet = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == model.AnimalId && a.UserProfileId == userId && !a.IsDeleted);

            if (pet == null)
                return false;

            // 2. Validate that the selected doctor can perform the selected procedure
            var doctorProcedure = await _context.DoctorProcedures
                .AnyAsync(dp => dp.DoctorId == model.DoctorId && dp.ProcedureId == model.ProcedureId);

            if (!doctorProcedure)
                return false;

            // 3. Validate that the time slot input is in correct format (e.g., "08:30")
            if (!TimeSpan.TryParse(model.TimeSlot, out var time))
                return false;

            // 4. Combine selected date and parsed time to form appointment datetime
            var appointmentDateTime = model.Date.Date.Add(time);

            // 5. Check if the selected slot is already booked or falls within lunch break
            bool isBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDateTime == appointmentDateTime &&
                !a.IsDeleted);

            if (isBooked || appointmentDateTime.TimeOfDay == TimeSpan.FromHours(12)) // Exclude 12:00 slot (lunch)
                return false;

            // 6. Create and save the appointment to the database
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
            // 1. Load the appointment and its related entities
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                    .ThenInclude(an => an.UserProfile)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.AnimalClinic)
                .Include(a => a.Procedure)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            // 2. Check if appointment exists, is not deleted, and belongs to the user
            if (appointment == null || appointment.Animal.IsDeleted || appointment.Animal.UserProfileId != requestingUserId)
            {
                return null; // Appointment doesn't exist or access is denied
            }

            // 3. Map appointment data to view model
            return new AppointmentDetailsViewModel
            {
                // Pet & Owner
                OwnerFullName = appointment.Animal.UserProfile.FullName,
                PetName = appointment.Animal.Name,
                Species = appointment.Animal.Species,
                Breed = appointment.Animal.Breed,
                Age = appointment.Animal.Age,
                Gender = appointment.Animal.Gender.ToString(),

                // Doctor Info
                DoctorName = appointment.Doctor.Name,
                Specialization = appointment.Doctor.Specialization,
                YearsOfExperience = appointment.Doctor.YearsOfExperience,
                PhoneNumber = appointment.Doctor.PhoneNumber,
                ClinicName = appointment.Doctor.AnimalClinic.Name,
                ClinicAddress = appointment.Doctor.AnimalClinic.Address,

                // Procedure Info
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
            // 1. Load the appointment and related entities
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    !a.IsDeleted &&
                    a.Animal.UserProfileId == userId); // Ensure ownership

            // 2. If not found or not owned, return null
            if (appointment == null)
                return null;

            // 3. Map appointment info to view model for cancellation confirmation
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
        public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
        {
            // 1. Load the appointment, ensuring it belongs to the requesting user and isn't already deleted
            var appointment = await _context.Appointments
                .Include(a => a.Animal)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    !a.IsDeleted &&
                    a.Animal.UserProfileId == userId);

            // 2. If appointment not found or unauthorized, return false
            if (appointment == null)
                return false;

            // 3. Mark the appointment as logically deleted (soft delete)
            appointment.IsDeleted = true;

            // 4. Persist changes to the database
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
