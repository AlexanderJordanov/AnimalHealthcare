namespace AnimalHealthcare.Web.ViewModels.UserManagement
{
    public class AdminCancelAppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string AnimalName { get; set; } = null!;
        public int AnimalId { get; set; }
    }
}
