using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnimalHealthcare.Web.ViewModels.Appointment
{
    public class CreateAppointmentViewModel
    {
        [Required]
        [Display(Name = "Select Pet")]
        public int AnimalId { get; set; }

        [Required]
        [Display(Name = "Select Procedure")]
        public int ProcedureId { get; set; }

        [Required]
        [Display(Name = "Select Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Select Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Select Time")]
        public string TimeSlot { get; set; } = null!;

        // For dropdown population
        public IEnumerable<SelectListItem> UserPets { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Procedures { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TimeSlots { get; set; } = new List<SelectListItem>();

        public DateTime MinDate => DateTime.Today;
        public DateTime MaxDate => DateTime.Today.AddMonths(6);
    }
}
