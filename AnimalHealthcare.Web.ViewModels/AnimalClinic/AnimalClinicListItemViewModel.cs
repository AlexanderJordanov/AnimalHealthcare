namespace AnimalHealthcare.Web.ViewModels.AnimalClinic
{
    public class AnimalClinicListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;
    }
}
