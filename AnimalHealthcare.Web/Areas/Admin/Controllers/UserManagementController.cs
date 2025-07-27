using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimalHealthcare.Web.Areas.Admin.Controllers
{    
    public class UserManagementController : BaseAdminController
    {
        private readonly IUserProfileService userProfileService;
        private readonly IUserManagementService userManagementService;

        public UserManagementController(IUserProfileService userProfileService, IUserManagementService userManagementService)
        {
            this.userProfileService = userProfileService;
            this.userManagementService = userManagementService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AllProfiles()
        {
            var adminId = GetUserId();

            var profile = await userProfileService.GetProfileByIdAsync(adminId, adminId);
            if (profile == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            var users = await userManagementService.GetAllUserProfilesAsync(adminId);

            var viewModel = new AdminProfilesViewModel
            {
                FullName = profile.FullName,
                Email = profile.User.Email!,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                Users = users
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var model = await userManagementService.GetUserDetailsAsync(id);

            if (model == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            var user = await userManagementService.GetUserBasicInfoAsync(id);
            if (user == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await userManagementService.DeleteUserAsync(id);
            if (!success)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }

            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(AllProfiles));
        }

        [HttpGet]
        public async Task<IActionResult> AnimalDetails(int id)
        {
            var viewModel = await userManagementService.GetAnimalWithAppointmentsAsync(id);

            if (viewModel == null)
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UnregisterAnimal(int id)
        {
            var viewModel = await userManagementService.GetUnregisterAnimalViewModelAsync(id);
            if (viewModel == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterAnimalConfirmed(int id)
        {
            var success = await userManagementService.UnregisterAnimalAsync(id);

            if (!success)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 400 });
            }

            var userId = userManagementService.GetAnimalOwnerId(id);
            if (userId == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            TempData["SuccessMessage"] = "Animal unregistered successfully.";
            return RedirectToAction("Details", "UserManagement", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var viewModel = await userManagementService.GetAppointmentDetailsAsync(id);

            if (viewModel == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var viewModel = await userManagementService.GetCancelAppointmentViewModelAsync(id);
            if (viewModel == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointmentConfirmed(int id)
        {
            var viewModel = await userManagementService.GetCancelAppointmentViewModelAsync(id);
            if (viewModel == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            var success = await userManagementService.CancelAppointmentAsync(id);
            if (!success)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 400 });
            }

            TempData["SuccessMessage"] = "Appointment canceled successfully.";
            return RedirectToAction("AnimalDetails", new { id = viewModel.AnimalId });
        }

    }
}
