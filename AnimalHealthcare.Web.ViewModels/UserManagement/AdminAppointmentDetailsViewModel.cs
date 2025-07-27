namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminAppointmentDetailsViewModel
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }

        public string AnimalName { get; set; } = null!;
        public DateTime AppointmentDateTime { get; set; }

        public string DoctorName { get; set; } = null!;
        public string DoctorSpecialization { get; set; } = null!;
        public string ClinicName { get; set; } = null!;

        public string ProcedureName { get; set; } = null!;
        public string ProcedureDescription { get; set; } = null!;
    }
}
