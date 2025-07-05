namespace AnimalHealthcare.Web.ViewModels.AnimalClinic
{
    public class AnimalClinicListViewModel
    {
        public IEnumerable<AnimalClinicListItemViewModel> Clinics { get; set; } = new List<AnimalClinicListItemViewModel>();
    }
}
