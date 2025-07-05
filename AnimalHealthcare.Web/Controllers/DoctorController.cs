using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AnimalHealthcare.Web.Controllers
{
    public class DoctorController : BaseController
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string? sortBy, string? filterBy, int page = 1)
        {
            const int pageSize = 5;
            var model = await _doctorService.GetDoctorsAsync(page, pageSize, sortBy, filterBy);
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetDoctorDetailsAsync(id);
            if (doctor == null) return NotFound();

            return View(doctor);
        }
    }
}
