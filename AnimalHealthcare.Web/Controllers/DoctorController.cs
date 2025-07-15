using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AnimalHealthcare.Web.Controllers
{
    using static AnimalHealthcare.GCommon.ValidationConstants.Doctor;
    public class DoctorController : BaseController
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string? sortBy, string? filterBy, int page = DefaultPage)
        {
            try
            {
                var model = await _doctorService.GetDoctorsAsync(page, PageSize, sortBy, filterBy);
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorDetailsAsync(id);
                if (doctor == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(doctor);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }
    }
}
