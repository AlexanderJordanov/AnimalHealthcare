using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnimalHealthcare.Web.ViewModels.Doctor
{
    public class DoctorListViewModel
    {
        public IEnumerable<DoctorListItemViewModel> Doctors { get; set; } = new List<DoctorListItemViewModel>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        // Optional: for future sorting
        public string? CurrentSort { get; set; }

        public string? CurrentFilter { get; set; }
        public IEnumerable<SelectListItem> AvailableFilters { get; set; } = new List<SelectListItem>();
    }
}
