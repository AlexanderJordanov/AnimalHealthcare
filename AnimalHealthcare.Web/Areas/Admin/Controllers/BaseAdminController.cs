using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimalHealthcare.Web.Areas.Admin.Controllers
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;

    [Area(AdminRole)]
    [Authorize(Roles = AdminRole)]
    public class BaseAdminController : Controller
    {
        private bool IsUserAuthenticated()
        {
            return this.User.Identity?.IsAuthenticated ?? false;
        }

        protected string? GetUserId()
        {
            string? userId = null;

            bool isAuthenticated = this.IsUserAuthenticated();

            if (isAuthenticated)
            {
                userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            return userId;
        }
    }
}
