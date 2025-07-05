using AnimalHealthcare.Services.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimalHealthcare.Web.Controllers
{
    public class ProcedureController : BaseController
    {
        private readonly IProcedureService _procedureService;

        public ProcedureController(IProcedureService procedureService)
        {
            _procedureService = procedureService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var procedures = await _procedureService.GetAllProceduresAsync();
            return View(procedures);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _procedureService.GetProcedureDetailsAsync(id);

            if (model == null)
            {
                return RedirectToAction("HandleStatusCode", "Error", new { code = 404 });
            }

            return View(model);
        }
    }
}
