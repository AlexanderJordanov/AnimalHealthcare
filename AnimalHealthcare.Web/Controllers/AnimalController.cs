using AnimalHealthcare.GCommon.Enums;
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

        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                var model = new RegisterPetViewModel();
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }           
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterPetViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                await _animalService.RegisterAnimalAsync(userId, model);

                TempData["SuccessMessage"] = "Pet registered successfully!";
                return RedirectToAction("ViewProfile", "UserProfile");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Unregister(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) 
                { 
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 }); 
                }

                var animal = await _animalService.GetPetUnregisterViewModelByIdAsync(id, userId);
                if (animal == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(animal);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterConfirmed(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var result = await _animalService.UnregisterPetAsync(id, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Animal unregistered successfully.";
                        return RedirectToAction("ViewProfile", "UserProfile");

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }

            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _animalService.GetAnimalDetailsViewModelAsync(id, userId);

                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }           
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _animalService.BuildEditPetViewModelAsync(id, userId);
                if (model == null) 
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 }); // Covers not found or not owned
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }         
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPetViewModel model)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _animalService.UpdateAnimalAsync(model, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Pet information updated successfully!";
                        return RedirectToAction("ViewProfile", "UserProfile");

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "No changes were made to the pet information.";
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

    }
}
