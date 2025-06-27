using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Animal;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class AnimalController : BaseController
    {
        private readonly IAnimalService _animalService;

        public AnimalController(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterPetViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterPetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserId(); // From BaseController
            if (userId == null) return Unauthorized();

            await _animalService.RegisterAnimalAsync(userId, model);

            TempData["SuccessMessage"] = "Pet registered successfully!";
            return RedirectToAction("ViewProfile", "UserProfile");
        }

        [HttpGet]
        public async Task<IActionResult> Unregister(int id)
        {
            try
            {
                var animal = await _animalService.GetPetUnregisterViewModelByIdAsync(id);
                if (animal == null)
                {
                    return NotFound();
                }

                return View(animal);
            }
            catch (Exception e)
            {

                return RedirectToAction(nameof(Unregister));
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterConfirmed(int id)
        {
            var success = await _animalService.UnregisterPetAsync(id);
            if (!success)
            {
                return BadRequest();
            }

            TempData["SuccessMessage"] = "Animal unregistered successfully.";
            return RedirectToAction("ViewProfile", "UserProfile");
        }
    }
}
