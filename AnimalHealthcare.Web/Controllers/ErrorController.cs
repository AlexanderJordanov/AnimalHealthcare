using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Home/Error")]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View(); // Will use Views/Shared/Error.cshtml
        }

        [Route("Home/HandleStatusCode")]
        public IActionResult HandleStatusCode(int code)
        {
            Response.StatusCode = code;

            return code switch
            {
                404 => View("NotFound", ViewBagWith("The page you're looking for was not found.")),
                400 => View("Error", ViewBagWith("Bad request. Please check your input.")),
                _ => View("Error", ViewBagWith($"Unexpected error (Status Code: {code})."))
            };

            ViewResult ViewBagWith(string message)
            {
                ViewBag.ErrorMessage = message;
                return View();
            }
        }
    }
}
