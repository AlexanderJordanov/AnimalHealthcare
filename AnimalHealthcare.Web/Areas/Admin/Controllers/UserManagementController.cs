using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Areas.Admin.Controllers
{    
    public class UserManagementController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
