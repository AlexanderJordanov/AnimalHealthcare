namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string DoctorName { get; set; } = null!;
        public string ProcedureName { get; set; } = null!;
    }
}
