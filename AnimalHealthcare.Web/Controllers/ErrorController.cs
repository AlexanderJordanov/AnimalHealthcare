using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    /// <summary>
    /// Handles application-level error routing and displays appropriate error views based on status codes.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Handles unhandled exceptions by displaying the generic internal server error view.
        /// </summary>
        /// <returns>The <c>Error.cshtml</c> view for HTTP 500 errors.</returns>
        [Route("Error")]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View(); // Views/Error/Error.cshtml
        }

        /// <summary>
        /// Handles known status codes (400, 401, 403, 404, etc.) and returns user-friendly error pages.
        /// </summary>
        /// <param name="code">The HTTP status code that triggered the error handler.</param>
        /// <returns>A view corresponding to the status code.</returns>
        [Route("Error/HandleStatusCode")]
        public IActionResult HandleStatusCode(int code = 404)
        {
            Response.StatusCode = code;

            return code switch
            {
                400 => ReturnWithMessage("Bad request. Please check your input.", "BadRequest"),
                401 => ReturnWithMessage("Unauthorized access. Please log in to continue.", "Unauthorized"),
                403 => ReturnWithMessage("You are not authorized to access this resource.", "Forbidden"),
                404 => ReturnWithMessage("The page you're looking for was not found.", "NotFound"),
                500 => ReturnWithMessage($"Unexpected error.", "Error")
            };

            /// <summary>
            /// Helper method to attach a user-friendly error message and return the appropriate view.
            /// </summary>
            /// <param name="message">The message to display in the view.</param>
            /// <param name="viewName">The name of the view to render.</param>
            /// <returns>A <see cref="ViewResult"/> with the specified error message.</returns>
            ViewResult ReturnWithMessage(string message, string viewName)
            {
                ViewBag.ErrorMessage = message;
                return View(viewName);
            }
        }
    }
}
