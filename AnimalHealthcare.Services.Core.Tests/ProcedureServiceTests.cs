using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core;
using Microsoft.EntityFrameworkCore;


namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class ProcedureServiceTests
    {
        private AnimalHealthcareDbContext _context;
        private ProcedureService _service;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase(databaseName: "ProcedureTestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            var clinic = new AnimalClinic 
            { 
                Id = 1,
                Address = "123 Pet Street",         
                PhoneNumber = "555-1234",
                 Name = "PetCare Clinic" 
            };
            await _context.AnimalClinics.AddAsync(clinic);

            var doctor1 = new Doctor
            {
                Id = 1,
                Name = "Dr. Vet",
                Specialization = "General",
                YearsOfExperience = 8,
                PhoneNumber = "123456789",
                IsDeleted = false,
                AnimalClinicId = clinic.Id
            };

            var doctor2 = new Doctor
            {
                Id = 2,
                Name = "Dr. Deleted",
                Specialization = "Surgery",
                YearsOfExperience = 12,
                PhoneNumber = "987654321",
                IsDeleted = true,
                AnimalClinicId = clinic.Id
            };

            await _context.Doctors.AddRangeAsync(doctor1, doctor2);

            var procedure1 = new Procedure
            {
                Id = 1,
                Name = "Vaccination",
                Description = "Animal vaccination procedure",
                IsDeleted = false
            };

            var procedure2 = new Procedure
            {
                Id = 2,
                Name = "Dental Cleaning",
                Description = "Dental cleaning for pets",
                IsDeleted = false
            };

            var deletedProcedure = new Procedure
            {
                Id = 3,
                Name = "Deleted Procedure",
                Description = "Should not appear",
                IsDeleted = true
            };

            await _context.Procedures.AddRangeAsync(procedure1, procedure2, deletedProcedure);

            // Add DoctorProcedures explicitly to join doctors and procedures
            var dp1 = new DoctorProcedure { DoctorId = doctor1.Id, ProcedureId = procedure1.Id };
            var dp2 = new DoctorProcedure { DoctorId = doctor2.Id, ProcedureId = procedure1.Id };

            await _context.DoctorProcedures.AddRangeAsync(dp1, dp2);

            await _context.SaveChangesAsync();

            _service = new ProcedureService(_context);
        }


        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetAllProceduresAsync_ReturnsOnlyNonDeleted()
        {
            var result = (await _service.GetAllProceduresAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(p => p.Name == "Vaccination"));
            Assert.That(result.Any(p => p.Name == "Dental Cleaning"));
            Assert.That(result.All(p => p.Name != "Deleted Procedure"));
        }

        [Test]
        public async Task GetProcedureDetailsAsync_ReturnsDetailsWithOnlyActiveDoctors()
        {
            var details = await _service.GetProcedureDetailsAsync(1);

            Assert.IsNotNull(details);
            Assert.That(details.Name, Is.EqualTo("Vaccination"));
            Assert.That(details.Doctors.Count, Is.EqualTo(1)); // Only one doctor not deleted
            var doctor = details.Doctors.First();
            Assert.That(doctor.Name, Is.EqualTo("Dr. Vet"));
            Assert.That(doctor.ClinicName, Is.EqualTo("PetCare Clinic"));
        }

        [Test]
        public async Task GetProcedureDetailsAsync_NonExistingId_ReturnsNull()
        {
            var details = await _service.GetProcedureDetailsAsync(999);
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetProcedureNameByIdAsync_ExistingProcedure_ReturnsName()
        {
            var name = await _service.GetProcedureNameByIdAsync(1);
            Assert.That(name, Is.EqualTo("Vaccination"));
        }

        [Test]
        public async Task GetProcedureNameByIdAsync_NonExistingProcedure_ReturnsNull()
        {
            var name = await _service.GetProcedureNameByIdAsync(999);
            Assert.IsNull(name);
        }

        [Test]
        public async Task GetAllProceduresAsync_WithNoProcedures_ReturnsEmptyList()
        {
            // Arrange: Clear all procedures
            _context.Procedures.RemoveRange(_context.Procedures);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllProceduresAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetProcedureDetailsAsync_WithProcedureHavingNoDoctors_ReturnsEmptyDoctorsList()
        {
            // Arrange
            var procedure = new Procedure
            {
                Id = 1000,
                Name = "Lonely Procedure",
                Description = "No doctors perform this",
                IsDeleted = false
            };
            await _context.Procedures.AddAsync(procedure);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetProcedureDetailsAsync(procedure.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(procedure.Name, result.Name);
            Assert.IsEmpty(result.Doctors);
        }

        [Test]
        public async Task GetProcedureDetailsAsync_WithAllDoctorsDeleted_ReturnsEmptyDoctorsList()
        {
            // Arrange
            var procedure = new Procedure
            {
                Id = 2000,
                Name = "Procedure With Deleted Doctors",
                Description = "All doctors deleted",
                IsDeleted = false
            };

            var doctor1 = new Doctor
            {
                Id = 100,
                Name = "Deleted Doctor 1",
                Specialization = "Surgery",           
                PhoneNumber = "123-456-7890",          
                IsDeleted = true,
                AnimalClinic = new AnimalClinic { Name = "Clinic 1", Address = "Addr", PhoneNumber = "123" }
            };

            var doctor2 = new Doctor
            {
                Id = 101,
                Name = "Deleted Doctor 2",
                Specialization = "Dentistry",          
                PhoneNumber = "987-654-3210",          
                IsDeleted = true,
                AnimalClinic = new AnimalClinic { Name = "Clinic 2", Address = "Addr2", PhoneNumber = "456" }
            };


            var doctorProcedure1 = new DoctorProcedure
            {
                Doctor = doctor1,
                Procedure = procedure
            };

            var doctorProcedure2 = new DoctorProcedure
            {
                Doctor = doctor2,
                Procedure = procedure
            };

            await _context.Procedures.AddAsync(procedure);
            await _context.Doctors.AddRangeAsync(doctor1, doctor2);
            await _context.DoctorProcedures.AddRangeAsync(doctorProcedure1, doctorProcedure2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetProcedureDetailsAsync(procedure.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Doctors);
        }

        [Test]
        public async Task GetProcedureDetailsAsync_WithMixedDoctors_ReturnsOnlyActiveDoctors()
        {
            // Arrange
            var procedure = new Procedure
            {
                Id = 3000,
                Name = "Procedure With Mixed Doctors",
                Description = "Some doctors active, some deleted",
                IsDeleted = false
            };

            var activeDoctor = new Doctor
            {
                Id = 200,
                Name = "Active Doctor",
                Specialization = "General Practice",    
                PhoneNumber = "555-0000",                
                IsDeleted = false,
                AnimalClinic = new AnimalClinic { Name = "Active Clinic", Address = "AddrA", PhoneNumber = "789" }
            };

            var deletedDoctor = new Doctor
            {
                Id = 201,
                Name = "Deleted Doctor",
                Specialization = "Dentistry",            
                PhoneNumber = "555-1111",                 
                IsDeleted = true,
                AnimalClinic = new AnimalClinic { Name = "Deleted Clinic", Address = "AddrB", PhoneNumber = "012" }
            };


            var activeDoctorProcedure = new DoctorProcedure
            {
                Doctor = activeDoctor,
                Procedure = procedure
            };

            var deletedDoctorProcedure = new DoctorProcedure
            {
                Doctor = deletedDoctor,
                Procedure = procedure
            };

            await _context.Procedures.AddAsync(procedure);
            await _context.Doctors.AddRangeAsync(activeDoctor, deletedDoctor);
            await _context.DoctorProcedures.AddRangeAsync(activeDoctorProcedure, deletedDoctorProcedure);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetProcedureDetailsAsync(procedure.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Doctors.Count, Is.EqualTo(1));
            Assert.That(result.Doctors.First().Name, Is.EqualTo(activeDoctor.Name));
        }

        [Test]
        public async Task GetProcedureNameByIdAsync_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetProcedureNameByIdAsync(-1);

            // Assert
            Assert.IsNull(result);
        }

    }
}

