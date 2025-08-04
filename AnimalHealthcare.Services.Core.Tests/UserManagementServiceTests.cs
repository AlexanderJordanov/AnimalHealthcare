using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class UserManagementServiceTests
    {
        private AnimalHealthcareDbContext _context = null!;
        private Mock<UserManager<IdentityUser>> _userManagerMock = null!;
        private UserManagementService _service = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase("UserManagementServiceTestDb_" + Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            // Seed IdentityUsers
            var identityUser1 = new IdentityUser { Id = "user-1", UserName = "user1@example.com", Email = "user1@example.com" };
            var identityUser2 = new IdentityUser { Id = "user-2", UserName = "user2@example.com", Email = "user2@example.com" };

            // Seed UserProfiles
            var userProfile1 = new UserProfile
            {
                Id = "user-1",
                FullName = "User One",
                PhoneNumber = "1234567890",
                Address = "123 Main St",
                User = identityUser1
            };
            var userProfile2 = new UserProfile
            {
                Id = "user-2",
                FullName = "User Two",
                PhoneNumber = null,
                Address = null,
                User = identityUser2
            };

            await _context.UserProfiles.AddRangeAsync(userProfile1, userProfile2);

            // Seed Animals for user-1 (one deleted, one active)
            var animal1 = new Animal
            {
                Id = 1,
                Name = "Bella",
                Species = "Dog",
                Breed = "Labrador",
                UserProfileId = "user-1",
                IsDeleted = false
            };
            var animal2 = new Animal
            {
                Id = 2,
                Name = "Max",
                Species = "Cat",
                Breed = "Siamese",
                UserProfileId = "user-1",
                IsDeleted = true
            };
            await _context.Animals.AddRangeAsync(animal1, animal2);

            // Seed Doctors and Clinics (if needed for appointments)
            var clinic = new AnimalClinic
            {
                Id = 1,
                Name = "Happy Pets Clinic",
                Address = "123 Pet Street",
                PhoneNumber = "555-1234",
                ImageUrl = "clinic.jpg"
            };
            var doctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Specialization = "Surgery",
                PhoneNumber = "555-6789",
                ImageUrl = "drsmith.jpg",
                YearsOfExperience = 10,
                IsDeleted = false,
                AnimalClinic = clinic
            };
            await _context.AnimalClinics.AddAsync(clinic);
            await _context.Doctors.AddAsync(doctor);

            // Seed Procedures
            var procedure = new Procedure
            {
                Id = 1,
                Name = "Vaccination",
                Description = "Routine vaccination",
                IsDeleted = false
            };
            await _context.Procedures.AddAsync(procedure);

            // Seed Appointments (one active, one deleted)
            var appointment1 = new Appointment
            {
                Id = 1,
                UserProfileId = "user-1",
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                IsDeleted = false
            };
            var appointment2 = new Appointment
            {
                Id = 2,
                UserProfileId = "user-1",
                AnimalId = 1,
                DoctorId = 1,
                ProcedureId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(2),
                IsDeleted = true
            };
            await _context.Appointments.AddRangeAsync(appointment1, appointment2);

            await _context.SaveChangesAsync();

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    if (id == identityUser1.Id) return identityUser1;
                    if (id == identityUser2.Id) return identityUser2;
                    return null;
                });

            _userManagerMock.Setup(um => um.DeleteAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _service = new UserManagementService(_context, _userManagerMock.Object);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetAllUserProfilesAsync_ExcludesSpecifiedUser()
        {
            var profiles = await _service.GetAllUserProfilesAsync("user-1");
            Assert.That(profiles.Any(p => p.Id == "user-1"), Is.False);
            Assert.That(profiles.Any(p => p.Id == "user-2"), Is.True);
        }

        [Test]
        public async Task GetAllUserProfilesAsync_WithNullExclude_ReturnsAllUsers()
        {
            var profiles = await _service.GetAllUserProfilesAsync(null!);
            Assert.That(profiles.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserDetailsAsync_ReturnsCorrectData()
        {
            var details = await _service.GetUserDetailsAsync("user-1");
            Assert.IsNotNull(details);
            Assert.That(details.UserId, Is.EqualTo("user-1"));
            Assert.That(details.Pets.Any(p => p.Name == "Bella"), Is.True);
            Assert.That(details.Pets.Any(p => p.Name == "Max"), Is.False); // Max is deleted
            Assert.That(details.PhoneNumber, Is.EqualTo("1234567890"));
            Assert.That(details.Address, Is.EqualTo("123 Main St"));
        }

        [Test]
        public async Task GetUserDetailsAsync_NonexistentUser_ReturnsNull()
        {
            var details = await _service.GetUserDetailsAsync("nonexistent");
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetUserBasicInfoAsync_ReturnsData()
        {
            var info = await _service.GetUserBasicInfoAsync("user-1");
            Assert.IsNotNull(info);
            Assert.That(info.Id, Is.EqualTo("user-1"));
            Assert.That(info.Email, Is.EqualTo("user1@example.com"));
        }

        [Test]
        public async Task GetUserBasicInfoAsync_UserNotFound_ReturnsNull()
        {
            var info = await _service.GetUserBasicInfoAsync("not-found");
            Assert.IsNull(info);
        }

        [Test]
        public async Task DeleteUserAsync_DeletesUserProfileAnimalsAndAppointments()
        {
            var result = await _service.DeleteUserAsync("user-1");
            Assert.IsTrue(result);

            var profile = await _context.UserProfiles.FindAsync("user-1");
            Assert.IsNull(profile);

            var animals = await _context.Animals.Where(a => a.UserProfileId == "user-1").ToListAsync();
            Assert.IsTrue(animals.All(a => a.IsDeleted));

            var appointments = await _context.Appointments.Where(appt => appt.UserProfileId == "user-1").ToListAsync();
            Assert.IsTrue(appointments.All(appt => appt.IsDeleted));

            _userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<IdentityUser>()), Times.Once);
        }

        [Test]
        public async Task DeleteUserAsync_ReturnsFalseIfUserNotFound()
        {
            var result = await _service.DeleteUserAsync("nonexistent");
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteUserAsync_ReturnsFalseIfDeleteFails()
        {
            _userManagerMock.Setup(um => um.DeleteAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Failed());

            var result = await _service.DeleteUserAsync("user-1");
            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetAnimalWithAppointmentsAsync_ReturnsData()
        {
            var animalDetails = await _service.GetAnimalWithAppointmentsAsync(1);
            Assert.IsNotNull(animalDetails);
            Assert.That(animalDetails.Id, Is.EqualTo(1));
            Assert.That(animalDetails.Appointments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAnimalWithAppointmentsAsync_NonexistentAnimal_ReturnsNull()
        {
            var animalDetails = await _service.GetAnimalWithAppointmentsAsync(999);
            Assert.IsNull(animalDetails);
        }

        [Test]
        public async Task GetUnregisterAnimalViewModelAsync_ReturnsData()
        {
            var vm = await _service.GetUnregisterAnimalViewModelAsync(1);
            Assert.IsNotNull(vm);
            Assert.That(vm.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUnregisterAnimalViewModelAsync_DeletedAnimal_ReturnsNull()
        {
            var vm = await _service.GetUnregisterAnimalViewModelAsync(2); // animal2 is deleted
            Assert.IsNull(vm);
        }

        [Test]
        public async Task GetUnregisterAnimalViewModelAsync_NonexistentAnimal_ReturnsNull()
        {
            var vm = await _service.GetUnregisterAnimalViewModelAsync(999);
            Assert.IsNull(vm);
        }

        [Test]
        public async Task UnregisterAnimalAsync_SoftDeletesAnimalAndAppointments()
        {
            var result = await _service.UnregisterAnimalAsync(1);
            Assert.IsTrue(result);

            var animal = await _context.Animals.FindAsync(1);
            Assert.IsTrue(animal!.IsDeleted);

            var appointments = await _context.Appointments.Where(a => a.AnimalId == 1).ToListAsync();
            Assert.IsTrue(appointments.All(a => a.IsDeleted));
        }

        [Test]
        public async Task UnregisterAnimalAsync_NonexistentAnimal_ReturnsFalse()
        {
            var result = await _service.UnregisterAnimalAsync(999);
            Assert.IsFalse(result);
        }

        [Test]
        public void GetAnimalOwnerId_ReturnsCorrectId()
        {
            var ownerId = _service.GetAnimalOwnerId(1);
            Assert.That(ownerId, Is.EqualTo("user-1"));
        }

        [Test]
        public void GetAnimalOwnerId_NonexistentAnimal_ReturnsNull()
        {
            var ownerId = _service.GetAnimalOwnerId(999);
            Assert.IsNull(ownerId);
        }

        [Test]
        public async Task GetAppointmentDetailsAsync_ReturnsData()
        {
            var details = await _service.GetAppointmentDetailsAsync(1);
            Assert.IsNotNull(details);
            Assert.That(details.Id, Is.EqualTo(1));
            Assert.That(details.AnimalName, Is.EqualTo("Bella"));
            Assert.That(details.DoctorName, Is.EqualTo("Dr. Smith"));
        }
        [Test]
        public async Task GetAppointmentDetailsAsync_NonexistentAppointment_ReturnsNull()
        {
            var details = await _service.GetAppointmentDetailsAsync(999);
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetCancelAppointmentViewModelAsync_ReturnsData()
        {
            var vm = await _service.GetCancelAppointmentViewModelAsync(1);
            Assert.IsNotNull(vm);
            Assert.That(vm.Id, Is.EqualTo(1));
            Assert.That(vm.AnimalName, Is.EqualTo("Bella"));
        }

        [Test]
        public async Task GetCancelAppointmentViewModelAsync_NonexistentAppointment_ReturnsNull()
        {
            var vm = await _service.GetCancelAppointmentViewModelAsync(999);
            Assert.IsNull(vm);
        }

        [Test]
        public async Task CancelAppointmentAsync_SetsIsDeletedTrue_ReturnsTrue()
        {
            var result = await _service.CancelAppointmentAsync(1);
            Assert.IsTrue(result);

            var appointment = await _context.Appointments.FindAsync(1);
            Assert.IsTrue(appointment!.IsDeleted);
        }

        [Test]
        public async Task CancelAppointmentAsync_NonexistentAppointment_ReturnsFalse()
        {
            var result = await _service.CancelAppointmentAsync(999);
            Assert.IsFalse(result);
        }
    }
}



