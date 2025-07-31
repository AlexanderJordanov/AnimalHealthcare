using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserProfile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class UserProfileController : BaseController
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAnimalService _animalService;

        public UserProfileController(IUserProfileService userProfileService, IAnimalService animalService)
        {
            _userProfileService = userProfileService;
            _animalService = animalService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profile = await _userProfileService.GetProfileByIdAsync(userId, userId);
                if (profile == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                var animals = await _animalService.GetAnimalSummariesByOwnerIdAsync(userId);
                var model = _userProfileService.BuildUserProfileViewModel(profile, animals);
                if (model == null) return NotFound();

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProfilePicture(string profilePictureUrl)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                if (!Uri.TryCreate(profilePictureUrl, UriKind.Absolute, out var uriResult) ||
                    !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    ModelState.AddModelError("", "Please provide a valid URL.");
                    return RedirectToAction(nameof(ViewProfile));
                }

                var result = await _userProfileService.UpdateProfilePictureAsync(userId, profilePictureUrl, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Profile picture updated successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(ViewProfile));

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var result = await _userProfileService.UpdateProfilePictureAsync(userId, null, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Profile picture removed successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(ViewProfile));

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
        public async Task<IActionResult> EditEmail()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _userProfileService.BuildEditEmailViewModelAsync(userId, userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmail(EditEmailViewModel model)
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

                var result = await _userProfileService.UpdateEmailAsync(userId, model, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Email updated successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your email is unchanged.";
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    default:
                        ModelState.AddModelError("", "Email update failed. Please try again.");
                        return View(model);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditFullName()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _userProfileService.BuildEditFullNameViewModelAsync(userId, userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFullName(EditFullNameViewModel model)
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

                var result = await _userProfileService.UpdateFullNameAsync(userId, model, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Full name updated successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your full name is unchanged.";
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

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
        public async Task<IActionResult> EditPhoneNumber()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _userProfileService.BuildEditPhoneNumberViewModelAsync(userId, userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoneNumber(EditPhoneNumberViewModel model)
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

                var result = await _userProfileService.UpdatePhoneNumberAsync(userId, model, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Phone number updated successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Phone number is unchanged.";
                        return View(model);

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
        public async Task<IActionResult> EditAddress()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _userProfileService.BuildEditAddressViewModelAsync(userId, userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(EditAddressViewModel model)
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

                var result = await _userProfileService.UpdateAddressAsync(userId, model, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Address updated successfully!";
                        return RedirectToAction(nameof(ViewProfile));

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your address is unchanged.";
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

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
        public async Task<IActionResult> Delete()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profile = await _userProfileService.GetProfileByIdAsync(userId, userId);
                if (profile == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });
                }

                return View(new DeleteUserProfileViewModel
                {
                    FullName = profile.FullName,
                    Email = profile.User.Email!
                });
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var result = await _userProfileService.DeleteUserProfileAsync(userId, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Your profile has been deleted.";
                        return RedirectToAction("Index", "Home");

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

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
