using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class AppointmentController : BaseController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<IActionResult> MyAppointments()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByUserIdAsync(userId);

            return View(appointments);
        }
    }
}
