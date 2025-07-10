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
            try
            {
                var model = await _clinicService.GetAllClinicsAsync();
                return View(model);
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return RedirectToAction("Error", "Home");
            }           
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _clinicService.GetClinicDetailsAsync(id);

                if (model == null)
                {
                    // Redirect to a custom error page
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return RedirectToAction("Error", "Home");
            }          
        }
    }
}
