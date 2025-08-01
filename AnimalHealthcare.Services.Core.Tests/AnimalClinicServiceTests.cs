using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core;
using Microsoft.EntityFrameworkCore;


namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class AnimalClinicServiceTests
    {
        private AnimalHealthcareDbContext _context;
        private AnimalClinicService _service;

        [SetUp]
        public async Task Setup()
        {
            // Setup in-memory database with unique name per test run to ensure isolation
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase(databaseName: "AnimalClinicTestDb_" + Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            // Add doctors first separately to avoid tracking conflicts
            var activeDoctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Specialization = "Surgery",
                ImageUrl = "drsmith.jpg",
                YearsOfExperience = 10,
                PhoneNumber = "555-5678",
                IsDeleted = false
            };

            var deletedDoctor = new Doctor
            {
                Id = 2,
                Name = "Dr. Jane",
                Specialization = "Dentistry",
                ImageUrl = "drjane.jpg",
                YearsOfExperience = 5,
                PhoneNumber = "555-8765",
                IsDeleted = true // Should not appear in results
            };

            await _context.Doctors.AddRangeAsync(activeDoctor, deletedDoctor);
            await _context.SaveChangesAsync();

            // Now create clinics referencing the existing doctors
            var clinic1 = new AnimalClinic
            {
                Id = 1,
                Name = "Happy Pets Clinic",
                Address = "123 Pet Street",
                PhoneNumber = "555-1234",
                ImageUrl = "image1.jpg",
                Doctors = new List<Doctor> { activeDoctor, deletedDoctor }
            };

            var clinic2 = new AnimalClinic
            {
                Id = 2,
                Name = "Healthy Tails Vet",
                Address = "456 Animal Avenue",
                PhoneNumber = "555-4321",
                ImageUrl = "image2.jpg",
                Doctors = new List<Doctor>()
            };

            await _context.AnimalClinics.AddRangeAsync(clinic1, clinic2);
            await _context.SaveChangesAsync();

            _service = new AnimalClinicService(_context);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetAllClinicsAsync_ReturnsAllClinics()
        {
            var result = await _service.GetAllClinicsAsync();

            Assert.IsNotNull(result);
            Assert.That(result.Clinics.Count(), Is.EqualTo(2));

            var clinicNames = result.Clinics.Select(c => c.Name).ToList();
            Assert.That(clinicNames, Does.Contain("Happy Pets Clinic"));
            Assert.That(clinicNames, Does.Contain("Healthy Tails Vet"));
        }

        [Test]
        public async Task GetClinicDetailsAsync_ExistingClinic_ReturnsDetailsWithActiveDoctors()
        {
            var details = await _service.GetClinicDetailsAsync(1);

            Assert.IsNotNull(details);
            Assert.That(details.Name, Is.EqualTo("Happy Pets Clinic"));
            Assert.That(details.Doctors.Count, Is.EqualTo(1)); // Only active doctor included
            var doctor = details.Doctors.First();
            Assert.That(doctor.Name, Is.EqualTo("Dr. Smith"));
            Assert.That(doctor.Specialization, Is.EqualTo("Surgery"));
            Assert.That(doctor.ProfileImageUrl, Is.EqualTo("drsmith.jpg"));
        }

        [Test]
        public async Task GetClinicDetailsAsync_NonExistingClinic_ReturnsNull()
        {
            var details = await _service.GetClinicDetailsAsync(999);
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetAllClinicsAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Setup fresh context with no data for this test
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase(databaseName: "EmptyDb_" + Guid.NewGuid())
                .Options;
            await using var emptyContext = new AnimalHealthcareDbContext(options);
            var service = new AnimalClinicService(emptyContext);

            var result = await service.GetAllClinicsAsync();

            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Clinics);
        }

        [Test]
        public async Task GetClinicDetailsAsync_DoctorsIncludeOnlyNonDeleted()
        {
            // Add clinic with doctors for this test
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase(databaseName: "ClinicDoctorsTestDb_" + Guid.NewGuid())
                .Options;
            await using var context = new AnimalHealthcareDbContext(options);

            var activeDoc = new Doctor
            {
                Id = 1,
                Name = "Active Doc",
                Specialization = "Surgery",
                ImageUrl = "img1.jpg",
                YearsOfExperience = 10,
                PhoneNumber = "111",
                IsDeleted = false
            };
            var deletedDoc = new Doctor
            {
                Id = 2,
                Name = "Deleted Doc",
                Specialization = "Dentistry",
                ImageUrl = "img2.jpg",
                YearsOfExperience = 5,
                PhoneNumber = "222",
                IsDeleted = true
            };

            await context.Doctors.AddRangeAsync(activeDoc, deletedDoc);
            await context.SaveChangesAsync();

            var clinic = new AnimalClinic
            {
                Id = 100,
                Name = "Test Clinic",
                Address = "123 Test St",
                PhoneNumber = "123-456",
                ImageUrl = "image.jpg",
                Doctors = new List<Doctor> { activeDoc, deletedDoc }
            };

            await context.AnimalClinics.AddAsync(clinic);
            await context.SaveChangesAsync();

            var service = new AnimalClinicService(context);

            var result = await service.GetClinicDetailsAsync(100);

            Assert.IsNotNull(result);
            Assert.That(result.Doctors.Count, Is.EqualTo(1));
            Assert.That(result.Doctors.First().Name, Is.EqualTo("Active Doc"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task GetClinicDetailsAsync_InvalidId_ReturnsNull(int invalidId)
        {
            var result = await _service.GetClinicDetailsAsync(invalidId);
            Assert.IsNull(result);
        }
    }
}
