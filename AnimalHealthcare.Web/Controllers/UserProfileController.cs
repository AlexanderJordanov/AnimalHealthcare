using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
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
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _userProfileService.GetProfileByIdAsync(userId);
            var animals = await _animalService.GetAnimalSummariesByOwnerIdAsync(userId);
            var model = _userProfileService.BuildUserProfileViewModel(profile, animals);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProfilePicture(string profilePictureUrl)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!Uri.TryCreate(profilePictureUrl, UriKind.Absolute, out var uriResult)
                || !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("", "Please provide a valid URL.");
                return RedirectToAction(nameof(ViewProfile));
            }

            await _userProfileService.UpdateProfilePictureAsync(userId, profilePictureUrl);
            TempData["SuccessMessage"] = "Profile picture updated successfully!";
            return RedirectToAction(nameof(ViewProfile));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _userProfileService.UpdateProfilePictureAsync(userId, null);
            TempData["SuccessMessage"] = "Profile picture removed successfully!";
            return RedirectToAction(nameof(ViewProfile));
        }
    }
}
