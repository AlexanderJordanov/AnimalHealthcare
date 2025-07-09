using AnimalHealthcare.Services.Core;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace AnimalHealthcare.Web.Controllers
{
    public class AppointmentController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IProcedureService _procedureService;

        public AppointmentController(IAppointmentService appointmentService, IDoctorService doctorService, IProcedureService procedureService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _procedureService = procedureService;
        }

        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByUserIdAsync(userId);

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? doctorId = null, int? procedureId = null)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _appointmentService.BuildCreateAppointmentViewModelAsync(userId, doctorId, procedureId);

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                // Rebuild dropdowns in case of validation failure
                var rebuiltModel = await _appointmentService.BuildCreateAppointmentViewModelAsync(
                    userId,
                    model.DoctorId != 0 ? model.DoctorId : null,
                    model.ProcedureId != 0 ? model.ProcedureId : null
                );

                // Keep selected values
                rebuiltModel.AnimalId = model.AnimalId;
                rebuiltModel.DoctorId = model.DoctorId;
                rebuiltModel.ProcedureId = model.ProcedureId;
                rebuiltModel.Date = model.Date;
                rebuiltModel.TimeSlot = model.TimeSlot;

                // Populate time slots manually
                if (model.DoctorId != 0 && model.Date != default)
                {
                    rebuiltModel.TimeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(model.DoctorId, model.Date);
                }

                return View(rebuiltModel);
            }

            var success = await _appointmentService.CreateAppointmentAsync(model, userId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to create appointment. Please try again.";
                return RedirectToAction("Create");
            }

            TempData["SuccessMessage"] = "Appointment created successfully!";
            return RedirectToAction("MyAppointments");
        }


        [HttpGet]
        public async Task<IActionResult> GetDoctorsByProcedure(int procedureId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var doctors = await _doctorService.GetDoctorsByProcedureAsync(procedureId);
            return Json(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var timeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, date);
            return Json(timeSlots);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _appointmentService.GetAppointmentDetailsAsync(id, userId);
            if (model == null) return Forbid();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var model = await _appointmentService.BuildCancelAppointmentViewModelAsync(id, userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int appointmentId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _appointmentService.CancelAppointmentAsync(appointmentId, userId);
            if (!success) return Forbid();

            TempData["SuccessMessage"] = "Appointment successfully canceled.";
            return RedirectToAction("MyAppointments");
        }
    }
}
