namespace AnimalHealthcare.Web.ViewModels.Animal
{
    public class AnimalAppointmentViewModel
    {
        public DateTime AppointmentDateTime { get; set; }
        public string DoctorName { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string ProcedureName { get; set; } = null!;
    }
}
