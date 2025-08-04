using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class AppointmentServiceTests
    {
        private AnimalHealthcareDbContext _context = null!;
        private AppointmentService _service = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase("AppointmentServiceTestDb_" + Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            // Fixed base date for appointments
            var baseDate = new DateTime(2025, 8, 4);

            // Seed Users
            var user1 = new UserProfile
            {
                Id = "user-1",
                FullName = "User One"
            };
            var user2 = new UserProfile
            {
                Id = "user-2",
                FullName = "User Two"
            };

            // Seed Animals with required fields populated
            var animal1 = new Animal
            {
                Id = 1,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                UserProfileId = "user-1",
                IsDeleted = false
            };

            var animal2 = new Animal
            {
                Id = 2,
                Name = "Milo",
                Species = "Cat",
                Breed = "Siamese",
                UserProfileId = "user-2",
                IsDeleted = false
            };

            // Seed Procedures
            var procedure1 = new Procedure
            {
                Id = 1,
                Name = "Vaccination",
                Description = "Routine pet vaccination to prevent diseases.",
                IsDeleted = false
            };

            var procedure2 = new Procedure
            {
                Id = 2,
                Name = "Dental Cleaning",
                Description = "Professional cleaning of pet's teeth.",
                IsDeleted = false
            };

            // Seed Doctors
            var doctor1 = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Specialization = "Surgery",
                PhoneNumber = "555-1234",
                ImageUrl = "drsmith.jpg",
                YearsOfExperience = 10,
                IsDeleted = false,
                AnimalClinicId = 1
            };

            var doctor2 = new Doctor
            {
                Id = 2,
                Name = "Dr. Jones",
                Specialization = "Dentistry",
                PhoneNumber = "555-5678",
                ImageUrl = "drjones.jpg",
                YearsOfExperience = 5,
                IsDeleted = false,
                AnimalClinicId = 1
            };

            var doctorProcedures = new List<DoctorProcedure>
            {
                new DoctorProcedure { DoctorId = 1, ProcedureId = 1 },
                new DoctorProcedure { DoctorId = 2, ProcedureId = 2 }
            };

            // Seed Appointments with fixed dates
            var appointment1 = new Appointment
            {
                Id = 1,
                UserProfileId = "user-1",
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = baseDate.Date.AddHours(8),
                IsDeleted = false
            };

            var appointment2 = new Appointment
            {
                Id = 2,
                UserProfileId = "user-1",
                AnimalId = 1,
                DoctorId = 2,
                ProcedureId = 2,
                AppointmentDateTime = baseDate.Date.AddDays(1).AddHours(9),
                IsDeleted = true // Deleted appointment, should be excluded
            };

            var appointment3 = new Appointment
            {
                Id = 3,
                UserProfileId = "user-2",
                AnimalId = 2,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = baseDate.Date.AddDays(2).AddHours(10),
                IsDeleted = false
            };

            var animalClinic = new AnimalClinic
            {
                Id = 1,
                Name = "Happy Pets Clinic",
                Address = "123 Pet Street",
                PhoneNumber = "555-1234",
                ImageUrl = "clinic.jpg"
            };

            await _context.UserProfiles.AddRangeAsync(user1, user2);
            await _context.Animals.AddRangeAsync(animal1, animal2);
            await _context.Doctors.AddRangeAsync(doctor1, doctor2);
            await _context.Procedures.AddRangeAsync(procedure1, procedure2);
            await _context.DoctorProcedures.AddRangeAsync(doctorProcedures);
            await _context.AnimalClinics.AddAsync(animalClinic);

            await _context.Appointments.AddRangeAsync(appointment1, appointment2, appointment3);
            await _context.SaveChangesAsync();

            _service = new AppointmentService(_context);
        }



        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetAppointmentsByUserIdAsync_ReturnsOnlyNonDeletedAppointmentsForUser()
        {
            // Arrange
            var expectedDate = new DateTime(2025, 8, 4); // matches setup

            // Act
            var result = await _service.GetAppointmentsByUserIdAsync("user-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1)); // Only one non-deleted appointment for user-1

            var appointment = result.First();
            Assert.That(appointment.Id, Is.EqualTo(1));
            Assert.That(appointment.PetName, Is.EqualTo("Buddy"));
            Assert.That(appointment.ProcedureName, Is.EqualTo("Vaccination"));
            Assert.That(appointment.AppointmentDateTime.Date, Is.EqualTo(expectedDate));
        }


        [Test]
        public async Task GetAppointmentsByUserIdAsync_UserWithNoAppointments_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAppointmentsByUserIdAsync("user-without-appointments");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAppointmentsByUserIdAsync_InvalidUserId_ReturnsEmptyList()
        {
            // Act
            var result = await _service.GetAppointmentsByUserIdAsync("invalid-user-id");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task BuildCreateAppointmentViewModelAsync_NoDoctorOrProcedureId_LoadsAllProceduresAndNoDoctors()
        {
            // Arrange
            var userId = "user-1";

            // Act
            var model = await _service.BuildCreateAppointmentViewModelAsync(userId);

            // Assert
            Assert.IsNotNull(model);
            Assert.That(model.UserPets, Is.Not.Empty, "Pets dropdown should be populated");
            Assert.That(model.Procedures, Is.Not.Empty, "Procedures dropdown should have all procedures");
            Assert.That(model.Doctors, Is.Empty, "Doctors dropdown should be empty");
            Assert.That(model.DoctorId, Is.EqualTo(0));
            Assert.That(model.ProcedureId, Is.EqualTo(0));
            Assert.That(model.Date.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task BuildCreateAppointmentViewModelAsync_DoctorIdAndProcedureIdProvided_LoadsSelectedProcedureAndDoctorOnly()
        {
            // Arrange
            var userId = "user-1";
            int doctorId = 1;
            int procedureId = 1;

            // Act
            var model = await _service.BuildCreateAppointmentViewModelAsync(userId, doctorId, procedureId);

            // Assert
            Assert.IsNotNull(model);
            Assert.That(model.UserPets, Is.Not.Empty, "Pets dropdown should be populated");

            Assert.That(model.Procedures.Count(), Is.EqualTo(1), "Procedures dropdown should have exactly 1 selected procedure");
            Assert.That(model.Procedures.First().Value, Is.EqualTo(procedureId.ToString()));

            Assert.That(model.Doctors.Count(), Is.EqualTo(1), "Doctors dropdown should have exactly 1 selected doctor");
            Assert.That(model.Doctors.First().Value, Is.EqualTo(doctorId.ToString()));

            Assert.That(model.DoctorId, Is.EqualTo(doctorId));
            Assert.That(model.ProcedureId, Is.EqualTo(procedureId));
            Assert.That(model.Date.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task BuildCreateAppointmentViewModelAsync_DoctorIdOnly_LoadsProceduresForDoctorAndSelectedDoctor()
        {
            // Arrange
            var userId = "user-1";
            int doctorId = 1;

            // Act
            var model = await _service.BuildCreateAppointmentViewModelAsync(userId, doctorId, null);

            // Assert
            Assert.IsNotNull(model);
            Assert.That(model.UserPets, Is.Not.Empty);

            Assert.That(model.Procedures, Is.Not.Empty, "Procedures dropdown should contain procedures for the doctor");
            Assert.That(model.Doctors.Count(), Is.EqualTo(1), "Doctors dropdown should have exactly 1 selected doctor");
            Assert.That(model.Doctors.First().Value, Is.EqualTo(doctorId.ToString()));

            Assert.That(model.DoctorId, Is.EqualTo(doctorId));
            Assert.That(model.ProcedureId, Is.EqualTo(0));
            Assert.That(model.Date.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task BuildCreateAppointmentViewModelAsync_ProcedureIdOnly_LoadsSelectedProcedureAndDoctorsForProcedure()
        {
            // Arrange
            var userId = "user-1";
            int procedureId = 1;

            // Act
            var model = await _service.BuildCreateAppointmentViewModelAsync(userId, null, procedureId);

            // Assert
            Assert.IsNotNull(model);
            Assert.That(model.UserPets, Is.Not.Empty);

            Assert.That(model.Procedures.Count(), Is.EqualTo(1), "Procedures dropdown should have exactly 1 selected procedure");
            Assert.That(model.Procedures.First().Value, Is.EqualTo(procedureId.ToString()));

            Assert.That(model.Doctors, Is.Not.Empty, "Doctors dropdown should have doctors for the procedure");

            Assert.That(model.DoctorId, Is.EqualTo(0));
            Assert.That(model.ProcedureId, Is.EqualTo(procedureId));
            Assert.That(model.Date.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task BuildCreateAppointmentViewModelAsync_UserHasNoPets_ReturnsEmptyUserPets()
        {
            // Arrange
            var userId = "no-pets-user";

            // Act
            var model = await _service.BuildCreateAppointmentViewModelAsync(userId);

            // Assert
            Assert.IsNotNull(model);
            Assert.IsEmpty(model.UserPets, "UserPets should be empty when user has no pets");
            Assert.That(model.Procedures, Is.Not.Empty, "Procedures dropdown should still be populated");
            Assert.That(model.Doctors, Is.Empty, "Doctors dropdown should be empty by default");
            Assert.That(model.DoctorId, Is.EqualTo(0));
            Assert.That(model.ProcedureId, Is.EqualTo(0));
            Assert.That(model.Date.Date, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task GetAvailableTimeSlotsAsync_WeekendDate_ReturnsNoSlotsMessage()
        {
            // Arrange
            var saturday = new DateTime(2023, 8, 5); // Saturday

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(1, saturday);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Text, Is.EqualTo("No available time slots"));
        }

        [Test]
        public async Task GetAvailableTimeSlotsAsync_WeekdayNoBookings_ReturnsAllWorkingSlots()
        {
            // Arrange
            var date = new DateTime(2025, 8, 5); // Day after seeded appointment, no bookings expected

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(1, date);

            // Assert
            var expectedSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30",
                "10:00", "10:30", "11:00", "11:30",
                "13:00", "13:30", "14:00", "14:30",
                "15:00", "15:30", "16:00", "16:30"
            };

            Assert.That(result.Count, Is.EqualTo(expectedSlots.Count));

            foreach (var slot in expectedSlots)
            {
                Assert.That(result.Any(r => r.Text == slot), Is.True, $"Missing slot {slot}");
            }
        }

        [Test]
        public async Task GetAvailableTimeSlotsAsync_WeekdayWithBookings_ExcludesBookedSlots()
        {
            // Arrange
            var date = new DateTime(2025, 8, 4); // Matches appointment1 date in setup

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(1, date);

            // Assert
            Assert.That(result.Any(slot => slot.Text == "08:00"), Is.False);  // Slot is booked
            Assert.That(result.Any(slot => slot.Text == "08:30"), Is.True);   // Next slot should be free

            var expectedSlots = new List<string>
    {
        "08:30", "09:00", "09:30",
        "10:00", "10:30", "11:00", "11:30",
        "13:00", "13:30", "14:00", "14:30",
        "15:00", "15:30", "16:00", "16:30"
    };

            Assert.That(result.Count, Is.EqualTo(expectedSlots.Count));

            foreach (var slot in expectedSlots)
            {
                Assert.That(result.Any(r => r.Text == slot), Is.True, $"Missing slot {slot}");
            }
        }




        [Test]
        public async Task GetAvailableTimeSlotsAsync_InvalidDoctorId_ReturnsAllWorkingSlots()
        {
            // Arrange
            var invalidDoctorId = 999;
            var date = DateTime.Today.AddDays(1);

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(invalidDoctorId, date);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result.Any(s => s.Text == "08:00"));
            Assert.That(result.Any(s => s.Text == "16:30"));
        }

        [Test]
        public async Task GetAvailableTimeSlotsAsync_DateInPast_ReturnsNoAvailableSlots()
        {
            // Arrange
            var doctorId = 1;
            var pastDate = DateTime.Today.AddDays(-1);

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(doctorId, pastDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Text, Is.EqualTo("No available time slots"));
            Assert.That(result[0].Value, Is.EqualTo(""));
        }



        [Test]
        public async Task GetAvailableTimeSlotsAsync_AllSlotsBooked_ReturnsNoAvailableSlots()
        {
            // Arrange
            var doctorId = 1;
            var date = DateTime.Today.AddDays(1);

            // Seed all slots booked for the doctor on that date
            var workingSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30",
                "10:00", "10:30", "11:00", "11:30",
                "13:00", "13:30", "14:00", "14:30",
                "15:00", "15:30", "16:00", "16:30"
            };

            foreach (var slot in workingSlots)
            {
                var time = TimeSpan.Parse(slot);
                var appointmentDateTime = date.Date.Add(time);
                _context.Appointments.Add(new Appointment
                {
                    DoctorId = doctorId,
                    AppointmentDateTime = appointmentDateTime,
                    IsDeleted = false,
                    AnimalId = 1, 
                    ProcedureId = 1, 
                    UserProfileId = "user-1"
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAvailableTimeSlotsAsync(doctorId, date);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Text, Is.EqualTo("No available time slots"));
        }

        [Test]
        public async Task CreateAppointmentAsync_ValidInput_CreatesAppointmentAndReturnsSuccess()
        {
            // Arrange
            var userId = "user-1";
            var futureDate = DateTime.Today.AddDays(10);
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                Date = futureDate,
                TimeSlot = "08:00"
            };

            // Act
            var result = await _service.CreateAppointmentAsync(model, userId);

            // Assert
            Assert.That(result, Is.EqualTo(AppointmentCreationResult.Success));

            var createdAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AnimalId == model.AnimalId &&
                    a.DoctorId == model.DoctorId &&
                    a.ProcedureId == model.ProcedureId &&
                    a.UserProfileId == userId &&
                    a.AppointmentDateTime == model.Date.Date.Add(TimeSpan.Parse(model.TimeSlot)) &&
                    !a.IsDeleted);

            Assert.That(createdAppointment, Is.Not.Null);
        }

        [Test]
        public async Task CreateAppointmentAsync_PetNotFound_ReturnsPetNotFound()
        {
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 999, // Non-existent pet
                DoctorId = 1,
                ProcedureId = 1,
                TimeSlot = "08:00",
                Date = DateTime.Today.AddDays(1)
            };
            var userId = "user-1";

            var result = await _service.CreateAppointmentAsync(model, userId);

            Assert.That(result, Is.EqualTo(AppointmentCreationResult.PetNotFound));
        }

        [Test]
        public async Task CreateAppointmentAsync_DoctorProcedureMismatch_ReturnsDoctorProcedureMismatch()
        {
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 2, // Procedure not done by doctor 1
                TimeSlot = "08:00",
                Date = DateTime.Today.AddDays(1)
            };
            var userId = "user-1";

            var result = await _service.CreateAppointmentAsync(model, userId);

            Assert.That(result, Is.EqualTo(AppointmentCreationResult.DoctorProcedureMismatch));
        }

        [Test]
        public async Task CreateAppointmentAsync_InvalidTimeSlotFormat_ReturnsInvalidTimeSlotFormat()
        {
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                TimeSlot = "invalid-time",
                Date = DateTime.Today.AddDays(1)
            };
            var userId = "user-1";

            var result = await _service.CreateAppointmentAsync(model, userId);

            Assert.That(result, Is.EqualTo(AppointmentCreationResult.InvalidTimeSlotFormat));
        }

        [Test]
        public async Task CreateAppointmentAsync_SlotAlreadyBooked_ReturnsSlotAlreadyBooked()
        {
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                TimeSlot = "08:00", // Already booked in setup
                Date = new DateTime(2025, 8, 4) // Fixed base date from setup
            };
            var userId = "user-1";

            var result = await _service.CreateAppointmentAsync(model, userId);

            Assert.That(result, Is.EqualTo(AppointmentCreationResult.SlotAlreadyBooked));
        }

        [Test]
        public async Task CreateAppointmentAsync_SlotDuringLunch_ReturnsSlotDuringLunch()
        {
            var model = new CreateAppointmentViewModel
            {
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                TimeSlot = "12:00", // Lunch break
                Date = DateTime.Today.AddDays(1)
            };
            var userId = "user-1";

            var result = await _service.CreateAppointmentAsync(model, userId);

            Assert.That(result, Is.EqualTo(AppointmentCreationResult.SlotDuringLunch));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_ValidAppointmentOwnedByUser_ReturnsDetails()
        {
            // Arrange
            var appointmentId = 1; // seeded appointment owned by user-1
            var userId = "user-1";

            // Act
            var result = await _service.GetAppointmentDetailsAsync(appointmentId, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.PetName, Is.EqualTo("Buddy"));
            Assert.That(result.DoctorName, Is.EqualTo("Dr. Smith"));
            Assert.That(result.ClinicName, Is.EqualTo("Happy Pets Clinic"));
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_AppointmentDoesNotExist_ReturnsNull()
        {
            // Arrange
            var appointmentId = 999; // non-existent
            var userId = "user-1";

            // Act
            var result = await _service.GetAppointmentDetailsAsync(appointmentId, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_AppointmentDeleted_ReturnsNull()
        {
            // Arrange
            var appointment = await _context.Appointments.FirstAsync(a => a.IsDeleted == true);
            var userId = appointment.UserProfileId;

            // Act
            var result = await _service.GetAppointmentDetailsAsync(appointment.Id, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_AppointmentBelongsToDifferentUser_ReturnsNull()
        {
            // Arrange
            var appointmentId = 1; // owned by user-1
            var userId = "user-2"; // different user

            // Act
            var result = await _service.GetAppointmentDetailsAsync(appointmentId, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_AppointmentWithDeletedAnimal_ReturnsNull()
        {
            // Arrange
            // Create appointment with deleted animal
            var deletedAnimal = new Animal
            {
                Id = 999,
                Name = "Ghost",
                Species = "Cat",
                Breed = "Unknown",
                UserProfileId = "user-1",
                IsDeleted = true
            };
            await _context.Animals.AddAsync(deletedAnimal);
            await _context.SaveChangesAsync();

            var appointmentWithDeletedAnimal = new Appointment
            {
                Id = 999,
                UserProfileId = "user-1",
                AnimalId = 999,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                IsDeleted = false
            };
            await _context.Appointments.AddAsync(appointmentWithDeletedAnimal);
            await _context.SaveChangesAsync();

            var userId = "user-1";

            // Act
            var result = await _service.GetAppointmentDetailsAsync(999, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildCancelAppointmentViewModelAsync_ValidAppointmentAndUser_ReturnsViewModel()
        {
            // Arrange
            int appointmentId = 1;
            string userId = "user-1";

            // Act
            var result = await _service.BuildCancelAppointmentViewModelAsync(appointmentId, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.AppointmentId, Is.EqualTo(appointmentId));
            Assert.That(result.PetName, Is.Not.Null.And.Not.Empty);
            Assert.That(result.DoctorName, Is.Not.Null.And.Not.Empty);
            Assert.That(result.AppointmentTime, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task BuildCancelAppointmentViewModelAsync_InvalidAppointmentId_ReturnsNull()
        {
            // Arrange
            int invalidAppointmentId = 999;
            string userId = "user-1";

            // Act
            var result = await _service.BuildCancelAppointmentViewModelAsync(invalidAppointmentId, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildCancelAppointmentViewModelAsync_AppointmentOwnedByAnotherUser_ReturnsNull()
        {
            // Arrange
            int appointmentId = 1; // Owned by user-1
            string otherUserId = "user-2";

            // Act
            var result = await _service.BuildCancelAppointmentViewModelAsync(appointmentId, otherUserId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildCancelAppointmentViewModelAsync_DeletedAppointment_ReturnsNull()
        {
            // Arrange
            int deletedAppointmentId = 2; // This is marked as deleted in your setup
            string userId = "user-1";

            // Act
            var result = await _service.BuildCancelAppointmentViewModelAsync(deletedAppointmentId, userId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task CancelAppointmentAsync_ValidAppointmentAndUser_Success()
        {
            // Arrange
            int appointmentId = 1;
            string userId = "user-1";

            // Act
            var result = await _service.CancelAppointmentAsync(appointmentId, userId);

            // Assert
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.That(appointment!.IsDeleted, Is.True);
        }

        [Test]
        public async Task CancelAppointmentAsync_AppointmentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            int appointmentId = 999; // Non-existent ID
            string userId = "user-1";

            // Act
            var result = await _service.CancelAppointmentAsync(appointmentId, userId);

            // Assert
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task CancelAppointmentAsync_AppointmentNotOwnedByUser_ReturnsUnauthorized()
        {
            // Arrange
            int appointmentId = 3; // Owned by user-2
            string userId = "user-1"; // Different user

            // Act
            var result = await _service.CancelAppointmentAsync(appointmentId, userId);

            // Assert
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.That(appointment!.IsDeleted, Is.False);
        }

        [Test]
        public async Task CancelAppointmentAsync_AlreadyDeletedAppointment_ReturnsNotFound()
        {
            // Arrange
            // Add a deleted appointment for testing
            var deletedAppointment = new Appointment
            {
                Id = 100,
                UserProfileId = "user-1",
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                IsDeleted = true
            };
            await _context.Appointments.AddAsync(deletedAppointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CancelAppointmentAsync(deletedAppointment.Id, "user-1");

            // Assert
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }
    }
}


