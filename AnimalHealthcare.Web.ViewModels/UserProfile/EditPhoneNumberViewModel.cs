using System.ComponentModel.DataAnnotations;

namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    using static AnimalHealthcare.GCommon.ValidationConstants;

    public class EditPhoneNumberViewModel
    {
        public string ProfileId { get; set; } = null!;

        [StringLength(PhoneNumberMaxLength, MinimumLength = PhoneNumberMinLength, ErrorMessage = StringLength)]
        [Phone(ErrorMessage = ErrorMessages.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
