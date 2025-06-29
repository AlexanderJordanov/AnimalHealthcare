namespace AnimalHealthcare.Web.ViewModels.Appointment
{
    public class CancelAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string PetName { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public DateTime AppointmentTime { get; set; }
    }
}
