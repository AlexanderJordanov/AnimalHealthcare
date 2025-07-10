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
                400 => ReturnWithMessage("Bad request. Please check your input.", "Error"), // Bad request
                401 => ReturnWithMessage("Unauthorized access. Please log in to continue.", "Error"), // Unauthorized
                403 => ReturnWithMessage("You are not authorized to access this resource.", "Error"), // Forbidden
                404 => ReturnWithMessage("The page you're looking for was not found.", "NotFound"), //Not found
                _ => ReturnWithMessage($"Unexpected error (Status Code: {code}).", "Error") // Other errors
            };

            ViewResult ReturnWithMessage(string message, string viewName)
            {
                ViewBag.ErrorMessage = message;
                return View(viewName);
            }
        }
    }
}
