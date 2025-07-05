namespace AnimalHealthcare.Web.ViewModels.Procedure
{
    public class ProcedureDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<DoctorForProcedureViewModel> Doctors { get; set; } = new List<DoctorForProcedureViewModel>();
    }
}
