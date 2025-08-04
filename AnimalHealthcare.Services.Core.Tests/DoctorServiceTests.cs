using AnimalHealthcare.Data;
using AnimalHealthcare.Data.Models;
using AnimalHealthcare.Services.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalHealthcare.Tests.Services
{
    [TestFixture]
    public class DoctorServiceTests
    {
        private AnimalHealthcareDbContext _context = null!;
        private DoctorService _service = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AnimalHealthcareDbContext>()
                .UseInMemoryDatabase("DoctorServiceTestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new AnimalHealthcareDbContext(options);

            var clinic1 = new AnimalClinic
            {
                Id = 1,
                Name = "Happy Pets Clinic",
                Address = "123 Pet St",
                PhoneNumber = "555-1111",
                ImageUrl = "clinic1.jpg"
            };
            var clinic2 = new AnimalClinic
            {
                Id = 2,
                Name = "Healthy Tails Vet",
                Address = "456 Animal Ave",
                PhoneNumber = "555-2222",
                ImageUrl = "clinic2.jpg"
            };
            await _context.AnimalClinics.AddRangeAsync(clinic1, clinic2);

            var procedure1 = new Procedure
            {
                Id = 1,
                Name = "Vaccination",
                Description = "Preventive vaccination.",
                IsDeleted = false
            };
            var procedure2 = new Procedure
            {
                Id = 2,
                Name = "Surgery",
                Description = "General surgery.",
                IsDeleted = false
            };
            var procedureDeleted = new Procedure
            {
                Id = 3,
                Name = "Deleted Procedure",
                Description = "Should be ignored.",
                IsDeleted = true
            };
            await _context.Procedures.AddRangeAsync(procedure1, procedure2, procedureDeleted);

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
                Name = "Dr. Jane",
                Specialization = "Dentistry",
                PhoneNumber = "555-5678",
                ImageUrl = "drjane.jpg",
                YearsOfExperience = 7,
                IsDeleted = false,
                AnimalClinicId = 2
            };
            var doctorDeleted = new Doctor
            {
                Id = 3,
                Name = "Dr. Deleted",
                Specialization = "Surgery",
                PhoneNumber = "555-0000",
                ImageUrl = "drdeleted.jpg",
                YearsOfExperience = 5,
                IsDeleted = true,
                AnimalClinicId = 1
            };
            await _context.Doctors.AddRangeAsync(doctor1, doctor2, doctorDeleted);

            var doctorProcedures = new List<DoctorProcedure>
            {
                new DoctorProcedure { DoctorId = 1, ProcedureId = 1 },
                new DoctorProcedure { DoctorId = 1, ProcedureId = 2 },
                new DoctorProcedure { DoctorId = 2, ProcedureId = 1 },
                new DoctorProcedure { DoctorId = 3, ProcedureId = 1 } // Deleted doctor should be ignored
            };
            await _context.DoctorProcedures.AddRangeAsync(doctorProcedures);

            await _context.SaveChangesAsync();

            _service = new DoctorService(_context);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetDoctorsByProcedureAsync_ValidProcedure_ReturnsOnlyNonDeletedDoctors()
        {
            var doctors = await _service.GetDoctorsByProcedureAsync(1);

            Assert.IsNotNull(doctors);
            Assert.That(doctors.Count, Is.EqualTo(2)); // doctor1 and doctor2 linked; doctor3 deleted excluded
            Assert.That(doctors.Any(d => d.Text == "Dr. Smith"));
            Assert.That(doctors.Any(d => d.Text == "Dr. Jane"));
            Assert.That(doctors.Any(d => d.Text == "Dr. Deleted") == false);
        }

        [Test]
        public async Task GetDoctorsByProcedureAsync_ProcedureWithNoDoctors_ReturnsEmptyList()
        {
            var doctors = await _service.GetDoctorsByProcedureAsync(9999);
            Assert.IsNotNull(doctors);
            Assert.IsEmpty(doctors);
        }

        [Test]
        public async Task GetDoctorsByProcedureAsync_DeletedProcedure_ReturnsEmptyList()
        {
            var doctors = await _service.GetDoctorsByProcedureAsync(3); // procedureDeleted
            Assert.IsNotNull(doctors);
            Assert.IsEmpty(doctors);
        }

        [Test]
        public async Task GetDoctorsAsync_ReturnsPagedDoctors_DefaultSortByName()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: null, filterBy: null);

            Assert.IsNotNull(result);
            Assert.That(result.Doctors.Count(), Is.EqualTo(2)); // Only 2 active doctors
            Assert.That(result.TotalPages, Is.EqualTo(1));
            Assert.That(result.CurrentPage, Is.EqualTo(1));
            Assert.That(result.CurrentSort, Is.Null);
            Assert.That(result.CurrentFilter, Is.Null);

            var names = result.Doctors.Select(d => d.Name).ToList();
            Assert.That(names, Is.Ordered.Ascending);
        }

        [Test]
        public async Task GetDoctorsAsync_SortsBySpeciality()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: "speciality", filterBy: null);

            var specs = result.Doctors.Select(d => d.Specialization).ToList();
            Assert.That(specs, Is.Ordered.Ascending);
            Assert.That(result.AvailableFilters.Select(f => f.Text), Does.Contain("Dentistry").And.Contain("Surgery"));
        }

        [Test]
        public async Task GetDoctorsAsync_SortsByClinic()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: "clinic", filterBy: null);

            var clinics = result.Doctors.Select(d => d.ClinicName).ToList();
            Assert.That(clinics, Is.Ordered.Ascending);
            Assert.That(result.AvailableFilters.Select(f => f.Text), Does.Contain("Happy Pets Clinic").And.Contain("Healthy Tails Vet"));
        }

        [Test]
        public async Task GetDoctorsAsync_FiltersBySpeciality()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: "speciality", filterBy: "Dentistry");

            Assert.That(result.Doctors.All(d => d.Specialization == "Dentistry"));
            Assert.That(result.CurrentFilter, Is.EqualTo("Dentistry"));
        }

        [Test]
        public async Task GetDoctorsAsync_FiltersByClinic()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: "clinic", filterBy: "Healthy Tails Vet");

            Assert.That(result.Doctors.All(d => d.ClinicName == "Healthy Tails Vet"));
            Assert.That(result.CurrentFilter, Is.EqualTo("Healthy Tails Vet"));
        }

        [Test]
        public async Task GetDoctorsAsync_FilterNoMatches_ReturnsEmptyList()
        {
            var result = await _service.GetDoctorsAsync(page: 1, pageSize: 10, sortBy: "clinic", filterBy: "Nonexistent Clinic");

            Assert.IsEmpty(result.Doctors);
        }

        [Test]
        public async Task GetDoctorsAsync_Pagination_WorksCorrectly()
        {
            // Assume pageSize 1, so 2 pages total for 2 doctors
            var page1 = await _service.GetDoctorsAsync(page: 1, pageSize: 1, sortBy: null, filterBy: null);
            var page2 = await _service.GetDoctorsAsync(page: 2, pageSize: 1, sortBy: null, filterBy: null);

            Assert.That(page1.Doctors.Count(), Is.EqualTo(1));
            Assert.That(page2.Doctors.Count(), Is.EqualTo(1));
            Assert.That(page1.Doctors.First().Id, Is.Not.EqualTo(page2.Doctors.First().Id));
        }

        [Test]
        public async Task GetDoctorDetailsAsync_ValidDoctor_ReturnsFullDetails()
        {
            var details = await _service.GetDoctorDetailsAsync(1);

            Assert.IsNotNull(details);
            Assert.That(details.Name, Is.EqualTo("Dr. Smith"));
            Assert.That(details.Specialization, Is.EqualTo("Surgery"));
            Assert.That(details.ClinicName, Is.EqualTo("Happy Pets Clinic"));
            Assert.That(details.ProceduresPerformed, Does.Contain("Vaccination").And.Contain("Surgery"));
        }

        [Test]
        public async Task GetDoctorDetailsAsync_DeletedDoctor_ReturnsNull()
        {
            var details = await _service.GetDoctorDetailsAsync(3);
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetDoctorDetailsAsync_NonExistentDoctor_ReturnsNull()
        {
            var details = await _service.GetDoctorDetailsAsync(9999);
            Assert.IsNull(details);
        }

        [Test]
        public async Task GetDoctorDetailsAsync_DoctorWithNoProcedures_ReturnsEmptyProcedures()
        {
            // Add a doctor with no procedures
            var doctorNoProc = new Doctor
            {
                Id = 4,
                Name = "Dr. NoProc",
                Specialization = "General",
                PhoneNumber = "555-9999",
                ImageUrl = "drnoproc.jpg",
                YearsOfExperience = 3,
                IsDeleted = false,
                AnimalClinicId = 1
            };
            await _context.Doctors.AddAsync(doctorNoProc);
            await _context.SaveChangesAsync();

            var details = await _service.GetDoctorDetailsAsync(4);

            Assert.IsNotNull(details);
            Assert.That(details.ProceduresPerformed, Is.EqualTo(string.Empty));
        }


        // 4) GetDoctorNameByIdAsync

        [Test]
        public async Task GetDoctorNameByIdAsync_ValidDoctor_ReturnsName()
        {
            var name = await _service.GetDoctorNameByIdAsync(1);
            Assert.That(name, Is.EqualTo("Dr. Smith"));
        }

        [Test]
        public async Task GetDoctorNameByIdAsync_DeletedDoctor_ReturnsNull()
        {
            var name = await _service.GetDoctorNameByIdAsync(3);
            Assert.IsNull(name);
        }

        [Test]
        public async Task GetDoctorNameByIdAsync_NonExistentDoctor_ReturnsNull()
        {
            var name = await _service.GetDoctorNameByIdAsync(9999);
            Assert.IsNull(name);
        }
    }
}
