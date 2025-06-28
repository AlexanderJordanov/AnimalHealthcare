using AnimalHealthcare.GCommon.Enums;
using System.ComponentModel.DataAnnotations;
using static AnimalHealthcare.GCommon.ValidationConstants;

namespace AnimalHealthcare.Web.ViewModels.Animal
{
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    using static AnimalHealthcare.GCommon.ValidationConstants.Animal;
    public class EditPetViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = Required)]
        [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessage = StringLength)]
        public string Name { get; set; } = null!;

        [Range(AgeMin, AgeMax, ErrorMessage = "{0} must be between {1} and {2}.")]
        public int Age { get; set; }

        [Required(ErrorMessage = Required)]
        [StringLength(SpeciesMaxLength, MinimumLength = SpeciesMinLength, ErrorMessage = StringLength)]
        public string Species { get; set; } = null!;

        [Required(ErrorMessage = Required)]
        [StringLength(BreedMaxLength, MinimumLength = BreedMinLength, ErrorMessage = StringLength)]
        public string Breed { get; set; } = null!;

        [Required(ErrorMessage = Required)]
        [Display(Name = "Gender")]
        public AnimalGender Gender { get; set; }
    }
}
