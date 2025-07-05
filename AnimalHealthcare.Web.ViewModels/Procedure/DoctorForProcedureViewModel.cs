namespace AnimalHealthcare.Web.ViewModels.Procedure
{
    public class DoctorForProcedureViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
    }
}
