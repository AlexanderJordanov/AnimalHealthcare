using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Home/Error")]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View(); // Views/Error/Error.cshtml
        }

        [Route("Home/HandleStatusCode")]
        public IActionResult HandleStatusCode(int code)
        {
            Response.StatusCode = code;

            return code switch
            {
                400 => ReturnWithMessage("Bad request. Please check your input.", "BadRequest"),
                401 => ReturnWithMessage("Unauthorized access. Please log in to continue.", "Unauthorized"),
                403 => ReturnWithMessage("You are not authorized to access this resource.", "Forbidden"),
                404 => ReturnWithMessage("The page you're looking for was not found.", "NotFound"),
                _ => ReturnWithMessage($"Unexpected error (Status Code: {code}).", "Error")
            };

            ViewResult ReturnWithMessage(string message, string viewName)
            {
                ViewBag.ErrorMessage = message;
                return View(viewName);
            }
        }
    }
}
