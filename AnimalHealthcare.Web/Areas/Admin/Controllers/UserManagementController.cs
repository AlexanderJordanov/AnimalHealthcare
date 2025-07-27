using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
