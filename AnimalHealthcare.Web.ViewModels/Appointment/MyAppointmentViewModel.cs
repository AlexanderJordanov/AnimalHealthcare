namespace AnimalHealthcare.Web.ViewModels.Appointment
{
    public class MyAppointmentViewModel
    {
        public int Id { get; set; } // Appointment ID
        public string PetName { get; set; } = null!;
        public string ProcedureName { get; set; } = null!;
        public DateTime AppointmentDateTime { get; set; }
    }
}
