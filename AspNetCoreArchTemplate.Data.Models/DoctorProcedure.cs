namespace AnimalHealthcare.Data.Models
{
    public class DoctorProcedure
    {
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int ProcedureId { get; set; }
        public Procedure Procedure { get; set; } = null!;
    }
}
