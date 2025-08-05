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
        public async Task<IActionResult> ViewProfile(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (requestingUserId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var profile = await _userProfileService.GetProfileByIdAsync(targetProfileId, requestingUserId);
                if (profile == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                var animals = await _animalService.GetAnimalSummariesByOwnerIdAsync(targetProfileId);
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
        public async Task<IActionResult> AddProfilePicture(string profileId, string profilePictureUrl)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                if (string.IsNullOrWhiteSpace(profileId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 400 });
                }

                // Validate URL
                if (!Uri.TryCreate(profilePictureUrl, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    TempData["InfoMessage"] = "Please provide a valid URL.";
                    return RedirectToAction(nameof(ViewProfile), new { profileId });
                }

                var result = await _userProfileService.UpdateProfilePictureAsync(profileId, profilePictureUrl, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Profile picture updated successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture(string profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                if (string.IsNullOrWhiteSpace(profileId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 400 });
                }

                var result = await _userProfileService.UpdateProfilePictureAsync(profileId, null, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Profile picture removed successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }



        [HttpGet]
        public async Task<IActionResult> EditEmail(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var model = await _userProfileService.BuildEditEmailViewModelAsync(targetProfileId, requestingUserId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch
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
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profileId = string.IsNullOrWhiteSpace(model.ProfileId)
                    ? requestingUserId
                    : model.ProfileId;

                if (!ModelState.IsValid)
                {
                    model.ProfileId = profileId;
                    return View(model);
                }

                var result = await _userProfileService.UpdateEmailAsync(profileId, model, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Email updated successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your email is unchanged.";
                        model.ProfileId = profileId;
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.Failed:
                    default:
                        ModelState.AddModelError(string.Empty, "Email update failed. Please try again.");
                        model.ProfileId = profileId;
                        return View(model);
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditFullName(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var model = await _userProfileService.BuildEditFullNameViewModelAsync(targetProfileId, requestingUserId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch
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
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profileId = string.IsNullOrWhiteSpace(model.ProfileId)
                    ? requestingUserId
                    : model.ProfileId;

                if (!ModelState.IsValid)
                {
                    model.ProfileId = profileId;
                    return View(model);
                }

                var result = await _userProfileService.UpdateFullNameAsync(profileId, model, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Full name updated successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your full name is unchanged.";
                        model.ProfileId = profileId;
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPhoneNumber(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var model = await _userProfileService.BuildEditPhoneNumberViewModelAsync(targetProfileId, requestingUserId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch
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
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profileId = string.IsNullOrWhiteSpace(model.ProfileId)
                    ? requestingUserId
                    : model.ProfileId;

                if (!ModelState.IsValid)
                {
                    model.ProfileId = profileId; 
                    return View(model);
                }

                var result = await _userProfileService.UpdatePhoneNumberAsync(profileId, model, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Phone number updated successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Phone number is unchanged.";
                        model.ProfileId = profileId;
                        return View(model);

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAddress(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var model = await _userProfileService.BuildEditAddressViewModelAsync(targetProfileId, requestingUserId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch
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
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profileId = string.IsNullOrWhiteSpace(model.ProfileId)
                    ? requestingUserId
                    : model.ProfileId;

                if (!ModelState.IsValid)
                {
                    model.ProfileId = profileId; 
                    return View(model);
                }

                var result = await _userProfileService.UpdateAddressAsync(profileId, model, requestingUserId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Address updated successfully!";
                        return RedirectToAction(nameof(ViewProfile), new { profileId });

                    case ServiceOperationResult.NoChange:
                        TempData["InfoMessage"] = "Your address is unchanged.";
                        model.ProfileId = profileId;
                        return View(model);

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? profileId)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var targetProfileId = profileId ?? requestingUserId;

                var profile = await _userProfileService.GetProfileByIdAsync(targetProfileId, requestingUserId);
                if (profile == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });
                }

                var vm = new DeleteUserProfileViewModel
                {
                    ProfileId = targetProfileId,
                    FullName = profile.FullName,
                    Email = profile.User?.Email ?? string.Empty
                };

                return View(vm);
            }
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(DeleteUserProfileViewModel model)
        {
            try
            {
                var requestingUserId = GetUserId();
                if (string.IsNullOrEmpty(requestingUserId))
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var profileId = string.IsNullOrWhiteSpace(model.ProfileId)
                    ? requestingUserId
                    : model.ProfileId;

                var result = await _userProfileService.DeleteUserProfileAsync(profileId, requestingUserId);

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
            catch
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }
    }
}
