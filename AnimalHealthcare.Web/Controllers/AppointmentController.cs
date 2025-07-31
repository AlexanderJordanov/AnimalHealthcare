using AnimalHealthcare.GCommon.Enums;
using AnimalHealthcare.Services.Core.Contracts;
using AnimalHealthcare.Web.ViewModels.Appointment;
using Microsoft.AspNetCore.Mvc;



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
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var appointments = await _appointmentService.GetAppointmentsByUserIdAsync(userId);

                return View(appointments);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? doctorId = null, int? procedureId = null)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _appointmentService.BuildCreateAppointmentViewModelAsync(userId, doctorId, procedureId);

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                if (!ModelState.IsValid)
                {
                    // Rebuild dropdowns if validation fails
                    var rebuiltModel = await _appointmentService.BuildCreateAppointmentViewModelAsync(
                        userId,
                        model.DoctorId != 0 ? model.DoctorId : null,
                        model.ProcedureId != 0 ? model.ProcedureId : null
                    );

                    // Preserve user selections
                    rebuiltModel.AnimalId = model.AnimalId;
                    rebuiltModel.DoctorId = model.DoctorId;
                    rebuiltModel.ProcedureId = model.ProcedureId;
                    rebuiltModel.Date = model.Date;
                    rebuiltModel.TimeSlot = model.TimeSlot;

                    // Load available time slots
                    if (model.DoctorId != 0 && model.Date != default)
                    {
                        rebuiltModel.TimeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(model.DoctorId, model.Date);
                    }

                    return View(rebuiltModel);
                }

                // Call service method and handle result enum
                var result = await _appointmentService.CreateAppointmentAsync(model, userId);

                switch (result)
                {
                    case AppointmentCreationResult.Success:
                        TempData["SuccessMessage"] = "Appointment created successfully!";
                        return RedirectToAction("MyAppointments");

                    case AppointmentCreationResult.PetNotFound:
                        TempData["ErrorMessage"] = "Selected pet not found or not owned by you.";
                        break;

                    case AppointmentCreationResult.DoctorProcedureMismatch:
                        TempData["ErrorMessage"] = "The selected doctor is not authorized to perform the selected procedure.";
                        break;

                    case AppointmentCreationResult.InvalidTimeSlotFormat:
                        TempData["ErrorMessage"] = "Invalid time format selected. Please choose a valid time slot.";
                        break;

                    case AppointmentCreationResult.SlotAlreadyBooked:
                        TempData["ErrorMessage"] = "The selected time slot has already been booked.";
                        break;

                    case AppointmentCreationResult.SlotDuringLunch:
                        TempData["ErrorMessage"] = "The 12:00 time slot is unavailable due to lunch break.";
                        break;

                    default:
                        TempData["ErrorMessage"] = "An unknown error occurred. Please try again.";
                        break;
                }

                // On failure, redirect back to Create view
                return RedirectToAction("Create", new
                {
                    doctorId = model.DoctorId,
                    procedureId = model.ProcedureId
                });
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetDoctorsByProcedure(int procedureId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var doctors = await _doctorService.GetDoctorsByProcedureAsync(procedureId);
                return Json(doctors);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var timeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, date);
                return Json(timeSlots);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }


                var model = await _appointmentService.GetAppointmentDetailsAsync(id, userId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var model = await _appointmentService.BuildCancelAppointmentViewModelAsync(id, userId);
                if (model == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int appointmentId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                {
                    return RedirectToAction("HandleStatusCode", "Error", new { code = 401 });
                }

                var result = await _appointmentService.CancelAppointmentAsync(appointmentId, userId);

                switch (result)
                {
                    case ServiceOperationResult.Success:
                        TempData["SuccessMessage"] = "Appointment successfully canceled.";
                        return RedirectToAction("MyAppointments");

                    case ServiceOperationResult.NotFound:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });

                    case ServiceOperationResult.Unauthorized:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 403 });

                    default:
                        return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 500 });
            }
        }

    }
}
