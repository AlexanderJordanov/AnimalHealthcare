using System.ComponentModel.DataAnnotations;

namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    public class EditFullNameViewModel
    {
        [Required(ErrorMessage = Required)]
        [StringLength(FullNameMaxLength, MinimumLength = FullNameMinLength, ErrorMessage = StringLength)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;
    }
}
