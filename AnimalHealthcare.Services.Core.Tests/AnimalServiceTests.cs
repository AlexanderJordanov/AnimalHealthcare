using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Web.ViewModels.Animal;
using Microsoft.EntityFrameworkCore;


namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class AnimalServiceTests
    {
        private AnimalHealthcareDbContext _context = null!;
        private AnimalService _service = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase("AnimalServiceTestDb_" + Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            // Seed user profiles
            var userProfiles = new List<UserProfile>
            {
                new UserProfile { Id = "user-1", FullName = "Test User One" },
                new UserProfile { Id = "user-2", FullName = "Test User Two" }
            };
            await _context.UserProfiles.AddRangeAsync(userProfiles);

            // Seed animal clinic
            var clinic = new AnimalClinic
            {
                Id = 1,
                Name = "Test Vet Clinic",
                Address = "123 Main St",
                PhoneNumber = "555-0000",
                ImageUrl = "clinic.jpg"
            };
            await _context.AnimalClinics.AddAsync(clinic);

            // Seed doctors
            var doctor1 = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Specialization = "Surgery",
                ImageUrl = "drsmith.jpg",
                YearsOfExperience = 10,
                PhoneNumber = "555-1111",
                AnimalClinic = clinic
            };

            var doctor2 = new Doctor
            {
                Id = 2,
                Name = "Dr. Jones",
                Specialization = "Dentistry",
                ImageUrl = "drjones.jpg",
                YearsOfExperience = 5,
                PhoneNumber = "555-2222",
                AnimalClinic = clinic
            };
            await _context.Doctors.AddRangeAsync(doctor1, doctor2);

            // Seed procedures
            var procedure1 = new Procedure
            {
                Id = 1,
                Name = "Vaccination",
                Description = "Annual vaccination",
                IsDeleted = false
            };

            var procedure2 = new Procedure
            {
                Id = 2,
                Name = "Dental Cleaning",
                Description = "Teeth cleaning procedure",
                IsDeleted = false
            };

            await _context.Procedures.AddRangeAsync(procedure1, procedure2);

            // Seed animals with appointments linked to doctors and procedures
            var animals = new List<Animal>
            {
                new Animal
                {
                    Id = 1,
                    Name = "Bella",
                    Species = "Dog",
                    Breed = "Labrador",
                    Age = 5,
                    Gender = GCommon.Enums.AnimalGender.Female,
                    UserProfileId = "user-1",
                    UserProfile = userProfiles.First(up => up.Id == "user-1"),
                    IsDeleted = false,
                    Appointments = new List<Appointment>
                    {
                        new Appointment
                        {
                            Id = 1,
                            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
                            IsDeleted = false,
                            UserProfileId = "user-1",
                            Doctor = doctor1,
                            Procedure = procedure1
                        },
                        new Appointment
                        {
                            Id = 2,
                            AppointmentDateTime = DateTime.UtcNow.AddDays(2),
                            IsDeleted = false,
                            UserProfileId = "user-1",
                            Doctor = doctor2,
                            Procedure = procedure2
                        }
                    }
                },
                new Animal
                {
                    Id = 2,
                    Name = "Max",
                    Species = "Cat",
                    Breed = "Siamese",
                    Age = 3,
                    Gender = GCommon.Enums.AnimalGender.Male,
                    UserProfileId = "user-1",
                    UserProfile = userProfiles.First(up => up.Id == "user-1"),
                    IsDeleted = true
                },
                new Animal
                {
                    Id = 3,
                    Name = "Charlie",
                    Species = "Dog",
                    Breed = "Beagle",
                    Age = 2,
                    Gender = GCommon.Enums.AnimalGender.Male,
                    UserProfileId = "user-2",
                    UserProfile = userProfiles.First(up => up.Id == "user-2"),
                    IsDeleted = false
                }
            };

            await _context.Animals.AddRangeAsync(animals);
            await _context.SaveChangesAsync();

            _service = new AnimalService(_context);
        }


        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetAnimalSummariesByOwnerIdAsync_ReturnsOnlyNonDeletedAnimalsForUser()
        {
            // Act
            var result = await _service.GetAnimalSummariesByOwnerIdAsync("user-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1)); // Only 1 non-deleted animal for user-1

            var animal = result.First();
            Assert.That(animal.Name, Is.EqualTo("Bella"));
            Assert.That(animal.Species, Is.EqualTo("Dog"));
            Assert.That(animal.Breed, Is.EqualTo("Labrador"));
        }

        [Test]
        public async Task GetAnimalSummariesByOwnerIdAsync_ReturnsEmptyListIfNoAnimals()
        {
            // Act
            var result = await _service.GetAnimalSummariesByOwnerIdAsync("nonexistent-user");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAnimalSummariesByOwnerIdAsync_NullOrEmptyUserId_ReturnsEmptyList()
        {
            // Act
            var resultNull = await _service.GetAnimalSummariesByOwnerIdAsync(null!);
            var resultEmpty = await _service.GetAnimalSummariesByOwnerIdAsync(string.Empty);

            // Assert
            Assert.IsNotNull(resultNull);
            Assert.IsEmpty(resultNull);

            Assert.IsNotNull(resultEmpty);
            Assert.IsEmpty(resultEmpty);
        }

        [Test]
        public async Task RegisterAnimalAsync_ValidData_AddsAnimal()
        {
            // Arrange
            string userId = "test-user";
            var model = new RegisterPetViewModel
            {
                Name = "Buddy",
                Age = 3,
                Species = "Dog",
                Breed = "Labrador",
                Gender = GCommon.Enums.AnimalGender.Male
            };

            // Act
            await _service.RegisterAnimalAsync(userId, model);

            // Assert
            var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Name == "Buddy" && a.UserProfileId == userId);
            Assert.That(animal, Is.Not.Null);
            Assert.That(animal.Species, Is.EqualTo("Dog"));
            Assert.That(animal.Breed, Is.EqualTo("Labrador"));
            Assert.That(animal.Age, Is.EqualTo(3));
            Assert.That(animal.Gender, Is.EqualTo(GCommon.Enums.AnimalGender.Male));
        }

        [Test, TestCaseSource(nameof(InvalidNameOrSpeciesCases))]
        public void RegisterAnimalAsync_InvalidNameOrSpecies_ThrowsArgumentException(string name, string species)
        {
            // Arrange
            string userId = "test-user";
            var model = new RegisterPetViewModel
            {
                Name = name,
                Age = 2,
                Species = species,
                Breed = "Labrador",
                Gender = GCommon.Enums.AnimalGender.Male
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.RegisterAnimalAsync(userId, model));
        }

        [Test]
        public async Task GetPetUnregisterViewModelByIdAsync_ValidIdAndOwner_ReturnsViewModel()
        {
            // Arrange
            var userId = "user-1";
            var animalId = 1;

            // Act
            var result = await _service.GetPetUnregisterViewModelByIdAsync(animalId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(animalId));
            Assert.That(result.Name, Is.EqualTo("Bella"));
        }

        [Test]
        public async Task GetPetUnregisterViewModelByIdAsync_AnimalDoesNotExist_ReturnsNull()
        {
            // Arrange
            var result = await _service.GetPetUnregisterViewModelByIdAsync(999, "user-1");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPetUnregisterViewModelByIdAsync_AnimalIsDeleted_ReturnsNull()
        {
            // Arrange
            var deletedAnimalId = 2;

            var result = await _service.GetPetUnregisterViewModelByIdAsync(deletedAnimalId, "user-1");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPetUnregisterViewModelByIdAsync_WrongOwner_ReturnsNull()
        {
            // Arrange
            var animalId = 1;
            var wrongUserId = "user-2";

            var result = await _service.GetPetUnregisterViewModelByIdAsync(animalId, wrongUserId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPetUnregisterViewModelByIdAsync_NoUserIdProvided_ReturnsViewModel()
        {
            // Arrange
            var animalId = 1;

            var result = await _service.GetPetUnregisterViewModelByIdAsync(animalId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(animalId));
        }

        [Test]
        public async Task UnregisterPetAsync_ValidPetAndUser_ReturnsSuccessAndSoftDeletes()
        {
            // Act
            var result = await _service.UnregisterPetAsync(1, "user-1");

            // Assert
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var animal = await _context.Animals.FindAsync(1);
            Assert.That(animal!.IsDeleted, Is.True);

            var appointments = _context.Appointments.Where(a => a.Id == 1 || a.Id == 2).ToList();
            Assert.That(appointments.All(a => a.IsDeleted), Is.True);
        }

        [Test]
        public async Task UnregisterPetAsync_AnimalNotFound_ReturnsNotFound()
        {
            var result = await _service.UnregisterPetAsync(999, "user-1");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UnregisterPetAsync_UnauthorizedUser_ReturnsUnauthorized()
        {
            var result = await _service.UnregisterPetAsync(1, "unauthorized-user");
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));

            // Animal should remain not deleted
            var animal = await _context.Animals.FindAsync(1);
            Assert.That(animal!.IsDeleted, Is.False);
        }

        [Test]
        public async Task UnregisterPetAsync_NoRequestingUserIdProvided_Succeeds()
        {
            // Should allow unregistering without user ID ownership check
            var result = await _service.UnregisterPetAsync(3);
            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var animal = await _context.Animals.FindAsync(3);
            Assert.That(animal!.IsDeleted, Is.True);
        }

        [Test]
        public async Task GetAnimalDetailsViewModelAsync_ValidAnimalAndOwner_ReturnsDetails()
        {
            // Arrange
            var userId = "user-1"; // owner of animalId 1
            var animalId = 1;

            // Act
            var result = await _service.GetAnimalDetailsViewModelAsync(animalId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(animalId));
            Assert.That(result.Name, Is.EqualTo("Bella"));
            Assert.That(result.Appointments, Is.Not.Empty);
            Assert.That(result.Appointments.All(a => !string.IsNullOrEmpty(a.DoctorName)));
        }

        [Test]
        public async Task GetAnimalDetailsViewModelAsync_DifferentUser_ReturnsNull()
        {
            // Arrange
            var userId = "user-2"; // not owner of animalId 1
            var animalId = 1;

            // Act
            var result = await _service.GetAnimalDetailsViewModelAsync(animalId, userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAnimalDetailsViewModelAsync_AnimalDoesNotExist_ReturnsNull()
        {
            // Arrange
            var userId = "user-1";
            var animalId = 999; // non-existing animal

            // Act
            var result = await _service.GetAnimalDetailsViewModelAsync(animalId, userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAnimalDetailsViewModelAsync_AnimalDeleted_ReturnsNull()
        {
            // Arrange
            var userId = "user-1";
            var animalId = 2; // soft-deleted animal

            // Act
            var result = await _service.GetAnimalDetailsViewModelAsync(animalId, userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task BuildEditPetViewModelAsync_ValidIdAndOwner_ReturnsViewModel()
        {
            var userId = "user-1";
            var petId = 1; 

            var result = await _service.BuildEditPetViewModelAsync(petId, userId);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(petId));
            Assert.That(result.Name, Is.EqualTo("Bella"));
        }

        [Test]
        public async Task BuildEditPetViewModelAsync_AnimalDoesNotExist_ReturnsNull()
        {
            var result = await _service.BuildEditPetViewModelAsync(999, "user-1");

            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditPetViewModelAsync_AnimalIsDeleted_ReturnsNull()
        {
            // Animal with Id=2 is deleted
            var result = await _service.BuildEditPetViewModelAsync(2, "user-1");

            Assert.IsNull(result);
        }

        [Test]
        public async Task BuildEditPetViewModelAsync_WrongOwner_ReturnsNull()
        {
            // Animal 1 belongs to user-1, test user-2 requesting
            var result = await _service.BuildEditPetViewModelAsync(1, "user-2");

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateAnimalAsync_ValidChanges_ReturnsSuccess()
        {
            var userId = "user-1";
            var model = new EditPetViewModel
            {
                Id = 1,
                Name = "Bella Updated",
                Age = 6,
                Species = "Dog",
                Breed = "Labrador",
                Gender = GCommon.Enums.AnimalGender.Female
            };

            var result = await _service.UpdateAnimalAsync(model, userId);

            Assert.That(result, Is.EqualTo(ServiceOperationResult.Success));

            var updated = await _context.Animals.FindAsync(model.Id);
            Assert.That(updated!.Name, Is.EqualTo("Bella Updated"));
            Assert.That(updated.Age, Is.EqualTo(6));
        }

        [Test]
        public async Task UpdateAnimalAsync_NoChanges_ReturnsNoChange()
        {
            var userId = "user-1";
            var model = new EditPetViewModel
            {
                Id = 1,
                Name = "Bella", // Same as original
                Age = 5,
                Species = "Dog",
                Breed = "Labrador",
                Gender = GCommon.Enums.AnimalGender.Female
            };

            var result = await _service.UpdateAnimalAsync(model, userId);

            Assert.That(result, Is.EqualTo(ServiceOperationResult.NoChange));
        }

        [Test]
        public async Task UpdateAnimalAsync_AnimalNotFound_ReturnsNotFound()
        {
            var userId = "user-1";
            var model = new EditPetViewModel
            {
                Id = 999, // Nonexistent ID
                Name = "Ghost",
                Age = 2,
                Species = "Dog",
                Breed = "Unknown",
                Gender = GCommon.Enums.AnimalGender.Male
            };

            var result = await _service.UpdateAnimalAsync(model, userId);

            Assert.That(result, Is.EqualTo(ServiceOperationResult.NotFound));
        }

        [Test]
        public async Task UpdateAnimalAsync_UnauthorizedUser_ReturnsUnauthorized()
        {
            var userId = "unauthorized-user";
            var model = new EditPetViewModel
            {
                Id = 1,
                Name = "Bella Updated",
                Age = 6,
                Species = "Dog",
                Breed = "Labrador",
                Gender = GCommon.Enums.AnimalGender.Female
            };

            var result = await _service.UpdateAnimalAsync(model, userId);

            Assert.That(result, Is.EqualTo(ServiceOperationResult.Unauthorized));
        }


        private static IEnumerable<TestCaseData> InvalidNameOrSpeciesCases()
        {
            yield return new TestCaseData(null, "Dog");
            yield return new TestCaseData("", "Dog");
            yield return new TestCaseData("   ", "Dog");
            yield return new TestCaseData("Buddy", null);
            yield return new TestCaseData("Buddy", "");
            yield return new TestCaseData("Buddy", "   ");
        }

    }
}
