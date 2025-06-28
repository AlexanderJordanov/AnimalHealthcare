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
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var animal = await _animalService.GetPetUnregisterViewModelByIdAsync(id, userId);
            if (animal == null)
            {
                return Forbid(); // Or NotFound(), depending on whether you want to reveal existence
            }

            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterConfirmed(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _animalService.UnregisterPetAsync(id, userId);
            if (!success)
            {
                return Forbid(); // or BadRequest if you prefer generic error
            }

            TempData["SuccessMessage"] = "Animal unregistered successfully.";
            return RedirectToAction("ViewProfile", "UserProfile");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            var model = await _animalService.GetAnimalDetailsViewModelAsync(id, userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _animalService.BuildEditPetViewModelAsync(id, userId);
            if (model == null) return Forbid(); // Not the owner or not found

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPetViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid) return View(model);

            var result = await _animalService.UpdateAnimalAsync(model, userId);

            if (result == null)
            {
                return Forbid(); // not authorized
            }

            if (result == false)
            {
                TempData["InfoMessage"] = "No changes were made to the pet information.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Pet information updated successfully!";
            return RedirectToAction("Details", new { id = model.Id });
        }
    }
}
