namespace AnimalHealthcare.Data.Models
{
    public class Procedure
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = null!;
        
        public string Description { get; set; } = null!;
        
        // Indicates if the procedure is deleted (soft delete)
        public bool IsDeleted { get; set; } = false;
        // Navigation property for related doctor procedures
        public ICollection<DoctorProcedure> DoctorProcedures { get; set; } = new HashSet<DoctorProcedure>();
    }
}
