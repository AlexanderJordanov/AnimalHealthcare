using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class AnimalClinicController : BaseController
    {
        private readonly IAnimalClinicService _clinicService;

        public AnimalClinicController(IAnimalClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _clinicService.GetAllClinicsAsync();
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _clinicService.GetClinicDetailsAsync(id);

            if (model == null)
            {
                // Option 1: Return built-in 404
                //return NotFound();

                // Option 2 (optional): Redirect to a custom error page
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(model);
        }
    }
}
