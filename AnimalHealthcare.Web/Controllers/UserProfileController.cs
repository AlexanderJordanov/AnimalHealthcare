using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserProfile;
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _userProfileService.UpdateProfilePictureAsync(userId, null);
            TempData["SuccessMessage"] = "Profile picture removed successfully!";
            return RedirectToAction(nameof(ViewProfile));
        }

        [HttpGet]
        public async Task<IActionResult> EditEmail()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _userProfileService.BuildEditEmailViewModelAsync(userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmail(EditEmailViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, unchanged) = await _userProfileService.UpdateEmailAsync(userId, model);

            if (!success)
            {
                // Optional: log or handle failure
                ModelState.AddModelError("", "Email update failed. Please try again.");
                return View(model);
            }

            if (unchanged)
            {
                TempData["InfoMessage"] = "Your email is unchanged.";
                return View(model);
            }
            else
            {
                TempData["SuccessMessage"] = "Email updated successfully!";
                return RedirectToAction(nameof(ViewProfile));
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditFullName()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _userProfileService.BuildEditFullNameViewModelAsync(userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFullName(EditFullNameViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _userProfileService.UpdateFullNameAsync(userId, model);

            if (!updated)
            {
                TempData["InfoMessage"] = "Your full name is unchanged.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Full name updated successfully!";
            return RedirectToAction(nameof(ViewProfile));
        }

        [HttpGet]
        public async Task<IActionResult> EditPhoneNumber()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _userProfileService.BuildEditPhoneNumberViewModelAsync(userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoneNumber(EditPhoneNumberViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid) return View(model);

            try
            {
                await _userProfileService.UpdatePhoneNumberAsync(userId, model);
                TempData["SuccessMessage"] = "Phone number updated successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["InfoMessage"] = ex.Message;
                return View(model);
            }

            return RedirectToAction(nameof(ViewProfile));
        }

        [HttpGet]
        public async Task<IActionResult> EditAddress()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _userProfileService.BuildEditAddressViewModelAsync(userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(EditAddressViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var unchanged = await _userProfileService.UpdateAddressAsync(userId, model);
            if (unchanged)
            {
                TempData["InfoMessage"] = "Your address is unchanged.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Address updated successfully!";
            return RedirectToAction(nameof(ViewProfile));
        }
    }
}
