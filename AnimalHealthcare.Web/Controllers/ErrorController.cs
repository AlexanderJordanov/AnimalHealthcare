using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Home/Error")]
        public IActionResult Error()
        {
            return View(); // Will use Views/Shared/Error.cshtml
        }

        [Route("Home/HandleStatusCode")]
        public IActionResult HandleStatusCode(int code)
        {
            if (code == 404)
            {
                ViewBag.ErrorMessage = "The page you're looking for was not found.";
                return View("NotFound");
            }
            if (code == 400)
            {
                ViewBag.ErrorMessage = "Bad request. Please check your input.";
                return View("Error");
            }

            ViewBag.ErrorMessage = $"Unexpected error (Status Code: {code}).";
            return View("Error");
        }
    }
}
